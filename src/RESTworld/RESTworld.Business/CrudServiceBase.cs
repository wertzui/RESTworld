using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RESTworld.Business.Abstractions;
using RESTworld.EntityFrameworkCore;
using RESTworld.EntityFrameworkCore.Models;
using RESTworld.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading.Tasks;

namespace RESTworld.Business
{
    public class CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        : ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
        where TContext : DbContextBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
        where TUpdateDto : DtoBase
    {
        protected readonly IDbContextFactory<TContext> _contextFactory;
        protected readonly ILogger<CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> _logger;
        protected readonly IMapper _mapper;
        protected readonly IEnumerable<ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> _authorizationHandlers;

        public CrudServiceBase(
            IDbContextFactory<TContext> contextFactory,
            IMapper mapper,
            IEnumerable<ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> authorizationHandlers,
            ILogger<CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>> logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _authorizationHandlers = authorizationHandlers ?? throw new ArgumentNullException(nameof(authorizationHandlers));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            if (System.Linq.Enumerable.Any(_authorizationHandlers))
                _logger.LogWarning($"No {nameof(ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>)}<{typeof(TEntity).Name}, {typeof(TCreateDto).Name}, {typeof(TGetListDto).Name}, {typeof(TGetFullDto).Name}, {typeof(TUpdateDto).Name}> is configured. No authorization will be performed for any methods of {nameof(CrudServiceBase<TContext, TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>)}<{typeof(TEntity).Name}, {typeof(TCreateDto).Name}, {typeof(TGetListDto).Name}, {typeof(TGetFullDto).Name}, {typeof(TUpdateDto).Name}>.");
        }

        public Task<ServiceResponse<TGetFullDto>> CreateAsync(TCreateDto dto)
            => TryExecuteWithAuthorizationAsync(
                dto,
                param1 => CreateInternalAsync(param1),
                (result, handler) => handler.HandleCreateRequestAsync(result),
                (response, handler) => handler.HandleCreateResponseAsync(response));

        public Task<ServiceResponse<object>> DeleteAsync(long id, byte[] timestamp)
            => TryExecuteWithAuthorizationAsync(
                id,
                timestamp,
                (param1, param2) => DeleteInternalAsync(param1, param2),
                (result, handler) => handler.HandleDeleteRequestAsync(result),
                (response, handler) => handler.HandleDeleteResponseAsync(response));

        public Task<ServiceResponse<IReadOnlyCollection<TGetListDto>>> GetListAsync(Func<System.Linq.IQueryable<TEntity>, System.Linq.IQueryable<TEntity>> filter)
            => TryExecuteWithAuthorizationAsync(
                filter,
                param1 => GetListInternalAsync(param1),
                (result, handler) => handler.HandleGetListRequestAsync(result),
                (response, handler) => handler.HandleGetListResponseAsync(response));

        public Task<ServiceResponse<TGetFullDto>> GetSingleAsync(long id)
            => TryExecuteWithAuthorizationAsync(
                id,
                param1 => GetSingleInternalAsync(param1),
                (result, handler) => handler.HandleGetSingleRequestAsync(result),
                (response, handler) => handler.HandleGetSingleResponseAsync(response));

        public Task<ServiceResponse<TGetFullDto>> UpdateAsync(TUpdateDto dto)
            => TryExecuteWithAuthorizationAsync(
                dto,
                param1 => UpdateInternalAsync(param1),
                (result, handler) => handler.HandleUpdateRequestAsync(result),
                (response, handler) => handler.HandleUpdateResponseAsync(response));

        protected virtual async Task<ServiceResponse<TGetFullDto>> CreateInternalAsync(TCreateDto dto)
        {
            var entity = _mapper.Map<TEntity>(dto);

            await using var context = _contextFactory.CreateDbContext();
            await context.AddAsync(entity);
            await context.SaveChangesAsync();

            var resultDto = _mapper.Map<TGetFullDto>(entity);

            return ServiceResponse.FromResult(resultDto);
        }

        protected virtual async Task<ServiceResponse<object>> DeleteInternalAsync(long id, byte[] timestamp)
        {
            await using var context = _contextFactory.CreateDbContext();

            var entity = await context.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
                return ServiceResponse.FromStatus<object>(HttpStatusCode.NotFound);

            if (!System.Linq.Enumerable.SequenceEqual(entity.Timestamp, timestamp))
                return ServiceResponse.FromProblem<object>(HttpStatusCode.Conflict, "The entity was modfied.");

            context.Remove(entity);

            await context.SaveChangesAsync();

            return ServiceResponse.FromStatus<object>(HttpStatusCode.OK);
        }

        protected virtual async Task<ServiceResponse<IReadOnlyCollection<TGetListDto>>> GetListInternalAsync(Func<System.Linq.IQueryable<TEntity>, System.Linq.IQueryable<TEntity>> filter)
        {
            var set = _contextFactory.Set<TEntity>();

            if (filter is not null)
                set = filter(set);

            var entities = await set.ToListAsync();

            var dtos = _mapper.Map<IReadOnlyCollection<TGetListDto>>(entities);

            return ServiceResponse.FromResult(dtos);
        }

        protected virtual async Task<ServiceResponse<TGetFullDto>> GetSingleInternalAsync(long id)
        {
            var entity = await _contextFactory.Set<TEntity>().FirstOrDefaultAsync(e => e.Id == id);

            if (entity is null)
                return ServiceResponse.FromStatus<TGetFullDto>(HttpStatusCode.NotFound);

            var dto = _mapper.Map<TGetFullDto>(entity);

            return ServiceResponse.FromResult(dto);
        }

        protected virtual async Task<ServiceResponse<T>> TryExecuteAsync<T>(Func<Task<ServiceResponse<T>>> function)
        {
            try
            {
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

        protected virtual Task<ServiceResponse<TResponse>> TryExecuteWithAuthorizationAsync<T1, TResponse>(
            T1 param1,
            Func<T1, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResult<TEntity, T1>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<AuthorizationResult<TEntity, T1>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<ServiceResponse<TResponse>>> authorizeResult)
            => TryExecuteAsync(() => WithAuthorizationAsync(param1, function, authorizeRequest, authorizeResult));

        protected virtual Task<ServiceResponse<TResponse>> TryExecuteWithAuthorizationAsync<T1, T2, TResponse>(
            T1 param1,
            T2 param2,
            Func<T1, T2, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResult<TEntity, T1, T2>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<AuthorizationResult<TEntity, T1, T2>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<ServiceResponse<TResponse>>> authorizeResult)
            => TryExecuteAsync(() => WithAuthorizationAsync(param1, param2, function, authorizeRequest, authorizeResult));

        protected virtual async Task<ServiceResponse<TGetFullDto>> UpdateInternalAsync(TUpdateDto dto)
        {
            await using var context = _contextFactory.CreateDbContext();

            var entity = await context.Set<TEntity>().FirstOrDefaultAsync(x => x.Id == dto.Id);

            if (entity is null)
                return ServiceResponse.FromStatus<TGetFullDto>(HttpStatusCode.NotFound);

            context.Entry(entity).Property(nameof(EntityBase.Timestamp)).OriginalValue = dto.Timestamp;

            _mapper.Map(dto, entity);

            await context.SaveChangesAsync();

            var resultDto = _mapper.Map<TGetFullDto>(entity);

            return ServiceResponse.FromResult(resultDto);
        }

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

        protected virtual async Task<AuthorizationResult<TEntity, T1>> AuthorizeRequestAsync<T1>(
            T1 value1,
            Func<AuthorizationResult<TEntity, T1>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<AuthorizationResult<TEntity, T1>>> authorizeRequest)
        {
            var result = AuthorizationResult.Ok<TEntity, T1>(value1);

            foreach (var handler in _authorizationHandlers)
            {
                result = await authorizeRequest(result, handler);
            }

            return result;
        }

        protected virtual async Task<AuthorizationResult<TEntity, T1, T2>> AuthorizeRequestAsync<T1, T2>(
            T1 value1,
            T2 value2,
            Func<AuthorizationResult<TEntity, T1, T2>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>, Task<AuthorizationResult<TEntity, T1, T2>>> authorizeRequest)
        {
            var result = AuthorizationResult.Ok<TEntity, T1, T2>(value1, value2);

            foreach (var handler in _authorizationHandlers)
            {
                result = await authorizeRequest(result, handler);
            }

            return result;
        }

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
    }
}