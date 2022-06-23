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
        : ReadServiceBase<TContext, TEntity, TGetListDto, TGetFullDto>, ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        where TContext : DbContextBase
        where TEntity : ConcurrentEntityBase
        where TGetListDto : ConcurrentDtoBase
        where TGetFullDto : ConcurrentDtoBase
        where TUpdateDto : ConcurrentDtoBase
    {
        private static bool _authorizationHandlerWarningWasLogged;

        /// <summary>
        /// Creates a new instance of the
        /// <see cref="CrudServiceBase{TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/> class.
        /// </summary>
        /// <param name="contextFactory">The factory used to create a <see cref="DbContext"/>.</param>
        /// <param name="mapper">The AutoMapper instance which maps between DTOs and entities.</param>
        /// <param name="authorizationHandlers">
        /// All AuthorizationHandlers which will be called during authorization.
        /// </param>
        /// <param name="userAccessor">The user accessor which gets the user of the current request.</param>
        /// <param name="logger">The logger.</param>
        public CrudServiceBase(
            IDbContextFactory<TContext> contextFactory,
            IMapper mapper,
            IEnumerable<ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> authorizationHandlers,
            IUserAccessor userAccessor,
            ILogger<CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> logger)
            : base(contextFactory, mapper, authorizationHandlers, userAccessor, logger)
        {
            CrudAuthorizationHandlers = authorizationHandlers ?? throw new ArgumentNullException(nameof(authorizationHandlers));

            LogAuthoriztaionHandlerWarningOnlyOneTimeIfNoHandlersArePresent(authorizationHandlers);
        }

        /// <summary>
        /// The authorization handlers which are used for all CRUD operations.
        /// </summary>
        protected virtual IEnumerable<ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> CrudAuthorizationHandlers { get; }

        /// <inheritdoc/>
        protected override IEnumerable<IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>> ReadAuthorizationHandlers => CrudAuthorizationHandlers;

        /// <inheritdoc/>
        public Task<ServiceResponse<TGetFullDto>> CreateAsync(TCreateDto dto)
            => TryExecuteWithAuthorizationAsync<TEntity, TCreateDto, TGetFullDto, ICreateAuthorizationHandler<TEntity, TCreateDto, TGetFullDto>>(
                dto,
                result => CreateInternalAsync(result),
                (result, handler) => handler.HandleCreateRequestAsync(result),
                (response, handler) => handler.HandleCreateResponseAsync(response),
                CrudAuthorizationHandlers);

        /// <inheritdoc/>
        public Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> CreateAsync(IReadOnlyCollection<TCreateDto> dtos)
            => TryExecuteWithAuthorizationAsync<TEntity, IReadOnlyCollection<TCreateDto>, IReadOnlyCollection<TGetFullDto>, ICreateAuthorizationHandler<TEntity, TCreateDto, TGetFullDto>>(
                dtos,
                result => CreateInternalAsync(result),
                (result, handler) => handler.HandleCreateRequestAsync(result),
                (response, handler) => handler.HandleCreateResponseAsync(response),
                CrudAuthorizationHandlers);

        /// <inheritdoc/>
        public Task<ServiceResponse<object>> DeleteAsync(long id, byte[] timestamp)
            => TryExecuteWithAuthorizationAsync<TEntity, long, byte[], object, IDeleteAuthorizationHandler<TEntity>>(
                id,
                timestamp,
                result => DeleteInternalAsync(result),
                (result, handler) => handler.HandleDeleteRequestAsync(result),
                (response, handler) => handler.HandleDeleteResponseAsync(response),
                CrudAuthorizationHandlers);

        /// <inheritdoc/>
        public Task<ServiceResponse<TGetFullDto>> UpdateAsync(TUpdateDto dto)
            => TryExecuteWithAuthorizationAsync<TEntity, TUpdateDto, TGetFullDto, IUpdateAuthorizationHandler<TEntity, TUpdateDto, TGetFullDto>>(
                dto,
                result => UpdateInternalAsync(result),
                (result, handler) => handler.HandleUpdateRequestAsync(result),
                (response, handler) => handler.HandleUpdateResponseAsync(response),
                CrudAuthorizationHandlers);

        /// <inheritdoc/>
        public Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> UpdateAsync(IUpdateMultipleRequest<TUpdateDto, TEntity> request)
            => TryExecuteWithAuthorizationAsync<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>, IReadOnlyCollection<TGetFullDto>, IUpdateAuthorizationHandler<TEntity, TUpdateDto, TGetFullDto>>(
                request,
                result => UpdateInternalAsync(result),
                (result, handler) => handler.HandleUpdateRequestAsync(result),
                (response, handler) => handler.HandleUpdateResponseAsync(response),
                CrudAuthorizationHandlers);

        /// <summary>
        /// Sets the OriginalValue of the timestamp if the timestamp exists on the entity. If it is
        /// marked with a [NotMapped] Attribute, this method will do nothing.
        /// </summary>
        /// <param name="dto">The DTO which contains the expected timestamp.</param>
        /// <param name="context">The context on which the changes are done.</param>
        /// <param name="entity">The entity as it has been read from the database.</param>
        protected static void SetTimestampOriginalValue(ConcurrentDtoBase dto, DbContext context, ConcurrentEntityBase entity)
        {
            var entry = context.Entry(entity);
            var timestampProperty = System.Linq.Enumerable.SingleOrDefault(entry.Properties, p => p.Metadata.Name == nameof(ConcurrentEntityBase.Timestamp));
            if (timestampProperty is not null)
                timestampProperty.OriginalValue = dto.Timestamp;
        }

        /// <summary>
        /// The method contains the business logic for the CREATE operation. It will insert the
        /// given DTO into the database.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the DTO to insert into the database.
        /// </param>
        /// <returns>A response containing the DTO as it is in the database.</returns>
        protected virtual async Task<ServiceResponse<TGetFullDto>> CreateInternalAsync(AuthorizationResult<TEntity, TCreateDto> authorizationResult)
        {
            var dto = authorizationResult.Value1;

            var entity = _mapper.Map<TEntity>(dto);

            await using var context = _contextFactory.CreateDbContext();

            context.Add(entity);

            await OnCreatingInternalAsync(authorizationResult, context, entity);

            await context.SaveChangesAsync(GetCurrentUsersName());

            var resultDto = _mapper.Map<TGetFullDto>(entity);

            await OnCreatedInternalAsync(authorizationResult, resultDto, entity);

            return ServiceResponse.FromResult(resultDto);
        }

        /// <summary>
        /// The method contains the business logic for the CREATE operation. It will insert the
        /// given DTOs into the database.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the DTOs to insert into the database.
        /// </param>
        /// <returns>A response containing the DTOs as it is in the database.</returns>
        protected virtual async Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> CreateInternalAsync(AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>> authorizationResult)
        {
            var dtos = authorizationResult.Value1;

            var entities = _mapper.Map<IReadOnlyCollection<TEntity>>(dtos);

            await using var context = _contextFactory.CreateDbContext();

            context.AddRange(entities);

            await OnCreatingInternalAsync(authorizationResult, context, entities);

            await context.SaveChangesAsync(GetCurrentUsersName());

            var resultDto = _mapper.Map<IReadOnlyCollection<TGetFullDto>>(entities);

            await OnCreatedInternalAsync(authorizationResult, resultDto, entities);

            return ServiceResponse.FromResult(resultDto);
        }

        /// <summary>
        /// The method contains the business logic for the DELETE operation. It will delete the
        /// entity with the given ID and timestamp from the database.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the identifier and the timestamp.
        /// </param>
        /// <returns>
        /// An empty 200 (Ok) response, or a problem response with either 404 (Not found) if the ID
        /// does not exist or 409 (Conflict) if the timestamp does not match.
        /// </returns>
        protected virtual async Task<ServiceResponse<object>> DeleteInternalAsync(AuthorizationResult<TEntity, long, byte[]> authorizationResult)
        {
            var id = authorizationResult.Value1;
            var timestamp = authorizationResult.Value2;

            await using var context = _contextFactory.CreateDbContext();

            var entity = await GetDbSetForUpdatingWithAuthorization(context, authorizationResult).FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
                return ServiceResponse.FromStatus<object>(HttpStatusCode.NotFound);

            if (entity.Timestamp is not null && timestamp is null)
                return ServiceResponse.FromProblem<object>(HttpStatusCode.Conflict, "You must provide a timestamp.");

            if (entity.Timestamp is not null && !System.Linq.Enumerable.SequenceEqual(entity.Timestamp, timestamp))
                return ServiceResponse.FromProblem<object>(HttpStatusCode.Conflict, "The entity was modfied.");

            context.Remove(entity);

            await OnDeletingAsync(authorizationResult, entity, context);

            await context.SaveChangesAsync(GetCurrentUsersName());

            await OnDeletedAsync(authorizationResult, entity);

            return ServiceResponse.FromStatus<object>(HttpStatusCode.OK);
        }

        /// <summary>
        /// Gets the DB set from the context that is used when updating and deleting (not reading!).
        /// If you need to add some custom logic on how this is generated, you can override this method.
        /// </summary>
        /// <param name="context">The DB context to get the set from.</param>
        /// <returns>The DB set that is used in all other methods.</returns>
        protected virtual System.Linq.IQueryable<TEntity> GetDbSetForUpdating(TContext context) => context.Set<TEntity>();

        /// <summary>
        /// Gets the DB set from <see cref="GetDbSetForUpdating"/> that is used when updating and
        /// deleting (not reading!) and applies the given authorization filters. You can override
        /// this method if you need to apply a custom authorization logic. If you just want to
        /// modify the DB set, you should override <see cref="GetDbSetForUpdating"/> instead.
        /// </summary>
        /// <param name="context">The DB context to get the set from.</param>
        /// <param name="authorizationResult">
        /// The authorization result containing the filter that will be applied to the DB set.
        /// </param>
        /// <returns>The filtered DB set that is used in all other methods.</returns>
        protected virtual System.Linq.IQueryable<TEntity> GetDbSetForUpdatingWithAuthorization(TContext context, AuthorizationResult<TEntity> authorizationResult) => GetDbSetForUpdating(context).WithAuthorizationFilter(authorizationResult);

        /// <summary>
        /// This method is called after SaveChangesAsync() so the entity is already persisted in the
        /// database and the result has been mapped into a DTO.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the DTO which contain updated values.
        /// </param>
        /// <param name="resultDto">The resulting DTO which was mapped from the persisted entity.</param>
        /// <param name="entity">The entity as it has been persisted in the database.</param>
        /// <returns></returns>
        protected virtual Task OnCreatedInternalAsync(AuthorizationResult<TEntity, TCreateDto> authorizationResult, TGetFullDto resultDto, TEntity entity)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method is called after SaveChangesAsync() so the entities are already persisted in
        /// the database and the result has been mapped into a DTOs.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the DTOs which contain updated values.
        /// </param>
        /// <param name="resultDto">
        /// The resulting DTOs which have been mapped from the persisted entity.
        /// </param>
        /// <param name="entities">The entities as they have been persisted in the database.</param>
        /// <returns></returns>
        protected virtual Task OnCreatedInternalAsync(AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>> authorizationResult, IReadOnlyCollection<TGetFullDto> resultDto, IReadOnlyCollection<TEntity> entities)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method is called after the entity has been created from the DTO but before
        /// SaveChangesAsync() is called.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the DTOs which contain updated values.
        /// </param>
        /// <param name="context">The context on which the changes are done.</param>
        /// <param name="entity">The entity with the modification from the DTO already applied.</param>
        /// <returns></returns>
        protected virtual Task OnCreatingInternalAsync(AuthorizationResult<TEntity, TCreateDto> authorizationResult, TContext context, TEntity entity)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method is called after the entities have been created from the DTOs but before
        /// SaveChangesAsync() is called.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the DTOs which contain updated values.
        /// </param>
        /// <param name="context">The context on which the changes are done.</param>
        /// <param name="entities">The entities with the modifications from the DTOs already applied.</param>
        /// <returns></returns>
        protected virtual Task OnCreatingInternalAsync(AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>> authorizationResult, TContext context, IReadOnlyCollection<TEntity> entities)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method is called after SaveChangesAsync() has been called.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the identifier and the timestamp.
        /// </param>
        /// <param name="entity">The entity as it has been read from the database.</param>
        /// <returns></returns>
        protected virtual Task OnDeletedAsync(AuthorizationResult<TEntity, long, byte[]> authorizationResult, TEntity entity)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method is called after the entity has been removed from the context, but before
        /// SaveChangesAsync() is called.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the identifier and the timestamp.
        /// </param>
        /// <param name="entity">The entity as it has been read from the database.</param>
        /// <param name="context">The context on which the changes are done.</param>
        /// <returns></returns>
        protected virtual Task OnDeletingAsync(AuthorizationResult<TEntity, long, byte[]> authorizationResult, TEntity entity, TContext context)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method is called after SaveChangesAsync() so the entity is already persisted in the
        /// database and the result has been mapped into a DTO.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the DTO which contain updated values.
        /// </param>
        /// <param name="resultDto">The resulting DTO which was mapped from the persisted entity.</param>
        /// <param name="entity">The entity as it has been persisted in the database.</param>
        /// <returns></returns>
        protected virtual Task OnUpdatedInternalAsync(AuthorizationResult<TEntity, TUpdateDto> authorizationResult, TGetFullDto resultDto, TEntity entity)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method is called after SaveChangesAsync() so the entities are already persisted in
        /// the database and the result has been mapped into a DTOs.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the DTOs which contain updated values.
        /// </param>
        /// <param name="resultDto">
        /// The resulting DTOs which have been mapped from the persisted entity.
        /// </param>
        /// <param name="entities">The entities as they have been persisted in the database.</param>
        /// <returns></returns>
        protected virtual Task OnUpdatedInternalAsync(AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>> authorizationResult, IReadOnlyCollection<TGetFullDto> resultDto, Dictionary<long, TEntity> entities)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method is called after the update from the DTO has been applied to the entity but
        /// before SaveChangesAsync() is called.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the DTOs which contain updated values.
        /// </param>
        /// <param name="context">The context on which the changes are done.</param>
        /// <param name="entity">The entity with the modification from the DTO already applied.</param>
        /// <returns></returns>
        protected virtual Task OnUpdatingInternalAsync(AuthorizationResult<TEntity, TUpdateDto> authorizationResult, TContext context, TEntity entity)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method is called after the update from the DTO has been applied to the entity but
        /// before SaveChangesAsync() is called.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the DTOs which contain updated values.
        /// </param>
        /// <param name="context">The context on which the changes are done.</param>
        /// <param name="entities">The entities with the modifications from the DTOs already applied.</param>
        /// <returns></returns>
        protected virtual Task OnUpdatingInternalAsync(AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>> authorizationResult, TContext context, IDictionary<long, TEntity> entities)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// The method contains the business logic for the UPDATE-Single operation. It will update
        /// the entity in the database with the given DTO.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the DTO which contains updated values.
        /// </param>
        /// <returns>The item as it is stored in the database after the update operation.</returns>
        protected virtual async Task<ServiceResponse<TGetFullDto>> UpdateInternalAsync(AuthorizationResult<TEntity, TUpdateDto> authorizationResult)
        {
            var dto = authorizationResult.Value1;

            await using var context = _contextFactory.CreateDbContext();

            var entity = await GetDbSetForUpdatingWithAuthorization(context, authorizationResult).FirstOrDefaultAsync(e => e.Id == dto.Id);

            if (entity is null)
                return ServiceResponse.FromStatus<TGetFullDto>(HttpStatusCode.NotFound);

            SetTimestampOriginalValue(dto, context, entity);

            _mapper.Map(dto, entity);

            await OnUpdatingInternalAsync(authorizationResult, context, entity);

            await context.SaveChangesAsync(GetCurrentUsersName());

            var resultDto = _mapper.Map<TGetFullDto>(entity);

            await OnUpdatedInternalAsync(authorizationResult, resultDto, entity);

            return ServiceResponse.FromResult(resultDto);
        }

        /// <summary>
        /// The method contains the business logic for the UPDATE-Multiple operation. It will update
        /// the entities in the database with the given DTOs.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the DTOs which contain updated values.
        /// </param>
        /// <returns>The items as stored in the database after the update operation.</returns>
        protected virtual async Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> UpdateInternalAsync(AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>> authorizationResult)
        {
            var request = authorizationResult.Value1;
            var dtos = request.Dtos;

            var ids = System.Linq.Enumerable.ToHashSet(System.Linq.Enumerable.Select(dtos, d => d.Id));
            await using var context = _contextFactory.CreateDbContext();

            var entities = await request.Filter(System.Linq.Queryable.Where(GetDbSetForUpdatingWithAuthorization(context, authorizationResult), e => ids.Contains(e.Id)))
                .ToDictionaryAsync(e => e.Id);

            if (entities.Count != dtos.Count)
                return ServiceResponse.FromStatus<IReadOnlyCollection<TGetFullDto>>(HttpStatusCode.NotFound);

            foreach (var dto in dtos)
            {
                var entity = entities[dto.Id];
                SetTimestampOriginalValue(dto, context, entity);
                _mapper.Map(dto, entity);
            }

            await OnUpdatingInternalAsync(authorizationResult, context, entities);

            await context.SaveChangesAsync(GetCurrentUsersName());

            var resultDto = _mapper.Map<IReadOnlyCollection<TGetFullDto>>(entities.Values);

            await OnUpdatedInternalAsync(authorizationResult, resultDto, entities);

            return ServiceResponse.FromResult(resultDto);
        }

        private void LogAuthoriztaionHandlerWarningOnlyOneTimeIfNoHandlersArePresent(IEnumerable<ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> authorizationHandlers)
        {
            if (!_authorizationHandlerWarningWasLogged && !System.Linq.Enumerable.Any(authorizationHandlers))
            {
                _authorizationHandlerWarningWasLogged = true;

                _logger.LogWarning("No {TCrudAuthorizationHandler} is configured. No authorization will be performed for any methods of {TCrudServiceBase}.",
                    $"{nameof(ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>)}<{typeof(TEntity).Name}, {typeof(TCreateDto).Name}, {typeof(TGetListDto).Name}, {typeof(TGetFullDto).Name}, {typeof(TUpdateDto).Name}>",
                    $"{nameof(CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>)}<{typeof(TEntity).Name}, {typeof(TCreateDto).Name}, {typeof(TGetListDto).Name}, {typeof(TGetFullDto).Name}, {typeof(TUpdateDto).Name}>");
            }
        }
    }
}