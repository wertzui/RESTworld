using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using System;
using System.Collections.Generic;
using System.Net;
using System.Security.Claims;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Authorization;

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
/// <seealso cref="CrudAuthorizationHandlerBase&lt;TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto&gt;" />
public class UserIsAuthorizedCrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto> : CrudAuthorizationHandlerBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="UserIsAuthorizedCrudAuthorizationHandler{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/> class.
    /// </summary>
    /// <param name="userAccessor">The user accessor.</param>
    /// <exception cref="ArgumentNullException">userAccessor</exception>
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
    public override Task<AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>>> HandleCreateRequestAsync(AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>> previousResult, CancellationToken cancellationToken)
    {
        var user = GetUser();

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult(previousResult.WithStatus(HttpStatusCode.Unauthorized));

        return HandleCreateRequestWithUserAsync(previousResult, user, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<AuthorizationResult<TEntity, TCreateDto>> HandleCreateRequestAsync(AuthorizationResult<TEntity, TCreateDto> previousResult, CancellationToken cancellationToken)
    {
        var user = GetUser();

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult(previousResult.WithStatus(HttpStatusCode.Unauthorized));

        return HandleCreateRequestWithUserAsync(previousResult, user, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleCreateResponseAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse, CancellationToken cancellationToken)
    {
        var user = GetUser();

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult(ServiceResponse.FromStatus<IReadOnlyCollection<TGetFullDto>>(HttpStatusCode.Unauthorized));

        return HandleCreateResponseWithUserAsync(previousResponse, user, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<ServiceResponse<TGetFullDto>> HandleCreateResponseAsync(ServiceResponse<TGetFullDto> previousResponse, CancellationToken cancellationToken)
    {
        var user = GetUser();

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult(ServiceResponse.FromStatus<TGetFullDto>(HttpStatusCode.Unauthorized));

        return HandleCreateResponseWithUserAsync(previousResponse, user, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<AuthorizationResult<TEntity, long, byte[]>> HandleDeleteRequestAsync(AuthorizationResult<TEntity, long, byte[]> previousResult, CancellationToken cancellationToken)
    {
        var user = GetUser();

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult(previousResult.WithStatus(HttpStatusCode.Unauthorized));

        return HandleDeleteRequestWithUserAsync(previousResult, user, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<ServiceResponse<object>> HandleDeleteResponseAsync(ServiceResponse<object> previousResponse, CancellationToken cancellationToken)
    {
        var user = GetUser();

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult(ServiceResponse.FromStatus<object>(HttpStatusCode.Unauthorized));

        return HandleDeleteResponseWithUserAsync(previousResponse, user, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<AuthorizationResult<TEntity, IGetListRequest<TGetListDto, TEntity>>> HandleGetListRequestAsync(AuthorizationResult<TEntity, IGetListRequest<TGetListDto, TEntity>> previousResult, CancellationToken cancellationToken)
    {
        var user = GetUser();

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult(previousResult.WithStatus(HttpStatusCode.Unauthorized));

        return HandleGetListRequestWithUserAsync(previousResult, user, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> HandleGetListResponseAsync(ServiceResponse<IReadOnlyPagedCollection<TGetListDto>> previousResponse, CancellationToken cancellationToken)
    {
        var user = GetUser();

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult(ServiceResponse.FromStatus<IReadOnlyPagedCollection<TGetListDto>>(HttpStatusCode.Unauthorized));

        return HandleGetListResponseWithUserAsync(previousResponse, user, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<AuthorizationResult<TEntity, long>> HandleGetSingleRequestAsync(AuthorizationResult<TEntity, long> previousResult, CancellationToken cancellationToken)
    {
        var user = GetUser();

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult(previousResult.WithStatus(HttpStatusCode.Unauthorized));

        return HandleGetSingleRequestWithUserAsync(previousResult, user, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<ServiceResponse<TGetFullDto>> HandleGetSingleResponseAsync(ServiceResponse<TGetFullDto> previousResponse, CancellationToken cancellationToken)
    {
        var user = GetUser();

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult(ServiceResponse.FromStatus<TGetFullDto>(HttpStatusCode.Unauthorized));

        return HandleGetSingleResponseWithUserAsync(previousResponse, user, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>>> HandleUpdateRequestAsync(AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>> previousResult, CancellationToken cancellationToken)
    {
        var user = GetUser();

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult(previousResult.WithStatus(HttpStatusCode.Unauthorized));

        return HandleUpdateRequestWithUserAsync(previousResult, user, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<AuthorizationResult<TEntity, TUpdateDto>> HandleUpdateRequestAsync(AuthorizationResult<TEntity, TUpdateDto> previousResult, CancellationToken cancellationToken)
    {
        var user = GetUser();

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult(previousResult.WithStatus(HttpStatusCode.Unauthorized));

        return HandleUpdateRequestWithUserAsync(previousResult, user, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleUpdateResponseAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse, CancellationToken cancellationToken)
    {
        var user = GetUser();

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult(ServiceResponse.FromStatus<IReadOnlyCollection<TGetFullDto>>(HttpStatusCode.Unauthorized));

        return HandleUpdateResponseWithUserAsync(previousResponse, user, cancellationToken);
    }

    /// <inheritdoc/>
    public override Task<ServiceResponse<TGetFullDto>> HandleUpdateResponseAsync(ServiceResponse<TGetFullDto> previousResponse, CancellationToken cancellationToken)
    {
        var user = GetUser();

        if (user is null || user.Identity is null || !user.Identity.IsAuthenticated)
            return Task.FromResult(ServiceResponse.FromStatus<TGetFullDto>(HttpStatusCode.Unauthorized));

        return HandleUpdateResponseWithUserAsync(previousResponse, user, cancellationToken);
    }

    /// <summary>
    /// Gets the user from the <see cref="UserAccessor"/>.
    /// </summary>
    /// <returns>The current user.</returns>
    protected virtual ClaimsPrincipal? GetUser() => UserAccessor.User;

    /// <summary>
    /// This method is the same as <see cref="HandleCreateRequestAsync(AuthorizationResult{TEntity, IReadOnlyCollection{TCreateDto}}, CancellationToken)" /> but also gives access to the current <paramref name="user" />.
    /// </summary>
    /// <param name="previousResult">The previous result.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    protected virtual Task<AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>>> HandleCreateRequestWithUserAsync(AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>> previousResult, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return base.HandleCreateRequestAsync(previousResult, cancellationToken);
    }

    /// <summary>
    /// This method is the same as <see cref="HandleCreateRequestAsync(AuthorizationResult{TEntity, TCreateDto}, CancellationToken)" /> but also gives access to the current <paramref name="user" />.
    /// </summary>
    /// <param name="previousResult">The previous result.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    protected virtual Task<AuthorizationResult<TEntity, TCreateDto>> HandleCreateRequestWithUserAsync(AuthorizationResult<TEntity, TCreateDto> previousResult, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return base.HandleCreateRequestAsync(previousResult, cancellationToken);
    }

    /// <summary>
    /// This method is the same as <see cref="HandleCreateResponseAsync(ServiceResponse{IReadOnlyCollection{TGetFullDto}}, CancellationToken)" /> but also gives access to the current <paramref name="user" />.
    /// </summary>
    /// <param name="previousResponse">The previous response.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    protected virtual Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleCreateResponseWithUserAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return base.HandleCreateResponseAsync(previousResponse, cancellationToken);
    }

    /// <summary>
    /// This method is the same as <see cref="HandleCreateResponseAsync(ServiceResponse{TGetFullDto}, CancellationToken)" /> but also gives access to the current <paramref name="user" />.
    /// </summary>
    /// <param name="previousResponse">The previous response.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    protected virtual Task<ServiceResponse<TGetFullDto>> HandleCreateResponseWithUserAsync(ServiceResponse<TGetFullDto> previousResponse, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return base.HandleCreateResponseAsync(previousResponse, cancellationToken);
    }

    /// <summary>
    /// This method is the same as <see cref="HandleDeleteRequestAsync(AuthorizationResult{TEntity, long, byte[]}, CancellationToken)" /> but also gives access to the current <paramref name="user" />.
    /// </summary>
    /// <param name="previousResult">The previous result.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    protected virtual Task<AuthorizationResult<TEntity, long, byte[]>> HandleDeleteRequestWithUserAsync(AuthorizationResult<TEntity, long, byte[]> previousResult, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return base.HandleDeleteRequestAsync(previousResult, cancellationToken);
    }

    /// <summary>
    /// This method is the same as <see cref="HandleDeleteResponseAsync(ServiceResponse{object}, CancellationToken)" /> but also gives access to the current <paramref name="user" />.
    /// </summary>
    /// <param name="previousResponse">The previous response.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    protected virtual Task<ServiceResponse<object>> HandleDeleteResponseWithUserAsync(ServiceResponse<object> previousResponse, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return base.HandleDeleteResponseAsync(previousResponse, cancellationToken);
    }

    /// <summary>
    /// This method is the same as <see cref="HandleGetListRequestAsync(AuthorizationResult{TEntity, IGetListRequest{TGetListDto, TEntity}}, CancellationToken)" /> but also gives access to the current <paramref name="user" />.
    /// </summary>
    /// <param name="previousResult">The previous result.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    protected virtual Task<AuthorizationResult<TEntity, IGetListRequest<TGetListDto, TEntity>>> HandleGetListRequestWithUserAsync(AuthorizationResult<TEntity, IGetListRequest<TGetListDto, TEntity>> previousResult, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return base.HandleGetListRequestAsync(previousResult, cancellationToken);
    }

    /// <summary>
    /// This method is the same as <see cref="HandleGetListResponseAsync(ServiceResponse{IReadOnlyPagedCollection{TGetListDto}}, CancellationToken)" /> but also gives access to the current <paramref name="user" />.
    /// </summary>
    /// <param name="previousResponse">The previous response.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    protected virtual Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> HandleGetListResponseWithUserAsync(ServiceResponse<IReadOnlyPagedCollection<TGetListDto>> previousResponse, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return base.HandleGetListResponseAsync(previousResponse, cancellationToken);
    }

    /// <summary>
    /// This method is the same as <see cref="HandleGetSingleRequestAsync(AuthorizationResult{TEntity, long}, CancellationToken)" /> but also gives access to the current <paramref name="user" />.
    /// </summary>
    /// <param name="previousResult">The previous result.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    protected virtual Task<AuthorizationResult<TEntity, long>> HandleGetSingleRequestWithUserAsync(AuthorizationResult<TEntity, long> previousResult, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return base.HandleGetSingleRequestAsync(previousResult, cancellationToken);
    }

    /// <summary>
    /// This method is the same as <see cref="HandleGetSingleResponseAsync(ServiceResponse{TGetFullDto}, CancellationToken)" /> but also gives access to the current <paramref name="user" />.
    /// </summary>
    /// <param name="previousResponse">The previous response.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    protected virtual Task<ServiceResponse<TGetFullDto>> HandleGetSingleResponseWithUserAsync(ServiceResponse<TGetFullDto> previousResponse, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return base.HandleGetSingleResponseAsync(previousResponse, cancellationToken);
    }

    /// <summary>
    /// This method is the same as <see cref="HandleUpdateRequestAsync(AuthorizationResult{TEntity, IUpdateMultipleRequest{TUpdateDto, TEntity}}, CancellationToken)" /> but also gives access to the current <paramref name="user" />.
    /// </summary>
    /// <param name="previousResult">The previous result.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    protected virtual Task<AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>>> HandleUpdateRequestWithUserAsync(AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>> previousResult, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return base.HandleUpdateRequestAsync(previousResult, cancellationToken);
    }

    /// <summary>
    /// This method is the same as <see cref="HandleUpdateRequestAsync(AuthorizationResult{TEntity, TUpdateDto}, CancellationToken)" /> but also gives access to the current <paramref name="user" />.
    /// </summary>
    /// <param name="previousResult">The previous result.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    protected virtual Task<AuthorizationResult<TEntity, TUpdateDto>> HandleUpdateRequestWithUserAsync(AuthorizationResult<TEntity, TUpdateDto> previousResult, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return base.HandleUpdateRequestAsync(previousResult, cancellationToken);
    }

    /// <summary>
    /// This method is the same as <see cref="HandleUpdateResponseAsync(ServiceResponse{TGetFullDto}, CancellationToken)"/> but also gives access to the current <paramref name="user"/>.
    /// </summary>
    /// <param name="previousResponse">The previous response.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    protected virtual Task<ServiceResponse<TGetFullDto>> HandleUpdateResponseWithUserAsync(ServiceResponse<TGetFullDto> previousResponse, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return base.HandleUpdateResponseAsync(previousResponse, cancellationToken);
    }

    /// <summary>
    /// This method is the same as <see cref="HandleUpdateResponseAsync(ServiceResponse{IReadOnlyCollection{TGetFullDto}}, CancellationToken)" /> but also gives access to the current <paramref name="user" />.
    /// </summary>
    /// <param name="previousResponse">The previous response.</param>
    /// <param name="user">The user.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns></returns>
    protected virtual Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleUpdateResponseWithUserAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse, ClaimsPrincipal user, CancellationToken cancellationToken)
    {
        return base.HandleUpdateResponseAsync(previousResponse, cancellationToken);
    }
}