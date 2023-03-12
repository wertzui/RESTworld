using AutoMapper;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RESTworld.Business.Authorization;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using RESTworld.EntityFrameworkCore;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Services
{
    /// <summary>
    /// Serves as a base class for services.
    /// It provides logging, mapping and most important authorization logic and exception handling.
    /// Whenever you implement your own service method, call <see cref="TryExecuteWithAuthorizationAsync{T1, TResponse, TAuthorizationHandler}(T1, Func{AuthorizationResultWithoutDb{T1}, CancellationToken, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResultWithoutDb{T1}, TAuthorizationHandler, CancellationToken, Task{AuthorizationResultWithoutDb{T1}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, CancellationToken, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler}, CancellationToken)"/> so authorization and error handling is executed.
    /// </summary>
    public abstract class ServiceBase
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected readonly ILogger _logger;
        protected readonly IMapper _mapper;
        protected readonly IUserAccessor _userAccessor;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceBase"/> class.
        /// </summary>
        /// <param name="mapper">The mapper.</param>
        /// <param name="userAccessor">The user accessor.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">
        /// mapper
        /// or
        /// userAccessor
        /// or
        /// logger
        /// </exception>
        public ServiceBase(
            IMapper mapper,
            IUserAccessor userAccessor,
            ILogger logger)
        {
            _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
            _userAccessor = userAccessor ?? throw new ArgumentNullException(nameof(userAccessor));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        /// <summary>
        /// Calls the Handle...RequestAsync for all <paramref name="authorizationHandlers"/> with one parameter.
        /// </summary>
        /// <typeparam name="T1">The type of the parameter.</typeparam>
        /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
        /// <param name="param1">The parameter.</param>
        /// <param name="authorizeRequest">Defines which Handle...RequestAsync method to call.</param>
        /// <param name="authorizationHandlers">The authorization handlers.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// The result of the authorization. May contain a failed authorization or a modified parameter.
        /// </returns>
        protected virtual async Task<AuthorizationResultWithoutDb<T1>> AuthorizeRequestAsync<T1, TAuthorizationHandler>(
            T1 param1,
            Func<AuthorizationResultWithoutDb<T1>, TAuthorizationHandler, CancellationToken, Task<AuthorizationResultWithoutDb<T1>>> authorizeRequest,
            IEnumerable<TAuthorizationHandler> authorizationHandlers,
            CancellationToken cancellationToken)
            where TAuthorizationHandler : IAuthorizationHandler
        {
            var result = AuthorizationResultWithoutDb.Ok(param1);

            foreach (var handler in authorizationHandlers)
            {
                result = await authorizeRequest(result, handler, cancellationToken);
            }

            return result;
        }

        /// <summary>
        /// Calls the Handle...RequestAsync for all <paramref name="authorizationHandlers"/> with two parameters.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
        /// <param name="param1">The first parameter.</param>
        /// <param name="param2">The second parameter.</param>
        /// <param name="authorizeRequest">Defines which Handle...RequestAsync method to call.</param>
        /// <param name="authorizationHandlers">The authorization handlers.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// The result of the authorization. May contain a failed authorization or modified parameters.
        /// </returns>
        protected virtual async Task<AuthorizationResultWithoutDb<T1, T2>> AuthorizeRequestAsync<T1, T2, TAuthorizationHandler>(
            T1 param1,
            T2 param2,
            Func<AuthorizationResultWithoutDb<T1, T2>, TAuthorizationHandler, CancellationToken, Task<AuthorizationResultWithoutDb<T1, T2>>> authorizeRequest,
            IEnumerable<TAuthorizationHandler> authorizationHandlers,
            CancellationToken cancellationToken)
            where TAuthorizationHandler : IAuthorizationHandler
        {
            var result = AuthorizationResultWithoutDb.Ok(param1, param2);

            foreach (var handler in authorizationHandlers)
            {
                result = await authorizeRequest(result, handler, cancellationToken);
            }

            return result;
        }

        /// <summary>
        /// Calls the Handle...ResultAsync for all <paramref name="authorizationHandlers"/>.
        /// </summary>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
        /// <param name="response">The response.</param>
        /// <param name="authorizeResult">Defines which Handle...ResultAsync method to call.</param>
        /// <param name="authorizationHandlers">The authorization handlers.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// The result of the authorization. May contain a failed authorization or a modified result.
        /// </returns>
        protected virtual async Task<ServiceResponse<TResponse>> AuthorizeResultAsync<TResponse, TAuthorizationHandler>(
            ServiceResponse<TResponse> response,
            Func<ServiceResponse<TResponse>, TAuthorizationHandler, CancellationToken, Task<ServiceResponse<TResponse>>> authorizeResult,
            IEnumerable<TAuthorizationHandler> authorizationHandlers,
            CancellationToken cancellationToken)
            where TAuthorizationHandler : IAuthorizationHandler
        {
            var result = response;

            foreach (var handler in authorizationHandlers)
            {
                result = await authorizeResult(response, handler, cancellationToken);
            }

            return result;
        }

        /// <summary>
        /// Gets the name of the current user which is used when calling <see cref="DbContextBase.SaveChangesAsync(string, System.Threading.CancellationToken)"/>.
        /// This is the user name which gets stored in the <see cref="ChangeTrackingEntityBase.CreatedBy"/> and <see cref="ChangeTrackingEntityBase.LastChangedBy"/> properties.
        /// </summary>
        /// <returns>The name of the current user as defined by the <see cref="IUserAccessor.User"/></returns>
        protected virtual string? GetCurrentUsersName() => _userAccessor?.User?.Identity?.Name;

        /// <summary>
        /// Tries to execute the given function while checking for errors and wrapping any exceptions into a problem ServiceResponse.
        /// It will first check for any pending database migrations. If there are any, a 503 (Service unavailable) is returned.
        /// If a <see cref="DbUpdateConcurrencyException"/> occurs during the execution, this will be wrapped into a 409 (Conflict) ServiceResponse.
        /// If any other exception occurs, this will be wrapped into a 500 (Internal server error) ServiceResponse.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>Either the result of the call or a ServiceResponse describing the problem.</returns>
        protected virtual async Task<ServiceResponse<T>> TryExecuteAsync<T>(Func<CancellationToken, Task<ServiceResponse<T>>> function, CancellationToken cancellationToken)
        {
            try
            {
                return await function(cancellationToken);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while executing a service call");
                return ServiceResponse.FromException<T>(e);
            }
        }

        /// <summary>
        /// Tries to execute a <paramref name="function" /> which accepts one parameter while performing <paramref name="authorizeRequest" /> before and <paramref name="authorizeResult" /> after the main invocation.
        /// This method combines <see cref="TryExecuteAsync{T}(Func{CancellationToken, Task{ServiceResponse{T}}}, CancellationToken)" /> with <see cref="WithAuthorizationAsync{T1, TResponse, TAuthorizationHandler}(T1, Func{AuthorizationResultWithoutDb{T1}, CancellationToken, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResultWithoutDb{T1}, TAuthorizationHandler, CancellationToken, Task{AuthorizationResultWithoutDb{T1}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, CancellationToken, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler}, CancellationToken)" />.
        /// </summary>
        /// <typeparam name="T1">The type of the parameter.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
        /// <param name="param1">The first parameter.</param>
        /// <param name="function">The main function to execute.</param>
        /// <param name="authorizeRequest">The authorization request function which is executed before the main function.</param>
        /// <param name="authorizeResult">The authorization result function which is executed after the main function.</param>
        /// <param name="authorizationHandlers">The authorization handlers.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        /// <seealso cref="TryExecuteAsync{T}(Func{CancellationToken, Task{ServiceResponse{T}}}, CancellationToken)" />
        /// <seealso cref="WithAuthorizationAsync{T1, TResponse, TAuthorizationHandler}(T1, Func{AuthorizationResultWithoutDb{T1}, CancellationToken, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResultWithoutDb{T1}, TAuthorizationHandler, CancellationToken, Task{AuthorizationResultWithoutDb{T1}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, CancellationToken, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler}, CancellationToken)" />
        protected virtual Task<ServiceResponse<TResponse>> TryExecuteWithAuthorizationAsync<T1, TResponse, TAuthorizationHandler>(
            T1 param1,
            Func<AuthorizationResultWithoutDb<T1>, CancellationToken, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResultWithoutDb<T1>, TAuthorizationHandler, CancellationToken, Task<AuthorizationResultWithoutDb<T1>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, TAuthorizationHandler, CancellationToken, Task<ServiceResponse<TResponse>>> authorizeResult,
            IEnumerable<TAuthorizationHandler> authorizationHandlers,
            CancellationToken cancellationToken)
            where TAuthorizationHandler : IAuthorizationHandler
            => TryExecuteAsync(token => WithAuthorizationAsync(param1, function, authorizeRequest, authorizeResult, authorizationHandlers, token), cancellationToken);

        /// <summary>
        /// Tries to execute a <paramref name="function" /> which accepts two parameters while performing <paramref name="authorizeRequest" /> before and <paramref name="authorizeResult" /> after the main invocation.
        /// This method combines <see cref="TryExecuteAsync{T}(Func{CancellationToken, Task{ServiceResponse{T}}}, CancellationToken)" /> with <see cref="WithAuthorizationAsync{T1, T2, TResponse, TAuthorizationHandler}(T1, T2, Func{AuthorizationResultWithoutDb{T1, T2}, CancellationToken, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResultWithoutDb{T1, T2}, TAuthorizationHandler, CancellationToken, Task{AuthorizationResultWithoutDb{T1, T2}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, CancellationToken, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler}, CancellationToken)" />.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
        /// <param name="param1">The first parameter.</param>
        /// <param name="param2">The second parameter.</param>
        /// <param name="function">The main function to execute.</param>
        /// <param name="authorizeRequest">The authorization request function which is executed before the main function.</param>
        /// <param name="authorizeResult">The authorization result function which is executed after the main function.</param>
        /// <param name="authorizationHandlers">The authorization handlers.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        /// <seealso cref="TryExecuteAsync{T}(Func{CancellationToken, Task{ServiceResponse{T}}}, CancellationToken)" />
        /// <seealso cref="WithAuthorizationAsync{T1, T2, TResponse, TAuthorizationHandler}(T1, T2, Func{AuthorizationResultWithoutDb{T1, T2}, CancellationToken, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResultWithoutDb{T1, T2}, TAuthorizationHandler, CancellationToken, Task{AuthorizationResultWithoutDb{T1, T2}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, CancellationToken, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler}, CancellationToken)"/>
        protected virtual Task<ServiceResponse<TResponse>> TryExecuteWithAuthorizationAsync<T1, T2, TResponse, TAuthorizationHandler>(
            T1 param1,
            T2 param2,
            Func<AuthorizationResultWithoutDb<T1, T2>, CancellationToken, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResultWithoutDb<T1, T2>, TAuthorizationHandler, CancellationToken, Task<AuthorizationResultWithoutDb<T1, T2>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, TAuthorizationHandler, CancellationToken, Task<ServiceResponse<TResponse>>> authorizeResult,
            IEnumerable<TAuthorizationHandler> authorizationHandlers,
            CancellationToken cancellationToken)
            where TAuthorizationHandler : IAuthorizationHandler
            => TryExecuteAsync(token => WithAuthorizationAsync(param1, param2, function, authorizeRequest, authorizeResult, authorizationHandlers, token), cancellationToken);

        /// <summary>
        /// Executes a <paramref name="function" /> which accepts one parameter while performing <paramref name="authorizeRequest" /> before and <paramref name="authorizeResult" /> after the main invocation.
        /// </summary>
        /// <typeparam name="T1">The type of the parameter.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
        /// <param name="param1">The first parameter.</param>
        /// <param name="function">The main function to execute.</param>
        /// <param name="authorizeRequest">The authorization request function which is executed before the main function.</param>
        /// <param name="authorizeResult">The authorization result function which is executed after the main function.</param>
        /// <param name="authorizationHandlers">The authorization handlers.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        protected virtual async Task<ServiceResponse<TResponse>> WithAuthorizationAsync<T1, TResponse, TAuthorizationHandler>(
            T1 param1,
            Func<AuthorizationResultWithoutDb<T1>, CancellationToken, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResultWithoutDb<T1>, TAuthorizationHandler, CancellationToken, Task<AuthorizationResultWithoutDb<T1>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, TAuthorizationHandler, CancellationToken, Task<ServiceResponse<TResponse>>> authorizeResult,
            IEnumerable<TAuthorizationHandler> authorizationHandlers,
            CancellationToken cancellationToken)
            where TAuthorizationHandler : IAuthorizationHandler
        {
            var requestAuthResult = await AuthorizeRequestAsync(param1, authorizeRequest, authorizationHandlers, cancellationToken);

            if (requestAuthResult.Status != HttpStatusCode.OK)
                return ServiceResponse.FromStatus<TResponse>(requestAuthResult.Status);

            var serviceCallResponse = await function(requestAuthResult, cancellationToken);

            var resultAuthResponse = await AuthorizeResultAsync(serviceCallResponse, authorizeResult, authorizationHandlers, cancellationToken);

            return resultAuthResponse;
        }

        /// <summary>
        /// Executes a <paramref name="function" /> which accepts two parameters while performing <paramref name="authorizeRequest" /> before and <paramref name="authorizeResult" /> after the main invocation.
        /// </summary>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
        /// <param name="param1">The first parameter.</param>
        /// <param name="param2">The second parameter.</param>
        /// <param name="function">The main function to execute.</param>
        /// <param name="authorizeRequest">The authorization request function which is executed before the main function.</param>
        /// <param name="authorizeResult">The authorization result function which is executed after the main function.</param>
        /// <param name="authorizationHandlers">The authorization handlers.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        protected virtual async Task<ServiceResponse<TResponse>> WithAuthorizationAsync<T1, T2, TResponse, TAuthorizationHandler>(
            T1 param1,
            T2 param2,
            Func<AuthorizationResultWithoutDb<T1, T2>, CancellationToken, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResultWithoutDb<T1, T2>, TAuthorizationHandler, CancellationToken, Task<AuthorizationResultWithoutDb<T1, T2>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, TAuthorizationHandler, CancellationToken, Task<ServiceResponse<TResponse>>> authorizeResult,
            IEnumerable<TAuthorizationHandler> authorizationHandlers,
            CancellationToken cancellationToken)
            where TAuthorizationHandler : IAuthorizationHandler
        {
            var requestAuthResult = await AuthorizeRequestAsync(param1, param2, authorizeRequest, authorizationHandlers, cancellationToken);

            if (requestAuthResult.Status != HttpStatusCode.OK)
                return ServiceResponse.FromStatus<TResponse>(requestAuthResult.Status);

            var serviceCallResponse = await function(requestAuthResult, cancellationToken);

            var resultAuthResponse = await AuthorizeResultAsync(serviceCallResponse, authorizeResult, authorizationHandlers, cancellationToken);

            return resultAuthResponse;
        }
    }
}