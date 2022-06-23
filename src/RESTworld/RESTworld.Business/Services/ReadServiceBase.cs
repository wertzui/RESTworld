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
using System.Linq;
using System.Net;
using System.Threading.Tasks;

namespace RESTworld.Business.Services
{
    /// <inheritdoc/>
    public class ReadServiceBase<TContext, TEntity, TGetListDto, TGetFullDto>
        : DbServiceBase<TContext>, IReadServiceBase<TEntity, TGetListDto, TGetFullDto>
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
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
        public ReadServiceBase(
            IDbContextFactory<TContext> contextFactory,
            IMapper mapper,
            IEnumerable<IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>> authorizationHandlers,
            IUserAccessor userAccessor,
            ILogger<ReadServiceBase<TContext, TEntity, TGetListDto, TGetFullDto>> logger)
            : base(contextFactory, mapper, userAccessor, logger)
        {
            ReadAuthorizationHandlers = authorizationHandlers ?? throw new ArgumentNullException(nameof(authorizationHandlers));

            LogAuthoriztaionHandlerWarningOnlyOneTimeIfNoHandlersArePresent(authorizationHandlers);
        }

        /// <summary>
        /// The authorization handlers which are used for read operations.
        /// </summary>
        protected virtual IEnumerable<IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>> ReadAuthorizationHandlers { get; }

        /// <inheritdoc/>
        public Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> GetListAsync(IGetListRequest<TEntity> request)
            => TryExecuteWithAuthorizationAsync<TEntity, IGetListRequest<TEntity>, IReadOnlyPagedCollection<TGetListDto>, IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>>(
                request,
                result => GetListInternalAsync(result),
                (result, handler) => handler.HandleGetListRequestAsync(result),
                (response, handler) => handler.HandleGetListResponseAsync(response),
                ReadAuthorizationHandlers);

        /// <inheritdoc/>
        public Task<ServiceResponse<TGetFullDto>> GetSingleAsync(long id)
            => TryExecuteWithAuthorizationAsync<TEntity, long, TGetFullDto, IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>>(
                id,
                result => GetSingleInternalAsync(result),
                (result, handler) => handler.HandleGetSingleRequestAsync(result),
                (response, handler) => handler.HandleGetSingleResponseAsync(response),
                ReadAuthorizationHandlers);

        /// <summary>
        /// Gets the DB set from the context that is used when reading (not updating!). If you need
        /// to add some custom logic on how this is generated, you can override this method.
        /// </summary>
        /// <returns>The DB set that is used in all other methods.</returns>
        protected virtual IQueryable<TEntity> GetDbSetForReading() => _contextFactory.Set<TContext, TEntity>();

        /// <summary>
        /// Gets the DB set from <see cref="GetDbSetForReading"/> that is used when reading (not
        /// updating!) and applies the given authorization filters. You can override this method if
        /// you need to apply a custom authorization logic. If you just want to modify the DB set,
        /// you should override <see cref="GetDbSetForReading"/> instead.
        /// </summary>
        /// <param name="authorizationResult">
        /// The authorization result containing the filter that will be applied to the DB set.
        /// </param>
        /// <returns>The filtered DB set that is used in all other methods.</returns>
        protected virtual IQueryable<TEntity> GetDbSetForReadingWithAuthorization(AuthorizationResult<TEntity> authorizationResult) => GetDbSetForReading().WithAuthorizationFilter(authorizationResult);

