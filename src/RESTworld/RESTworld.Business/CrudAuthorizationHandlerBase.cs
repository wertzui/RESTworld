using RESTworld.Business.Abstractions;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RESTworld.Business
{
    /// <summary>
    /// This class can be used as a base for your authorization handlers.
    /// It does not check anything, but implements the interface.
    /// This allows you to only override the methods which you need to implement.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the create dto.</typeparam>
    /// <typeparam name="TGetListDto">The type of the get list dto.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the get full dto.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the update dto.</typeparam>
    /// <seealso cref="ICrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}" />
    public abstract class CrudAuthorizationHandlerBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto> : ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
    {
        /// <inheritdoc/>
        public virtual Task<AuthorizationResult<TEntity, TCreateDto>> HandleCreateRequestAsync(AuthorizationResult<TEntity, TCreateDto> previousResult) => Task.FromResult(previousResult);

        /// <inheritdoc/>
        public virtual Task<AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>>> HandleCreateRequestAsync(AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>> previousResult) => Task.FromResult(previousResult);

        /// <inheritdoc/>
        public virtual Task<ServiceResponse<TGetFullDto>> HandleCreateResponseAsync(ServiceResponse<TGetFullDto> previousResponse) => Task.FromResult(previousResponse);

        /// <inheritdoc/>
        public virtual Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleCreateResponseAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse) => Task.FromResult(previousResponse);

        /// <inheritdoc/>
        public virtual Task<AuthorizationResult<TEntity, long, byte[]>> HandleDeleteRequestAsync(AuthorizationResult<TEntity, long, byte[]> previousResult) => Task.FromResult(previousResult);

        /// <inheritdoc/>
        public virtual Task<ServiceResponse<object>> HandleDeleteResponseAsync(ServiceResponse<object> previousResponse) => Task.FromResult(previousResponse);

        /// <inheritdoc/>
        public virtual Task<AuthorizationResult<TEntity, IGetListRequest<TEntity>>> HandleGetListRequestAsync(AuthorizationResult<TEntity, IGetListRequest<TEntity>> previousResult) => Task.FromResult(previousResult);

        /// <inheritdoc/>
        public virtual Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> HandleGetListResponseAsync(ServiceResponse<IReadOnlyPagedCollection<TGetListDto>> previousResponse) => Task.FromResult(previousResponse);

        /// <inheritdoc/>
        public virtual Task<AuthorizationResult<TEntity, long>> HandleGetSingleRequestAsync(AuthorizationResult<TEntity, long> previousResult) => Task.FromResult(previousResult);

        /// <inheritdoc/>
        public virtual Task<ServiceResponse<TGetFullDto>> HandleGetSingleResponseAsync(ServiceResponse<TGetFullDto> previousResponse) => Task.FromResult(previousResponse);

        /// <inheritdoc/>
        public virtual Task<AuthorizationResult<TEntity, TUpdateDto>> HandleUpdateRequestAsync(AuthorizationResult<TEntity, TUpdateDto> previousResult) => Task.FromResult(previousResult);

        /// <inheritdoc/>
        public virtual Task<AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>>> HandleUpdateRequestAsync(AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>> previousResult) => Task.FromResult(previousResult);

        /// <inheritdoc/>
        public virtual Task<ServiceResponse<TGetFullDto>> HandleUpdateResponseAsync(ServiceResponse<TGetFullDto> previousResponse) => Task.FromResult(previousResponse);

        /// <inheritdoc/>
        public virtual Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleUpdateResponseAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse) => Task.FromResult(previousResponse);
    }
}