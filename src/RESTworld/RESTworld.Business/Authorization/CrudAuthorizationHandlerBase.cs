using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using RESTworld.Business.Services.Abstractions;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Authorization;

/// <summary>
/// This class can be used as a base for your CRUD authorization handlers.
/// Use it if you have a <see cref="ICrudServiceBase{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/> and want to check for authorization.
/// It does not check anything, but implements the interface.
/// This allows you to only override the methods which you need to implement.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TCreateDto">The type of the create DTO.</typeparam>
/// <typeparam name="TGetListDto">The type of the get list DTO.</typeparam>
/// <typeparam name="TGetFullDto">The type of the get full DTO.</typeparam>
/// <typeparam name="TUpdateDto">The type of the update DTO.</typeparam>
/// <seealso cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}" />
public abstract class CrudAuthorizationHandlerBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto> : ReadAuthorizationHandlerBase<TEntity, TGetListDto, TGetFullDto>, ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
{
    /// <inheritdoc/>
    public virtual Task<AuthorizationResult<TEntity, TCreateDto>> HandleCreateRequestAsync(AuthorizationResult<TEntity, TCreateDto> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public virtual Task<AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>>> HandleCreateRequestAsync(AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public virtual Task<ServiceResponse<TGetFullDto>> HandleCreateResponseAsync(ServiceResponse<TGetFullDto> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);

    /// <inheritdoc/>
    public virtual Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleCreateResponseAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);

    /// <inheritdoc/>
    public virtual Task<AuthorizationResult<TEntity, long, byte[]>> HandleDeleteRequestAsync(AuthorizationResult<TEntity, long, byte[]> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public virtual Task<ServiceResponse<object>> HandleDeleteResponseAsync(ServiceResponse<object> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);

    /// <inheritdoc/>
    public virtual Task<AuthorizationResult<TEntity, TUpdateDto>> HandleUpdateRequestAsync(AuthorizationResult<TEntity, TUpdateDto> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public virtual Task<AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>>> HandleUpdateRequestAsync(AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>> previousResult, CancellationToken cancellationToken) => Task.FromResult(previousResult);

    /// <inheritdoc/>
    public virtual Task<ServiceResponse<TGetFullDto>> HandleUpdateResponseAsync(ServiceResponse<TGetFullDto> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);

    /// <inheritdoc/>
    public virtual Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleUpdateResponseAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse, CancellationToken cancellationToken) => Task.FromResult(previousResponse);
}