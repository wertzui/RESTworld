using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RESTworld.Business.Authorization;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using RESTworld.Business.Services.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RESTworld.Business.Services
{
    /// <inheritdoc/>
    public class CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        : DbServiceBase<TContext>, ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
        where TUpdateDto : DtoBase
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected IEnumerable<ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> _authorizationHandlers;
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
            : base(contextFactory, mapper, userAccessor, logger)
        {
            _authorizationHandlers = authorizationHandlers ?? throw new ArgumentNullException(nameof(authorizationHandlers));

            if (!System.Linq.Enumerable.Any(_authorizationHandlers))
                _logger.LogWarning($"No {nameof(ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>)}<{typeof(TEntity).Name}, {typeof(TCreateDto).Name}, {typeof(TGetListDto).Name}, {typeof(TGetFullDto).Name}, {typeof(TUpdateDto).Name}> is configured. No authorization will be performed for any methods of {nameof(CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>)}<{typeof(TEntity).Name}, {typeof(TCreateDto).Name}, {typeof(TGetListDto).Name}, {typeof(TGetFullDto).Name}, {typeof(TUpdateDto).Name}>.");
        }

        /// <inheritdoc/>
        public Task<ServiceResponse<TGetFullDto>> CreateAsync(TCreateDto dto)
            => TryExecuteWithAuthorizationAsync<TEntity, TCreateDto, TGetFullDto, ICreateAuthorizationHandler<TEntity, TCreateDto, TGetFullDto>>(
                dto,
                result => CreateInternalAsync(result),
                (result, handler) => handler.HandleCreateRequestAsync(result),
                (response, handler) => handler.HandleCreateResponseAsync(response),
                _authorizationHandlers);

        /// <inheritdoc/>
        public Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> CreateAsync(IReadOnlyCollection<TCreateDto> dtos)
            => TryExecuteWithAuthorizationAsync<TEntity, IReadOnlyCollection<TCreateDto>, IReadOnlyCollection<TGetFullDto>, ICreateAuthorizationHandler<TEntity, TCreateDto, TGetFullDto>>(
                dtos,
                result => CreateInternalAsync(result),
                (result, handler) => handler.HandleCreateRequestAsync(result),
                (response, handler) => handler.HandleCreateResponseAsync(response),
                _authorizationHandlers);

        /// <inheritdoc/>
        public Task<ServiceResponse<object>> DeleteAsync(long id, byte[] timestamp)
            => TryExecuteWithAuthorizationAsync<TEntity, long, byte[], object, IDeleteAuthorizationHandler<TEntity>>(
                id,
                timestamp,
                result => DeleteInternalAsync(result),
                (result, handler) => handler.HandleDeleteRequestAsync(result),
                (response, handler) => handler.HandleDeleteResponseAsync(response),
                _authorizationHandlers);

        /// <inheritdoc/>
        public Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> GetListAsync(IGetListRequest<TEntity> request)
            => TryExecuteWithAuthorizationAsync<TEntity, IGetListRequest<TEntity>, IReadOnlyPagedCollection<TGetListDto>, IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>>(
                request,
                result => GetListInternalAsync(result),
                (result, handler) => handler.HandleGetListRequestAsync(result),
                (response, handler) => handler.HandleGetListResponseAsync(response),
                _authorizationHandlers);

        /// <inheritdoc/>
        public Task<ServiceResponse<TGetFullDto>> GetSingleAsync(long id)
            => TryExecuteWithAuthorizationAsync<TEntity, long, TGetFullDto, IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>>(
                id,
                result => GetSingleInternalAsync(result),
                (result, handler) => handler.HandleGetSingleRequestAsync(result),
                (response, handler) => handler.HandleGetSingleResponseAsync(response),
                _authorizationHandlers);

        /// <inheritdoc/>
        public Task<ServiceResponse<TGetFullDto>> UpdateAsync(TUpdateDto dto)
            => TryExecuteWithAuthorizationAsync<TEntity, TUpdateDto, TGetFullDto, IUpdateAuthorizationHandler<TEntity, TUpdateDto, TGetFullDto>>(
                dto,
                result => UpdateInternalAsync(result),
                (result, handler) => handler.HandleUpdateRequestAsync(result),
                (response, handler) => handler.HandleUpdateResponseAsync(response),
                _authorizationHandlers);

        /// <inheritdoc/>
        public Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> UpdateAsync(IUpdateMultipleRequest<TUpdateDto, TEntity> request)
            => TryExecuteWithAuthorizationAsync<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>, IReadOnlyCollection<TGetFullDto>, IUpdateAuthorizationHandler<TEntity, TUpdateDto, TGetFullDto>>(
                request,
                result => UpdateInternalAsync(result),
                (result, handler) => handler.HandleUpdateRequestAsync(result),
                (response, handler) => handler.HandleUpdateResponseAsync(response),
                _authorizationHandlers);

        /// <summary>
        /// The method contains the business logic for the CREATE operation.
        /// It will insert the given DTO into the database.
        /// </summary>
        /// <param name="authorizationResult">The result of the authorization which contains the DTO to insert into the database.</param>
        /// <returns>A response containing the DTO as it is in the database.</returns>
        protected virtual async Task<ServiceResponse<TGetFullDto>> CreateInternalAsync(AuthorizationResult<TEntity, TCreateDto> authorizationResult)
        {
            var dto = authorizationResult.Value1;

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
        /// <param name="authorizationResult">The result of the authorization which contains the DTOs to insert into the database.</param>
        /// <returns>A response containing the DTOs as it is in the database.</returns>
        protected virtual async Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> CreateInternalAsync(AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>> authorizationResult)
        {
            var dtos = authorizationResult.Value1;

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
        /// <param name="authorizationResult">The result of the authorization which contains the identifier and the timestamp.</param>
        /// <returns>An empty 200 (Ok) response, or a problem response with either 404 (Not found) if the ID does not exist or 409 (Conflict) if the timestamp does not match.</returns>
        protected virtual async Task<ServiceResponse<object>> DeleteInternalAsync(AuthorizationResult<TEntity, long, byte[]> authorizationResult)
        {
            var id = authorizationResult.Value1;
            var timestamp = authorizationResult.Value2;

            await using var context = _contextFactory.CreateDbContext();

            var entity = await context.Set<TEntity>().WithAuthorizationFilter(authorizationResult).FirstOrDefaultAsync(e => e.Id == id);

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
        /// <param name="authorizationResult">The result of the authorization which contains the request which contains a filter on which items to return. The filter is normally mapped from the OData parameters that where passed into the controller ($filter, $orderby, $top, $skip).</param>
        /// <returns>All items as specified in the request with paging options if specified.</returns>
        /// <exception cref="ArgumentException">If request.CalculateTotalCount is true, request.FilterForTotalCount must not be null.</exception>
        protected virtual async Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> GetListInternalAsync(AuthorizationResult<TEntity, IGetListRequest<TEntity>> authorizationResult)
        {
            var request = authorizationResult.Value1;

            var setForEntities = _contextFactory.Set<TEntity>().WithAuthorizationFilter(authorizationResult);

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

                    var totalPagesSet = _contextFactory.Set<TEntity>().WithAuthorizationFilter(authorizationResult);
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
        /// <param name="authorizationResult">TThe result of the authorization which contains thehe identifier of the item.</param>
        /// <returns>The item with the specified ID, or 404 (Not Found).</returns>
        protected virtual async Task<ServiceResponse<TGetFullDto>> GetSingleInternalAsync(AuthorizationResult<TEntity, long> authorizationResult)
        {
            var id = authorizationResult.Value1;

            var entity = await _contextFactory.Set<TEntity>().WithAuthorizationFilter(authorizationResult).FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
                return ServiceResponse.FromStatus<TGetFullDto>(HttpStatusCode.NotFound);

            var dto = _mapper.Map<TGetFullDto>(entity);

            return ServiceResponse.FromResult(dto);
        }

        /// <summary>
        /// The method contains the business logic for the UPDATE-Single operation.
        /// It will update the entity in the database with the given DTO.
        /// </summary>
        /// <param name="authorizationResult">The result of the authorization which contains the DTO which contains updated values.</param>
        /// <returns>The item as it is stored in the database after the update operation.</returns>
        protected virtual async Task<ServiceResponse<TGetFullDto>> UpdateInternalAsync(AuthorizationResult<TEntity, TUpdateDto> authorizationResult)
        {
            var dto = authorizationResult.Value1;

            await using var context = _contextFactory.CreateDbContext();

            var entity = await context.Set<TEntity>().WithAuthorizationFilter(authorizationResult).FirstOrDefaultAsync(e => e.Id == dto.Id);

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
        /// <param name="authorizationResult">The result of the authorization which contains the DTOs which contain updated values.</param>
        /// <returns>The items as stored in the database after the update operation.</returns>
        protected virtual async Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> UpdateInternalAsync(AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>> authorizationResult)
        {
            var request = authorizationResult.Value1;
            var dtos = request.Dtos;

            var ids = System.Linq.Enumerable.ToHashSet(System.Linq.Enumerable.Select(dtos, d => d.Id));
            await using var context = _contextFactory.CreateDbContext();

            var entities = await request.Filter(System.Linq.Queryable.Where(context.Set<TEntity>().WithAuthorizationFilter(authorizationResult), e => ids.Contains(e.Id)))
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
    }
}