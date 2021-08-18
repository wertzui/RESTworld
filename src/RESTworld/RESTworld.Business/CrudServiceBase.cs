using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RESTworld.Business.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RESTworld.Business
{
    /// <inheritdoc/>
    public class CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        : ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
        where TUpdateDto : DtoBase
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected readonly IDbContextFactory<TContext> _contextFactory;
        protected readonly ILogger<CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> _logger;
        protected readonly IMapper _mapper;
        protected readonly IEnumerable<ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> _authorizationHandlers;
        protected readonly IUserAccessor _userAccessor;
        protected static bool _databaseIsMigratedToLatestVersion;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Creates a new instance of the <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/> class.
        /// </summary>
        /// <param name="contextFactory">The factory used to create a <see cref="DbContext"/>.</param>
        /// <param name="mapper">The AutoMapper instance which maps between DTOs and entities.</param>
        /// <param name="authorizationHandlers">All AuthorizationHandlers which will be called during authorization.</param>
        /// <param name="userAccessor">The user accessor which gets the user of the current request.</param>
        /// <param name="logger">The logger.</param>
        public CrudServiceBase(
            IDbContextFactory<TContext> contextFactory,
            IMapper mapper,
            IEnumerable<ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> authorizationHandlers,
            IUserAccessor userAccessor,
            ILogger<CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _authorizationHandlers = authorizationHandlers ?? throw new ArgumentNullException(nameof(authorizationHandlers));
            _userAccessor = userAccessor;
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (System.Linq.Enumerable.Any(_authorizationHandlers))
                _logger.LogWarning($"No {nameof(ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>)}<{typeof(TEntity).Name}, {typeof(TCreateDto).Name}, {typeof(TGetListDto).Name}, {typeof(TGetFullDto).Name}, {typeof(TUpdateDto).Name}> is configured. No authorization will be performed for any methods of {nameof(CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>)}<{typeof(TEntity).Name}, {typeof(TCreateDto).Name}, {typeof(TGetListDto).Name}, {typeof(TGetFullDto).Name}, {typeof(TUpdateDto).Name}>.");
        }

        /// <inheritdoc/>
        public Task<ServiceResponse<TGetFullDto>> CreateAsync(TCreateDto dto)
            => TryExecuteWithAuthorizationAsync(
                dto,
                param1 => CreateInternalAsync(param1),
                (result, handler) => handler.HandleCreateRequestAsync(result),
                (response, handler) => handler.HandleCreateResponseAsync(response));

        /// <inheritdoc/>
        public Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> CreateAsync(IReadOnlyCollection<TCreateDto> dtos)
            => TryExecuteWithAuthorizationAsync(
                dtos,
                param1 => CreateInternalAsync(param1),
                (result, handler) => handler.HandleCreateRequestAsync(result),
                (response, handler) => handler.HandleCreateResponseAsync(response));

        /// <inheritdoc/>
        public Task<ServiceResponse<object>> DeleteAsync(long id, byte[] timestamp)
            => TryExecuteWithAuthorizationAsync(
                id,
                timestamp,
                (param1, param2) => DeleteInternalAsync(param1, param2),
                (result, handler) => handler.HandleDeleteRequestAsync(result),
                (response, handler) => handler.HandleDeleteResponseAsync(response));

        /// <inheritdoc/>
        public Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> GetListAsync(IGetListRequest<TEntity> request)
            => TryExecuteWithAuthorizationAsync(
                request,
                param1 => GetListInternalAsync(param1),
                (result, handler) => handler.HandleGetListRequestAsync(result),
                (response, handler) => handler.HandleGetListResponseAsync(response));

        /// <inheritdoc/>
        public Task<ServiceResponse<TGetFullDto>> GetSingleAsync(long id)
            => TryExecuteWithAuthorizationAsync(
                id,
                param1 => GetSingleInternalAsync(param1),
                (result, handler) => handler.HandleGetSingleRequestAsync(result),
                (response, handler) => handler.HandleGetSingleResponseAsync(response));

        /// <inheritdoc/>
        public Task<ServiceResponse<TGetFullDto>> UpdateAsync(TUpdateDto dto)
            => TryExecuteWithAuthorizationAsync(
                dto,
                param1 => UpdateInternalAsync(param1),
                (result, handler) => handler.HandleUpdateRequestAsync(result),
                (response, handler) => handler.HandleUpdateResponseAsync(response));

        /// <inheritdoc/>
        public Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> UpdateAsync(IUpdateMultipleRequest<TUpdateDto, TEntity> request)
            => TryExecuteWithAuthorizationAsync(
                request,
                param1 => UpdateInternalAsync(param1),
                (result, handler) => handler.HandleUpdateRequestAsync(result),
                (response, handler) => handler.HandleUpdateResponseAsync(response));

        /// <summary>
        /// The method contains the business logic for the CREATE operation.
        /// It will insert the given DTO into the database.
        /// </summary>
        /// <param name="dto">The DTO to insert into the database.</param>
        /// <returns>A response containing the DTO as it is in the database.</returns>
        protected virtual async Task<ServiceResponse<TGetFullDto>> CreateInternalAsync(TCreateDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);

            await using var context = _contextFactory.CreateDbContext();
            context.Add(entity);
            await context.SaveChangesAsync(GetCurrentUsersName());

            var resultDto = _mapper.Map<TGetFullDto>(entity);

            return ServiceResponse.FromResult(resultDto);
        }

        /// <summary>
        /// The method contains the business logic for the CREATE operation.
        /// It will insert the given DTOs into the database.
        /// </summary>
        /// <param name="dtos">The DTOs to insert into the database.</param>
        /// <returns>A response containing the DTOs as it is in the database.</returns>
        protected virtual async Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> CreateInternalAsync(IReadOnlyCollection<TCreateDto> dtos)
        {
            var entities = _mapper.Map<IReadOnlyCollection<TEntity>>(dtos);

            await using var context = _contextFactory.CreateDbContext();
            context.AddRange(entities);
            await context.SaveChangesAsync(GetCurrentUsersName());

            var resultDto = _mapper.Map<IReadOnlyCollection<TGetFullDto>>(entities);

            return ServiceResponse.FromResult(resultDto);
        }

        /// <summary>
        /// The method contains the business logic for the DELETE operation.
        /// It will delete the entity with the given ID and timestamp from the database.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="timestamp">The timestamp.</param>
        /// <returns>An empty 200 (Ok) response, or a problem response with either 404 (Not found) if the ID does not exist or 409 (Conflict) if the timestamp does not match.</returns>
        protected virtual async Task<ServiceResponse<object>> DeleteInternalAsync(long id, byte[] timestamp)
        {
            await using var context = _contextFactory.CreateDbContext();

            var entity = await context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
                return ServiceResponse.FromStatus<object>(HttpStatusCode.NotFound);

            if (!System.Linq.Enumerable.SequenceEqual(entity.Timestamp, timestamp))
                return ServiceResponse.FromProblem<object>(HttpStatusCode.Conflict, "The entity was modfied.");

            context.Remove(entity);

            await context.SaveChangesAsync(GetCurrentUsersName());

            return ServiceResponse.FromStatus<object>(HttpStatusCode.OK);
        }

        /// <summary>
        /// The method contains the business logic for the READ-List operation.
        /// It will return the items from the database as specified in the request.
        /// </summary>
        /// <param name="request">The request which contains a filter on which items to return. The filter is normally mapped from the OData parameters that where passed into the controller ($filter, $orderby, $top, $skip).</param>
        /// <returns>All items as specified in the request with paging options if specified.</returns>
        /// <exception cref="ArgumentException">If request.CalculateTotalCount is true, request.FilterForTotalCount must not be null.</exception>
        protected virtual async Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> GetListInternalAsync(IGetListRequest<TEntity> request)
        {
            var setForEntities = _contextFactory.Set<TEntity>();

            Task<long> totalPagesTask = null;
            long? totalCount = null;

            var tasks = new List<Task>(2);

            if (request is not null)
            {
                if (request.Filter is not null)
                    setForEntities = request.Filter(setForEntities);

                if (request.CalculateTotalCount)
                {
                    if (request.FilterForTotalCount is null)
                        throw new ArgumentException($"If {nameof(request)}.{nameof(request.CalculateTotalCount)} is true, {nameof(request)}.{nameof(request.FilterForTotalCount)} must not be null.", nameof(request));

                    var totalPagesSet = _contextFactory.Set<TEntity>();
                    totalPagesSet = request.FilterForTotalCount(totalPagesSet);
                    totalPagesTask = totalPagesSet.LongCountAsync();
                    tasks.Add(totalPagesTask);
                }
            }

            var entitiesTask = setForEntities.ToListAsync();
            tasks.Add(entitiesTask);

            await Task.WhenAll(tasks);

            var dtos = _mapper.Map<IReadOnlyCollection<TGetListDto>>(entitiesTask.Result);

            if (request.CalculateTotalCount)
                totalCount = totalPagesTask.Result;

            IReadOnlyPagedCollection<TGetListDto> pagedCollection = new ReadOnlyPagedCollection<TGetListDto>(dtos, totalCount);

            return ServiceResponse.FromResult(pagedCollection);
        }

        /// <summary>
        /// The method contains the business logic for the READ-Single operation.
        /// It will return the item with the specified ID from the database.
        /// </summary>
        /// <param name="id">The identifier of the item.</param>
        /// <returns>The item with the specified ID, or 404 (Not Found).</returns>
        protected virtual async Task<ServiceResponse<TGetFullDto>> GetSingleInternalAsync(long id)
        {
            var entity = await _contextFactory.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
                return ServiceResponse.FromStatus<TGetFullDto>(HttpStatusCode.NotFound);

            var dto = _mapper.Map<TGetFullDto>(entity);

            return ServiceResponse.FromResult(dto);
        }

        /// <summary>
        /// Tries to execute the given function while checking for errors and wrapping any exceptions into a problem ServiceResponse.
        /// It will first check for any pending database migrations. If there are any, a 503 (Service unavailable) is returned.
        /// If a <see cref="DbUpdateConcurrencyException"/> occurs during the execution, this will be wrapped into a 409 (Conflict) ServiceResponse.
        /// If any other exception occurs, this will be wrapped into a 500 (Internal server error) ServiceResponse.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <returns>Either the result of the call or a ServiceResponse describing the problem.</returns>
        protected virtual async Task<ServiceResponse<T>> TryExecuteAsync<T>(Func<Task<ServiceResponse<T>>> function)
        {
            try
            {
                if (!_databaseIsMigratedToLatestVersion)
                {
                    var pendingMigrations = await GetPendingMigrationsAsync();
                    _databaseIsMigratedToLatestVersion = !System.Linq.Enumerable.Any(pendingMigrations);
                    if (!_databaseIsMigratedToLatestVersion)
                        return ServiceResponse.FromProblem<T>(HttpStatusCode.ServiceUnavailable, $"The following migrations are still pending for {typeof(TContext).Name}:{Environment.NewLine}{string.Join(Environment.NewLine, pendingMigrations)}");
                }

                return await function();
            }
            catch (DbUpdateConcurrencyException e)
            {
                return ServiceResponse.FromException<T>(HttpStatusCode.Conflict, e);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while executing a service call");
                return ServiceResponse.FromException<T>(e);
            }
        }

        /// <summary>
        /// Gets all pending migrations if the database is relational.
        /// You may want to override this method if you do not want to execute migrations during application startup, but still want to access the database.
        /// </summary>
        /// <returns>All pending migrations.</returns>
        protected virtual async Task<IEnumerable<string>> GetPendingMigrationsAsync()
        {
            await using var context = _contextFactory.CreateDbContext();

            if (context.Database.IsRelational())
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                return pendingMigrations;
            }

            return Array.Empty<string>();
        }

        /// <summary>
        /// Tries to execute a <paramref name="function" /> which accepts one parameter while performing <paramref name="authorizeRequest" /> before and <paramref name="authorizeResult" /> after the main invocation.
        /// This method combines <see cref="TryExecuteAsync{T}(Func{Task{ServiceResponse{T}}})"/> with <see cref="WithAuthorizationAsync{T1, TResponse}(T1, Func{T1, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResult{TEntity, T1}, ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}, Task{AuthorizationResult{TEntity, T1}}}, Func{ServiceResponse{TResponse}, ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}, Task{ServiceResponse{TResponse}}})"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the parameter.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="param1">The first parameter.</param>
        /// <param name="function">The main function to execute.</param>
        /// <param name="authorizeRequest">The authorization request function which is executed before the main function.</param>
        /// <param name="authorizeResult">The authorization result function which is executed after the main function.</param>
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        /// <seealso cref="TryExecuteAsync{T}(Func{Task{ServiceResponse{T}}})" />
        /// <seealso cref="WithAuthorizationAsync{T1, TResponse}(T1, Func{T1, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResult{TEntity, T1}, ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}, Task{AuthorizationResult{TEntity, T1}}}, Func{ServiceResponse{TResponse}, ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}, Task{ServiceResponse{TResponse}}})"/>
        protected virtual Task<ServiceResponse<TResponse>> TryExecuteWithAuthorizationAsync<T1, TResponse>(
            T1 param1,
            Func<T1, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResult<TEntity, T1>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<AuthorizationResult<TEntity, T1>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<ServiceResponse<TResponse>>> authorizeResult)
            => TryExecuteAsync(() => WithAuthorizationAsync(param1, function, authorizeRequest, authorizeResult));


        /// <summary>
        /// Tries to execute a <paramref name="function" /> which accepts two parameters while performing <paramref name="authorizeRequest" /> before and <paramref name="authorizeResult" /> after the main invocation.
        /// This method combines <see cref="TryExecuteAsync{T}(Func{Task{ServiceResponse{T}}})"/> with <see cref="WithAuthorizationAsync{T1, T2, TResponse}(T1, T2, Func{T1, T2, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResult{TEntity, T1, T2}, ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}, Task{AuthorizationResult{TEntity, T1, T2}}}, Func{ServiceResponse{TResponse}, ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}, Task{ServiceResponse{TResponse}}})"/>.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="param1">The first parameter.</param>
        /// <param name="param2">The second parameter.</param>
        /// <param name="function">The main function to execute.</param>
        /// <param name="authorizeRequest">The authorization request function which is executed before the main function.</param>
        /// <param name="authorizeResult">The authorization result function which is executed after the main function.</param>
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        /// <seealso cref="TryExecuteAsync{T}(Func{Task{ServiceResponse{T}}})" />
        /// <seealso cref="WithAuthorizationAsync{T1, T2, TResponse}(T1, T2, Func{T1, T2, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResult{TEntity, T1, T2}, ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}, Task{AuthorizationResult{TEntity, T1, T2}}}, Func{ServiceResponse{TResponse}, ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}, Task{ServiceResponse{TResponse}}})"/>
        protected virtual Task<ServiceResponse<TResponse>> TryExecuteWithAuthorizationAsync<T1, T2, TResponse>(
            T1 param1,
            T2 param2,
            Func<T1, T2, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResult<TEntity, T1, T2>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<AuthorizationResult<TEntity, T1, T2>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<ServiceResponse<TResponse>>> authorizeResult)
            => TryExecuteAsync(() => WithAuthorizationAsync(param1, param2, function, authorizeRequest, authorizeResult));

        /// <summary>
        /// The method contains the business logic for the UPDATE-Single operation.
        /// It will update the entity in the database with the given DTO.
        /// </summary>
        /// <param name="dto">The DTO which contains updated values.</param>
        /// <returns>The item as it is stored in the database after the update operation.</returns>
        protected virtual async Task<ServiceResponse<TGetFullDto>> UpdateInternalAsync(TUpdateDto dto)
        {
            await using var context = _contextFactory.CreateDbContext();

            var entity = await context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == dto.Id);

            if (entity is null)
                return ServiceResponse.FromStatus<TGetFullDto>(HttpStatusCode.NotFound);

            context.Entry(entity).Property(nameof(EntityBase.Timestamp)).OriginalValue = dto.Timestamp;

            _mapper.Map(dto, entity);

            await context.SaveChangesAsync(GetCurrentUsersName());

            var resultDto = _mapper.Map<TGetFullDto>(entity);

            return ServiceResponse.FromResult(resultDto);
        }

        /// <summary>
        /// The method contains the business logic for the UPDATE-Multiple operation.
        /// It will update the entities in the database with the given DTOs.
        /// </summary>
        /// <param name="request">The DTOs which contain updated values.</param>
        /// <returns>The items as stored in the database after the update operation.</returns>
        protected virtual async Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> UpdateInternalAsync(IUpdateMultipleRequest<TUpdateDto, TEntity> request)
        {
            var dtos = request.Dtos;

            var ids = System.Linq.Enumerable.ToHashSet(System.Linq.Enumerable.Select(dtos, d => d.Id));
            await using var context = _contextFactory.CreateDbContext();

            var entities = await request.Filter(System.Linq.Queryable.Where(context.Set<TEntity>(), e => ids.Contains(e.Id)))
                .ToDictionaryAsync(e => e.Id);

            if (entities.Count != dtos.Count)
                return ServiceResponse.FromStatus<IReadOnlyCollection<TGetFullDto>>(HttpStatusCode.NotFound);

            foreach (var dto in dtos)
            {
                var entity = entities[dto.Id];
                context.Entry(entity).Property(nameof(EntityBase.Timestamp)).OriginalValue = dto.Timestamp;
                _mapper.Map(dto, entity);
            }

            await context.SaveChangesAsync(GetCurrentUsersName());

            var resultDto = _mapper.Map<IReadOnlyCollection<TGetFullDto>>(entities.Values);

            return ServiceResponse.FromResult(resultDto);
        }

        /// <summary>
        /// Executes a <paramref name="function" /> which accepts one parameter while performing <paramref name="authorizeRequest" /> before and <paramref name="authorizeResult" /> after the main invocation.
        /// </summary>
        /// <typeparam name="T1">The type of the parameter.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="param1">The first parameter.</param>
        /// <param name="function">The main function to execute.</param>
        /// <param name="authorizeRequest">The authorization request function which is executed before the main function.</param>
        /// <param name="authorizeResult">The authorization result function which is executed after the main function.</param>
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        protected virtual async Task<ServiceResponse<TResponse>> WithAuthorizationAsync<T1, TResponse>(
            T1 param1,
            Func<T1, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResult<TEntity, T1>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<AuthorizationResult<TEntity, T1>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<ServiceResponse<TResponse>>> authorizeResult)
        {

            var requestAuthResult = await AuthorizeRequestAsync(param1, authorizeRequest);

            if (requestAuthResult.Status != HttpStatusCode.OK)
                return ServiceResponse.FromStatus<TResponse>(requestAuthResult.Status);

            var serviceCallResponse = await function(requestAuthResult.Value1);

            var resultAuthResponse = await AuthorizeResultAsync(serviceCallResponse, authorizeResult);

            return resultAuthResponse;
        }

        /// <summary>
        /// Executes a <paramref name="function" /> which accepts two parameters while performing <paramref name="authorizeRequest" /> before and <paramref name="authorizeResult" /> after the main invocation.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="param1">The first parameter.</param>
        /// <param name="param2">The second parameter.</param>
        /// <param name="function">The main function to execute.</param>
        /// <param name="authorizeRequest">The authorization request function which is executed before the main function.</param>
        /// <param name="authorizeResult">The authorization result function which is executed after the main function.</param>
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        protected virtual async Task<ServiceResponse<TResponse>> WithAuthorizationAsync<T1, T2, TResponse>(
            T1 param1,
            T2 param2,
            Func<T1, T2, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResult<TEntity, T1, T2>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<AuthorizationResult<TEntity, T1, T2>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<ServiceResponse<TResponse>>> authorizeResult)
        {

            var requestAuthResult = await AuthorizeRequestAsync(param1, param2, authorizeRequest);

            if (requestAuthResult.Status != HttpStatusCode.OK)
                return ServiceResponse.FromStatus<TResponse>(requestAuthResult.Status);

            var serviceCallResponse = await function(requestAuthResult.Value1, requestAuthResult.Value2);

            var resultAuthResponse = await AuthorizeResultAsync(serviceCallResponse, authorizeResult);

            return resultAuthResponse;
        }

        /// <summary>
        /// Calls the Handle...RequestAsync for all <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>s with one parameter.
        /// </summary>
        /// <typeparam name="T1">The type of the parameter.</typeparam>
        /// <param name="param1">The parameter.</param>
        /// <param name="authorizeRequest">Defines which Handle...RequestAsync method to call.</param>
        /// <returns>The result of the authorization. May contain a failed authorization or a modified parameter.</returns>
        protected virtual async Task<AuthorizationResult<TEntity, T1>> AuthorizeRequestAsync<T1>(
            T1 param1,
            Func<AuthorizationResult<TEntity, T1>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<AuthorizationResult<TEntity, T1>>> authorizeRequest)
        {
            var result = AuthorizationResult.Ok<TEntity, T1>(param1);

            foreach (var handler in _authorizationHandlers)
            {
                result = await authorizeRequest(result, handler);
            }

            return result;
        }


        /// <summary>
        /// Calls the Handle...RequestAsync for all <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}" />s with two parameters.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <param name="param1">The first parameter.</param>
        /// <param name="param2">The second parameter.</param>
        /// <param name="authorizeRequest">Defines which Handle...RequestAsync method to call.</param>
        /// <returns>
        /// The result of the authorization. May contain a failed authorization or modified parameters.
        /// </returns>
        protected virtual async Task<AuthorizationResult<TEntity, T1, T2>> AuthorizeRequestAsync<T1, T2>(
            T1 param1,
            T2 param2,
            Func<AuthorizationResult<TEntity, T1, T2>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<AuthorizationResult<TEntity, T1, T2>>> authorizeRequest)
        {
            var result = AuthorizationResult.Ok<TEntity, T1, T2>(param1, param2);

            foreach (var handler in _authorizationHandlers)
            {
                result = await authorizeRequest(result, handler);
            }

            return result;
        }

        /// <summary>
        /// Calls the Handle...ResultAsync for all <see cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}" />s.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <param name="response">The response.</param>
        /// <param name="authorizeResult">Defines which Handle...ResultAsync method to call.</param>
        /// <returns>
        /// The result of the authorization. May contain a failed authorization or a modified result.
        /// </returns>
        protected virtual async Task<ServiceResponse<TResponse>> AuthorizeResultAsync<TResponse>(
            ServiceResponse<TResponse> response,
            Func<ServiceResponse<TResponse>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<ServiceResponse<TResponse>>> authorizeResult)
        {
            var result = response;

            foreach (var handler in _authorizationHandlers)
            {
                result = await authorizeResult(response, handler);
            }

            return result;

        }

        /// <summary>
        /// Gets the name of the current user which is used when calling <see cref="DbContextBase.SaveChangesAsync(string, System.Threading.CancellationToken)"/>.
        /// This is the user name which gets stored in the <see cref="ChangeTrackingEntityBase.CreatedBy"/> and <see cref="ChangeTrackingEntityBase.LastChangedBy"/> properties.
        /// </summary>
        /// <returns>The name of the current user as defined by the <see cref="IUserAccessor.User"/></returns>
        protected virtual string GetCurrentUsersName() => _userAccessor?.User?.Identity?.Name;
    }
}