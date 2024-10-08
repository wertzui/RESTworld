﻿using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using RESTworld.Business.Services.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Authorization;

/// <summary>
/// This class can be used as a base for your READ authorization handlers.
/// Use it if you have a <see cref="IReadServiceBase{TEntity, TGetListDto, TGetFullDto}"/> and want to check for authorization.
/// It does not check anything, but implements the interface.
/// This allows you to only override the methods which you need to implement.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TGetListDto">The type of the get list DTO.</typeparam>
/// <typeparam name="TGetFullDto">The type of the get full DTO.</typeparam>
/// <seealso cref="IReadAuthorizationHandler{TEntity, TGetListDto, TGetFullDto}" />
public abstract class ReadAuthorizationHandlerBase<TEntity, TGetListDto, TGetFullDto> : IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto>
{
    /// <inheritdoc/>
    public Task<AuthorizationResult<TEntity, IGetHistoryRequest<TGetFullDto, TEntity>>> HandleGetHistoryRequestAsync(AuthorizationResult<TEntity, IGetHistoryRequest<TGetFullDto, TEntity>> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public Task<ServiceResponse<IReadOnlyPagedCollection<TGetFullDto>>> HandleGetHistoryResponseAsync(ServiceResponse<IReadOnlyPagedCollection<TGetFullDto>> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);

    /// <inheritdoc/>
    public virtual Task<AuthorizationResult<TEntity, IGetListRequest<TGetListDto, TEntity>>> HandleGetListRequestAsync(AuthorizationResult<TEntity, IGetListRequest<TGetListDto, TEntity>> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public virtual Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> HandleGetListResponseAsync(ServiceResponse<IReadOnlyPagedCollection<TGetListDto>> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);

    /// <inheritdoc/>
    public virtual Task<AuthorizationResult<TEntity, long>> HandleGetSingleRequestAsync(AuthorizationResult<TEntity, long> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public virtual Task<ServiceResponse<TGetFullDto>> HandleGetSingleResponseAsync(ServiceResponse<TGetFullDto> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);
}