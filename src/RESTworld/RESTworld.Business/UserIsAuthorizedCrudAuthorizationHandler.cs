using RESTworld.Business.Abstractions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RESTworld.Business
{
    /// <summary>
    /// This authorization handler gets the current user and provides overidable methods for request and response handling which also have the user as an input parameter.
    /// It will ensure that the user is authenticated for every request.
    /// It will not check if the user is authenticated during response handling as this is not needed, because the user cannot change between a request and the associated response.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the create dto.</typeparam>
    /// <typeparam name="TGetListDto">The type of the get list dto.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the get full dto.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the update dto.</typeparam>
    /// <seealso cref="RESTworld.Business.CrudAuthorizationHandlerBase&lt;TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto&gt;" />
    public class UserIsAuthorizedCrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto> : CrudAuthorizationHandlerBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserIsAuthorizedCrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/> class.
        /// </summary>
        /// <param name="userAccessor">The user accessor.</param>
        /// <exception cref="System.ArgumentNullException">userAccessor</exception>
        public UserIsAuthorizedCrudAuthorizationHandler(IUserAccessor userAccessor)
        {
            UserAccessor = userAccessor ?? throw new ArgumentNullException(nameof(userAccessor));
        }

        /// <summary>
        /// Gets the user accessor. If you need to change how the user is retrieved, you can override the <see cref="GetUser"/> method.
        /// </summary>
        /// <value>
        /// The user accessor.
        /// </value>
        protected IUserAccessor UserAccessor { get; }

        /// <inheritdoc/>
        public override Task<AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>>> HandleCreateRequestAsync(AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>> previousResult)
        {
            var user = GetUser();

            if (!user.Identity.IsAuthenticated)
                return Task.FromResult(previousResult.WithStatus(HttpStatusCode.Unauthorized));

            return HandleCreateRequestWithUserAsync(previousResult, user);
        }

        /// <inheritdoc/>
        public override Task<AuthorizationResult<TEntity, TCreateDto>> HandleCreateRequestAsync(AuthorizationResult<TEntity, TCreateDto> previousResult)
        {
            var user = GetUser();

            if (!user.Identity?.IsAuthenticated == true)
                return Task.FromResult(previousResult.WithStatus(HttpStatusCode.Unauthorized));

            return HandleCreateRequestWithUserAsync(previousResult, user);
        }

        /// <inheritdoc/>
        public override Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleCreateResponseAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse)
        {
            var user = GetUser();

            return HandleCreateResponseWithUserAsync(previousResponse, user);
        }

        /// <inheritdoc/>
        public override Task<ServiceResponse<TGetFullDto>> HandleCreateResponseAsync(ServiceResponse<TGetFullDto> previousResponse)
        {
            var user = GetUser();

            return HandleCreateResponseWithUserAsync(previousResponse, user);
        }

        /// <inheritdoc/>
        public override Task<AuthorizationResult<TEntity, long, byte[]>> HandleDeleteRequestAsync(AuthorizationResult<TEntity, long, byte[]> previousResult)
        {
            var user = GetUser();
            if (!user.Identity?.IsAuthenticated == true)
                return Task.FromResult(previousResult.WithStatus(HttpStatusCode.Unauthorized));

            return HandleDeleteRequestWithUserAsync(previousResult, user);
        }

        /// <inheritdoc/>
        public override Task<ServiceResponse<object>> HandleDeleteResponseAsync(ServiceResponse<object> previousResponse)
        {
            var user = GetUser();

            return HandleDeleteResponseWithUserAsync(previousResponse, user);
        }

        /// <inheritdoc/>
        public override Task<AuthorizationResult<TEntity, IGetListRequest<TEntity>>> HandleGetListRequestAsync(AuthorizationResult<TEntity, IGetListRequest<TEntity>> previousResult)
        {
            var user = GetUser();

            if (!user.Identity?.IsAuthenticated == true)
                return Task.FromResult(previousResult.WithStatus(HttpStatusCode.Unauthorized));

            return HandleGetListRequestWithUserAsync(previousResult, user);
        }

        /// <inheritdoc/>
        public override Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> HandleGetListResponseAsync(ServiceResponse<IReadOnlyPagedCollection<TGetListDto>> previousResponse)
        {
            var user = GetUser();

            return HandleGetListResponseWithUserAsync(previousResponse, user);
        }

        /// <inheritdoc/>
        public override Task<AuthorizationResult<TEntity, long>> HandleGetSingleRequestAsync(AuthorizationResult<TEntity, long> previousResult)
        {
            var user = GetUser();

            if (!user.Identity?.IsAuthenticated == true)
                return Task.FromResult(previousResult.WithStatus(HttpStatusCode.Unauthorized));

            return HandleGetSingleRequestWithUserAsync(previousResult, user);
        }

        /// <inheritdoc/>
        public override Task<ServiceResponse<TGetFullDto>> HandleGetSingleResponseAsync(ServiceResponse<TGetFullDto> previousResponse)
        {
            var user = GetUser();

            return HandleGetSingleResponseWithUserAsync(previousResponse, user);
        }

        /// <inheritdoc/>
        public override Task<AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>>> HandleUpdateRequestAsync(AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>> previousResult)
        {
            var user = GetUser();

            if (!user.Identity?.IsAuthenticated == true)
                return Task.FromResult(previousResult.WithStatus(HttpStatusCode.Unauthorized));

            return HandleUpdateRequestWithUserAsync(previousResult, user);
        }

        /// <inheritdoc/>
        public override Task<AuthorizationResult<TEntity, TUpdateDto>> HandleUpdateRequestAsync(AuthorizationResult<TEntity, TUpdateDto> previousResult)
        {
            var user = GetUser();

            if (!user.Identity?.IsAuthenticated == true)
                return Task.FromResult(previousResult.WithStatus(System.Net.HttpStatusCode.Unauthorized));

            return HandleUpdateRequestWithUserAsync(previousResult, user);
        }

        /// <inheritdoc/>
        public override Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleUpdateResponseAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse)
        {
            var user = GetUser();

            return HandleUpdateResponseWithUserAsync(previousResponse, user);
        }

        /// <inheritdoc/>
        public override Task<ServiceResponse<TGetFullDto>> HandleUpdateResponseAsync(ServiceResponse<TGetFullDto> previousResponse)
        {
            var user = GetUser();

            return HandleUpdateResponseWithUserAsync(previousResponse, user);
        }

        /// <summary>
        /// Gets the user from the <see cref="UserAccessor"/>.
        /// </summary>
        /// <returns>The current user.</returns>
        protected virtual ClaimsPrincipal GetUser() => UserAccessor.User;

        /// <summary>
        /// This method is the same as <see cref="HandleCreateRequestAsync(AuthorizationResult{TEntity, IReadOnlyCollection{TCreateDto}})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>>> HandleCreateRequestWithUserAsync(AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>> previousResult, ClaimsPrincipal user)
        {
            return base.HandleCreateRequestAsync(previousResult);
        }

        /// <summary>
        /// This method is the same as <see cref="HandleCreateRequestAsync(AuthorizationResult{TEntity, TCreateDto})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<AuthorizationResult<TEntity, TCreateDto>> HandleCreateRequestWithUserAsync(AuthorizationResult<TEntity, TCreateDto> previousResult, ClaimsPrincipal user)
        {
            return base.HandleCreateRequestAsync(previousResult);
        }

        /// <summary>
        /// This method is the same as <see cref="HandleCreateResponseAsync(ServiceResponse{IReadOnlyCollection{TGetFullDto}})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResponse">The previous response.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleCreateResponseWithUserAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse, ClaimsPrincipal user)
        {
            return base.HandleCreateResponseAsync(previousResponse);
        }

        /// <summary>
        /// This method is the same as <see cref="HandleCreateResponseAsync(ServiceResponse{TGetFullDto})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResponse">The previous response.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<ServiceResponse<TGetFullDto>> HandleCreateResponseWithUserAsync(ServiceResponse<TGetFullDto> previousResponse, ClaimsPrincipal user)
        {
            return base.HandleCreateResponseAsync(previousResponse);
        }

        /// <summary>
        /// This method is the same as <see cref="HandleDeleteRequestAsync(AuthorizationResult{TEntity, long, byte[]})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<AuthorizationResult<TEntity, long, byte[]>> HandleDeleteRequestWithUserAsync(AuthorizationResult<TEntity, long, byte[]> previousResult, ClaimsPrincipal user)
        {
            return base.HandleDeleteRequestAsync(previousResult);
        }

        /// <summary>
        /// This method is the same as <see cref="HandleDeleteResponseAsync(ServiceResponse{object})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResponse">The previous response.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<ServiceResponse<object>> HandleDeleteResponseWithUserAsync(ServiceResponse<object> previousResponse, ClaimsPrincipal user)
        {
            return base.HandleDeleteResponseAsync(previousResponse);
        }

        /// <summary>
        /// This method is the same as <see cref="HandleGetListRequestAsync(AuthorizationResult{TEntity, IGetListRequest{TEntity}})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<AuthorizationResult<TEntity, IGetListRequest<TEntity>>> HandleGetListRequestWithUserAsync(AuthorizationResult<TEntity, IGetListRequest<TEntity>> previousResult, ClaimsPrincipal user)
        {
            return base.HandleGetListRequestAsync(previousResult);
        }

        /// <summary>
        /// This method is the same as <see cref="HandleGetListResponseAsync(ServiceResponse{IReadOnlyPagedCollection{TGetListDto}})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResponse">The previous response.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> HandleGetListResponseWithUserAsync(ServiceResponse<IReadOnlyPagedCollection<TGetListDto>> previousResponse, ClaimsPrincipal user)
        {
            return base.HandleGetListResponseAsync(previousResponse);
        }

        /// <summary>
        /// This method is the same as <see cref="HandleGetSingleRequestAsync(AuthorizationResult{TEntity, long})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<AuthorizationResult<TEntity, long>> HandleGetSingleRequestWithUserAsync(AuthorizationResult<TEntity, long> previousResult, ClaimsPrincipal user)
        {
            return base.HandleGetSingleRequestAsync(previousResult);
        }

        /// <summary>
        /// This method is the same as <see cref="HandleGetSingleResponseAsync(ServiceResponse{TGetFullDto})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResponse">The previous response.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<ServiceResponse<TGetFullDto>> HandleGetSingleResponseWithUserAsync(ServiceResponse<TGetFullDto> previousResponse, ClaimsPrincipal user)
        {
            return base.HandleGetSingleResponseAsync(previousResponse);
        }

        /// <summary>
        /// This method is the same as <see cref="HandleUpdateRequestAsync(AuthorizationResult{TEntity, IUpdateMultipleRequest{TUpdateDto, TEntity}})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>>> HandleUpdateRequestWithUserAsync(AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>> previousResult, ClaimsPrincipal user)
        {
            return base.HandleUpdateRequestAsync(previousResult);
        }

        /// <summary>
        /// This method is the same as <see cref="HandleUpdateRequestAsync(AuthorizationResult{TEntity, TUpdateDto})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<AuthorizationResult<TEntity, TUpdateDto>> HandleUpdateRequestWithUserAsync(AuthorizationResult<TEntity, TUpdateDto> previousResult, ClaimsPrincipal user)
        {
            return base.HandleUpdateRequestAsync(previousResult);
        }

        /// <summary>
        /// This method is the same as <see cref="HandleUpdateResponseAsync(ServiceResponse{TGetFullDto})"/> but also gives access to the current <paramref name="user"/>.
        /// </summary>
        /// <param name="previousResponse">The previous response.</param>
        /// <param name="user">The user.</param>
        protected virtual Task<ServiceResponse<TGetFullDto>> HandleUpdateResponseWithUserAsync(ServiceResponse<TGetFullDto> previousResponse, ClaimsPrincipal user)
        {
            return base.HandleUpdateResponseAsync(previousResponse);
        }

        /// <summary>
        /// This method is the same as <see cref="HandleUpdateResponseAsync(ServiceResponse{IReadOnlyCollection{TGetFullDto}})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResponse">The previous response.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleUpdateResponseWithUserAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse, ClaimsPrincipal user)
        {
            return base.HandleUpdateResponseAsync(previousResponse);
        }
    }
}