        /// <summary>
        /// The method contains the business logic for the READ-List operation. It will return the
        /// items from the database as specified in the request.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the request which contains a filter on
        /// which items to return. The filter is normally mapped from the OData parameters that
        /// where passed into the controller ($filter, $orderby, $top, $skip).
        /// </param>
        /// <returns>All items as specified in the request with paging options if specified.</returns>
        /// <exception cref="ArgumentException">
        /// If request.CalculateTotalCount is true, request.FilterForTotalCount must not be null.
        /// </exception>
        protected virtual async Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> GetListInternalAsync(AuthorizationResult<TEntity, IGetListRequest<TEntity>> authorizationResult)
        {
            var request = authorizationResult.Value1;

            var setForEntities = GetDbSetForReadingWithAuthorization(authorizationResult);

            Task<long>? totalPagesTask = null;
            long? totalCount = null;

            var tasks = new List<Task>(2);

            if (request is not null)
            {
                if (request.Filter is not null)
                    setForEntities = request.Filter(setForEntities);

                if (request.CalculateTotalCount)
                {
                    if (request.FilterForTotalCount is null)
                        throw new ArgumentException($"If {nameof(authorizationResult)}{nameof(authorizationResult.Value1)}.{nameof(authorizationResult.Value1.CalculateTotalCount)} is true, {nameof(authorizationResult)}{nameof(authorizationResult.Value1)}.{nameof(authorizationResult.Value1.FilterForTotalCount)} must not be null.", nameof(authorizationResult));

                    var totalPagesSet = GetDbSetForReadingWithAuthorization(authorizationResult);
                    totalPagesSet = request.FilterForTotalCount(totalPagesSet);
                    totalPagesTask = totalPagesSet.LongCountAsync();
                    tasks.Add(totalPagesTask);
                }
            }

            var entitiesTask = setForEntities.ToListAsync();
            tasks.Add(entitiesTask);

            await Task.WhenAll(tasks);

            var dtos = _mapper.Map<IReadOnlyCollection<TGetListDto>>(entitiesTask.Result);

            if (request is not null && request.CalculateTotalCount && totalPagesTask is not null)
                totalCount = totalPagesTask.Result;

            IReadOnlyPagedCollection<TGetListDto> pagedCollection = new ReadOnlyPagedCollection<TGetListDto>(dtos, totalCount);

            await OnGotListInternalAsync(authorizationResult, pagedCollection, entitiesTask.Result);

            return ServiceResponse.FromResult(pagedCollection);
        }

        /// <summary>
        /// The method contains the business logic for the READ-Single operation. It will return the
        /// item with the specified ID from the database.
        /// </summary>
        /// <param name="authorizationResult">
        /// TThe result of the authorization which contains the identifier of the item.
        /// </param>
        /// <returns>The item with the specified ID, or 404 (Not Found).</returns>
        protected virtual async Task<ServiceResponse<TGetFullDto>> GetSingleInternalAsync(AuthorizationResult<TEntity, long> authorizationResult)
        {
            var id = authorizationResult.Value1;

            var entity = await GetDbSetForReadingWithAuthorization(authorizationResult).FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
                return ServiceResponse.FromStatus<TGetFullDto>(HttpStatusCode.NotFound);

            var dto = _mapper.Map<TGetFullDto>(entity);

            await OnGotSingleInternalAsync(authorizationResult, dto, entity);

            return ServiceResponse.FromResult(dto);
        }

        /// <summary>
        /// This method is called after the entities have been read from the database and mapped
        /// into DTOs.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the filter.
        /// </param>
        /// <param name="pagedCollection">The DTOs which have been mapped from the entities.</param>
        /// <param name="entities">The entity as they have been read from the database.</param>
        /// <returns></returns>
        protected virtual Task OnGotListInternalAsync(AuthorizationResult<TEntity, IGetListRequest<TEntity>> authorizationResult, IReadOnlyPagedCollection<TGetListDto> pagedCollection, IReadOnlyCollection<TEntity> entities)
        {
            return Task.CompletedTask;
        }

        /// <summary>
        /// This method is called after the entity has been read from the database and mapped into a DTO.
        /// </summary>
        /// <param name="authorizationResult">
        /// The result of the authorization which contains the ID to get.
        /// </param>
        /// <param name="dto">The DTO which was mapped from the entity.</param>
        /// <param name="entity">The entity as it has been read from the database.</param>
        /// <returns></returns>
        protected virtual Task OnGotSingleInternalAsync(AuthorizationResult<TEntity, long> authorizationResult, TGetFullDto dto, TEntity entity)
        {
            return Task.CompletedTask;
        }

        private void LogAuthoriztaionHandlerWarningOnlyOneTimeIfNoHandlersArePresent(IEnumerable<IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>> authorizationHandlers)
        {
            if (!_authorizationHandlerWarningWasLogged && !System.Linq.Enumerable.Any(authorizationHandlers))
            {
                _authorizationHandlerWarningWasLogged = true;

                _logger.LogWarning("No {TReadAuthorizationHandler} is configured. No authorization will be performed for any methods of {TReadServiceBase}.",
                    $"{nameof(IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>)}<{typeof(TEntity).Name}, {typeof(TGetListDto).Name}, {typeof(TGetFullDto).Name}>",
                    $"{nameof(ReadServiceBase<TContext, TEntity, TGetListDto, TGetFullDto>)}<{typeof(TEntity).Name}, {typeof(TGetListDto).Name}, {typeof(TGetFullDto).Name}>");
            }
        }
    }
}