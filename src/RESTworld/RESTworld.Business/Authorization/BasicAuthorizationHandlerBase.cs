﻿using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Authorization;

/// <summary>
/// This class can be used as a base for your basic authorization handlers.
/// Use it if you have your own service implementing a logic which does not fit into CRUD.
/// It does not check anything, but implements the interface.
/// This allows you to only override the methods which you need to implement.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <seealso cref="IBasicAuthorizationHandler&lt;TRequest, TResponse&gt;" />
public abstract class BasicAuthorizationHandlerBase<TRequest, TResponse> : IBasicAuthorizationHandler<TRequest, TResponse>
{
    /// <inheritdoc/>
    public virtual Task<AuthorizationResultWithoutDb<TRequest>> HandleRequestAsync(AuthorizationResultWithoutDb<TRequest> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public virtual Task<ServiceResponse<TResponse>> HandleResponseAsync(ServiceResponse<TResponse> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);
}

/// <summary>
/// This class can be used as a base for your basic authorization handlers.
/// Use it if you have your own service implementing a logic which does not fit into CRUD.
/// It does not check anything, but implements the interface.
/// This allows you to only override the methods which you need to implement.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <seealso cref="IBasicAuthorizationHandler&lt;TEntity, TRequest, TResponse&gt;" />
/// <seealso cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}" />
public abstract class BasicAuthorizationHandlerBase<TEntity, TRequest, TResponse> : IBasicAuthorizationHandler<TEntity, TRequest, TResponse>
{
    /// <inheritdoc/>
    public virtual Task<AuthorizationResult<TEntity, TRequest>> HandleRequestAsync(AuthorizationResult<TEntity, TRequest> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public virtual Task<ServiceResponse<TResponse>> HandleResponseAsync(ServiceResponse<TResponse> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);
}