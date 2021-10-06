﻿using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using System;
using System.Net;
using System.Security.Claims;
using System.Threading.Tasks;

namespace RESTworld.Business.Authorization
{
    /// <summary>
    /// This authorization handler gets the current user and provides overidable methods for request and response handling which also have the user as an input parameter.
    /// It will ensure that the user is authenticated for every request.
    /// It will not check if the user is authenticated during response handling as this is not needed, because the user cannot change between a request and the associated response.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TRequest">The type of the request.</typeparam>
    /// <typeparam name="TResponse">The type of the response.</typeparam>
    /// <seealso cref="BasicAuthorizationHandlerBase&lt;TEntity, TRequest, TResponse&gt;" />
    public class UserIsAuthorizedBasicAuthorizationHandler<TEntity, TRequest, TResponse> : BasicAuthorizationHandlerBase<TEntity, TRequest, TResponse>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="UserIsAuthorizedBasicAuthorizationHandler{TEntity, TRequest, TResponse}"/> class.
        /// </summary>
        /// <param name="userAccessor">The user accessor.</param>
        /// <exception cref="ArgumentNullException">userAccessor</exception>
        public UserIsAuthorizedBasicAuthorizationHandler(IUserAccessor userAccessor)
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
        public override Task<AuthorizationResult<TEntity, TRequest>> HandleRequestAsync(AuthorizationResult<TEntity, TRequest> previousResult)
        {
            var user = GetUser();

            if (!user.Identity.IsAuthenticated)
                return Task.FromResult(previousResult.WithStatus(HttpStatusCode.Unauthorized));

            return HandleRequestWithUserAsync(previousResult, user);
        }

        /// <inheritdoc/>
        public override Task<ServiceResponse<TResponse>> HandleResponseAsync(ServiceResponse<TResponse> previousResponse)
        {
            var user = GetUser();

            return HandleResponseWithUserAsync(previousResponse, user);
        }

        /// <summary>
        /// Gets the user from the <see cref="UserAccessor"/>.
        /// </summary>
        /// <returns>The current user.</returns>
        protected virtual ClaimsPrincipal GetUser() => UserAccessor.User;

        /// <summary>
        /// This method is the same as <see cref="HandleRequestAsync(AuthorizationResult{TEntity, TRequest})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<AuthorizationResult<TEntity, TRequest>> HandleRequestWithUserAsync(AuthorizationResult<TEntity, TRequest> previousResult, ClaimsPrincipal user)
        {
            return base.HandleRequestAsync(previousResult);
        }

        /// <summary>
        /// This method is the same as <see cref="HandleResponseAsync(ServiceResponse{TResponse})" /> but also gives access to the current <paramref name="user" />.
        /// </summary>
        /// <param name="previousResponse">The previous response.</param>
        /// <param name="user">The user.</param>
        /// <returns></returns>
        protected virtual Task<ServiceResponse<TResponse>> HandleResponseWithUserAsync(ServiceResponse<TResponse> previousResponse, ClaimsPrincipal user)
        {
            return base.HandleResponseAsync(previousResponse);
        }
    }
}