using AutoMapper;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RESTworld.Business.Authorization;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using RESTworld.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Net;
using System.Text.RegularExpressions;
using System.Threading.Tasks;

namespace RESTworld.Business.Services
{
    /// <summary>
    /// Serves as base class for services which make calls to the database.
    /// It provides logging, mapping, migration testing and most important authorization logic and exception handling.
    /// Whenever you implement your own service method, call <see cref="DbServiceBase{TContext}.TryExecuteWithAuthorizationAsync{TEntity, T1, T2, TResponse, TAuthorizationHandler}(T1, T2, Func{AuthorizationResult{TEntity, T1, T2}, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResult{TEntity, T1, T2}, TAuthorizationHandler, Task{AuthorizationResult{TEntity, T1, T2}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler})"/> so authorization and error handling is executed.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <seealso cref="RESTworld.Business.Services.ServiceBase" />
    public abstract class DbServiceBase<TContext> : ServiceBase
        where TContext : DbContextBase
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected static bool _databaseIsMigratedToLatestVersion;
        protected readonly IDbContextFactory<TContext> _contextFactory;
        protected static readonly Regex _foreignKeyRegex = new(@"FOREIGN KEY constraint ""(?<ForeignKeyName>(FK_(?<ForeignTable>\w+)_(?<PrimaryTable>\w+)_(?<ForeignKey>\w+))|[^""]+)""");
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member



        /// <summary>
        /// Initializes a new instance of the <see cref="DbServiceBase{TContext}"/> class.
        /// </summary>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="userAccessor">The user accessor.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="System.ArgumentNullException">contextFactory</exception>
        public DbServiceBase(
            IDbContextFactory<TContext> contextFactory,
            IMapper mapper,
            IUserAccessor userAccessor,
            ILogger logger)
            : base(mapper, userAccessor, logger)
        {
            _contextFactory = contextFactory ?? throw new ArgumentNullException(nameof(contextFactory));
        }


        /// <summary>
        /// Gets all pending migrations if the database is relational.
        /// You may want to override this method if you do not want to execute migrations during application startup, but still want to access the database.
        /// </summary>
        /// <returns>All pending migrations.</returns>
        protected virtual async Task<IEnumerable<string>> GetPendingMigrationsAsync()
        {
            await using var context = _contextFactory.CreateDbContext();

            if (context.Database.IsRelational())
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync();
                return pendingMigrations;
            }

            return Array.Empty<string>();
        }

        /// <summary>
        /// Tries to execute the given function while checking for errors and wrapping any exceptions into a problem ServiceResponse.
        /// It will first check for any pending database migrations. If there are any, a 503 (Service unavailable) is returned.
        /// If a <see cref="DbUpdateConcurrencyException"/> occurs during the execution, this will be wrapped into a 409 (Conflict) ServiceResponse.
        /// If any other exception occurs, this will be wrapped into a 500 (Internal server error) ServiceResponse.
        /// </summary>
        /// <typeparam name="T">The type of the result.</typeparam>
        /// <param name="function">The function to execute.</param>
        /// <returns>Either the result of the call or a ServiceResponse describing the problem.</returns>
        protected override async Task<ServiceResponse<T>> TryExecuteAsync<T>(Func<Task<ServiceResponse<T>>> function)
        {
            try
            {
                if (!_databaseIsMigratedToLatestVersion)
                {
                    var pendingMigrations = await GetPendingMigrationsAsync();
                    _databaseIsMigratedToLatestVersion = !System.Linq.Enumerable.Any(pendingMigrations);
                    if (!_databaseIsMigratedToLatestVersion)
                        return ServiceResponse.FromProblem<T>(HttpStatusCode.ServiceUnavailable, $"The following migrations are still pending for {typeof(TContext).Name}:{Environment.NewLine}{string.Join(Environment.NewLine, pendingMigrations)}");
                }

                return await function();
            }
            catch (DbUpdateConcurrencyException e)
            {
                return ServiceResponse.FromException<T>(HttpStatusCode.Conflict, e);
            }
            catch (DbUpdateException e) when (e.InnerException is SqlException se && se.Number == 547)
            {
                return GetForeignKeyExceptionResponse<T>(se);
            }
            catch (Exception e)
            {
                _logger.LogError(e, "Error while executing a service call");
                return ServiceResponse.FromException<T>(e);
            }
        }



        /// <summary>
        /// Calls the Handle...RequestAsync for all <paramref name="authorizationHandlers"/> with one parameter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the parameter.</typeparam>
        /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
        /// <param name="param1">The parameter.</param>
        /// <param name="authorizeRequest">Defines which Handle...RequestAsync method to call.</param>
        /// <param name="authorizationHandlers">The authorization handlers.</param>
        /// <returns>
        /// The result of the authorization. May contain a failed authorization or a modified parameter.
        /// </returns>
        protected virtual async Task<AuthorizationResult<TEntity, T1>> AuthorizeRequestAsync<TEntity, T1, TAuthorizationHandler>(
            T1 param1,
            Func<AuthorizationResult<TEntity, T1>, TAuthorizationHandler, Task<AuthorizationResult<TEntity, T1>>> authorizeRequest,
            IEnumerable<TAuthorizationHandler> authorizationHandlers)
            where TAuthorizationHandler : IAuthorizationHandler
        {
            var result = AuthorizationResult.Ok<TEntity, T1>(param1);

            foreach (var handler in authorizationHandlers)
            {
                result = await authorizeRequest(result, handler);
            }

            return result;
        }

        /// <summary>
        /// Calls the Handle...RequestAsync for all <paramref name="authorizationHandlers"/> with two parameters.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the first parameter.</typeparam>
        /// <typeparam name="T2">The type of the second parameter.</typeparam>
        /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
        /// <param name="param1">The first parameter.</param>
        /// <param name="param2">The second parameter.</param>
        /// <param name="authorizeRequest">Defines which Handle...RequestAsync method to call.</param>
        /// <param name="authorizationHandlers">The authorization handlers.</param>
        /// <returns>
        /// The result of the authorization. May contain a failed authorization or modified parameters.
        /// </returns>
        protected virtual async Task<AuthorizationResult<TEntity, T1, T2>> AuthorizeRequestAsync<TEntity, T1, T2, TAuthorizationHandler>(
            T1 param1,
            T2 param2,
            Func<AuthorizationResult<TEntity, T1, T2>, TAuthorizationHandler, Task<AuthorizationResult<TEntity, T1, T2>>> authorizeRequest,
            IEnumerable<TAuthorizationHandler> authorizationHandlers)
            where TAuthorizationHandler : IAuthorizationHandler
        {
            var result = AuthorizationResult.Ok<TEntity, T1, T2>(param1, param2);

            foreach (var handler in authorizationHandlers)
            {
                result = await authorizeRequest(result, handler);
            }

            return result;
        }

        /// <summary>
        /// Tries to execute a <paramref name="function" /> which accepts one parameter while performing <paramref name="authorizeRequest" /> before and <paramref name="authorizeResult" /> after the main invocation.
        /// This method combines <see cref="TryExecuteAsync{T}(Func{Task{ServiceResponse{T}}})" /> with <see cref="WithAuthorizationAsync{TEntity, T1, TResponse, TAuthorizationHandler}(T1, Func{AuthorizationResult{TEntity, T1}, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResult{TEntity, T1}, TAuthorizationHandler, Task{AuthorizationResult{TEntity, T1}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler})" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the parameter.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
        /// <param name="param1">The first parameter.</param>
        /// <param name="function">The main function to execute.</param>
        /// <param name="authorizeRequest">The authorization request function which is executed before the main function.</param>
        /// <param name="authorizeResult">The authorization result function which is executed after the main function.</param>
        /// <param name="authorizationHandlers">The authorization handlers.</param>
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        /// <seealso cref="TryExecuteAsync{T}(Func{Task{ServiceResponse{T}}})" />
        /// <seealso cref="WithAuthorizationAsync{TEntity, T1, TResponse, TAuthorizationHandler}(T1, Func{AuthorizationResult{TEntity, T1}, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResult{TEntity, T1}, TAuthorizationHandler, Task{AuthorizationResult{TEntity, T1}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler})" />
        protected virtual Task<ServiceResponse<TResponse>> TryExecuteWithAuthorizationAsync<TEntity, T1, TResponse, TAuthorizationHandler>(
            T1 param1,
            Func<AuthorizationResult<TEntity, T1>, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResult<TEntity, T1>, TAuthorizationHandler, Task<AuthorizationResult<TEntity, T1>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, TAuthorizationHandler, Task<ServiceResponse<TResponse>>> authorizeResult,
            IEnumerable<TAuthorizationHandler> authorizationHandlers)
            where TAuthorizationHandler : IAuthorizationHandler
            => TryExecuteAsync(() => WithAuthorizationAsync(param1, function, authorizeRequest, authorizeResult, authorizationHandlers));

        /// <summary>
        /// Tries to execute a <paramref name="function" /> which accepts two parameters while performing <paramref name="authorizeRequest" /> before and <paramref name="authorizeResult" /> after the main invocation.
        /// This method combines <see cref="TryExecuteAsync{T}(Func{Task{ServiceResponse{T}}})" /> with <see cref="WithAuthorizationAsync{TEntity, T1, T2, TResponse, TAuthorizationHandler}(T1, T2, Func{AuthorizationResult{TEntity, T1, T2}, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResult{TEntity, T1, T2}, TAuthorizationHandler, Task{AuthorizationResult{TEntity, T1, T2}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler})" />.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
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
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        /// <seealso cref="TryExecuteAsync{T}(Func{Task{ServiceResponse{T}}})" />
        /// <seealso cref="WithAuthorizationAsync{TEntity, T1, T2, TResponse, TAuthorizationHandler}(T1, T2, Func{AuthorizationResult{TEntity, T1, T2}, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResult{TEntity, T1, T2}, TAuthorizationHandler, Task{AuthorizationResult{TEntity, T1, T2}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler})" />
        protected virtual Task<ServiceResponse<TResponse>> TryExecuteWithAuthorizationAsync<TEntity, T1, T2, TResponse, TAuthorizationHandler>(
            T1 param1,
            T2 param2,
            Func<AuthorizationResult<TEntity, T1, T2>, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResult<TEntity, T1, T2>, TAuthorizationHandler, Task<AuthorizationResult<TEntity, T1, T2>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, TAuthorizationHandler, Task<ServiceResponse<TResponse>>> authorizeResult,
            IEnumerable<TAuthorizationHandler> authorizationHandlers)
            where TAuthorizationHandler : IAuthorizationHandler
            => TryExecuteAsync(() => WithAuthorizationAsync(param1, param2, function, authorizeRequest, authorizeResult, authorizationHandlers));

        /// <summary>
        /// Executes a <paramref name="function" /> which accepts one parameter while performing <paramref name="authorizeRequest" /> before and <paramref name="authorizeResult" /> after the main invocation.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the parameter.</typeparam>
        /// <typeparam name="TResponse">The type of the response.</typeparam>
        /// <typeparam name="TAuthorizationHandler">The type of the authorization handler.</typeparam>
        /// <param name="param1">The first parameter.</param>
        /// <param name="function">The main function to execute.</param>
        /// <param name="authorizeRequest">The authorization request function which is executed before the main function.</param>
        /// <param name="authorizeResult">The authorization result function which is executed after the main function.</param>
        /// <param name="authorizationHandlers">The authorization handlers.</param>
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        protected virtual async Task<ServiceResponse<TResponse>> WithAuthorizationAsync<TEntity, T1, TResponse, TAuthorizationHandler>(
            T1 param1,
            Func<AuthorizationResult<TEntity, T1>, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResult<TEntity, T1>, TAuthorizationHandler, Task<AuthorizationResult<TEntity, T1>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, TAuthorizationHandler, Task<ServiceResponse<TResponse>>> authorizeResult,
            IEnumerable<TAuthorizationHandler> authorizationHandlers)
            where TAuthorizationHandler : IAuthorizationHandler
        {
            var requestAuthResult = await AuthorizeRequestAsync(param1, authorizeRequest, authorizationHandlers);

            if (requestAuthResult.Status != HttpStatusCode.OK)
                return ServiceResponse.FromStatus<TResponse>(requestAuthResult.Status);

            var serviceCallResponse = await function(requestAuthResult);

            var resultAuthResponse = await AuthorizeResultAsync(serviceCallResponse, authorizeResult, authorizationHandlers);

            return resultAuthResponse;
        }

        /// <summary>
        /// Executes a <paramref name="function" /> which accepts two parameters while performing <paramref name="authorizeRequest" /> before and <paramref name="authorizeResult" /> after the main invocation.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
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
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        protected virtual async Task<ServiceResponse<TResponse>> WithAuthorizationAsync<TEntity, T1, T2, TResponse, TAuthorizationHandler>(
            T1 param1,
            T2 param2,
            Func<AuthorizationResult<TEntity, T1, T2>, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResult<TEntity, T1, T2>, TAuthorizationHandler, Task<AuthorizationResult<TEntity, T1, T2>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, TAuthorizationHandler, Task<ServiceResponse<TResponse>>> authorizeResult,
            IEnumerable<TAuthorizationHandler> authorizationHandlers)
            where TAuthorizationHandler : IAuthorizationHandler
        {
            var requestAuthResult = await AuthorizeRequestAsync(param1, param2, authorizeRequest, authorizationHandlers);

            if (requestAuthResult.Status != HttpStatusCode.OK)
                return ServiceResponse.FromStatus<TResponse>(requestAuthResult.Status);

            var serviceCallResponse = await function(requestAuthResult);

            var resultAuthResponse = await AuthorizeResultAsync(serviceCallResponse, authorizeResult, authorizationHandlers);

            return resultAuthResponse;
        }

        private static ServiceResponse<T> GetForeignKeyExceptionResponse<T>(SqlException e)
        {
            if (e.Message is null)
                return ServiceResponse.FromException<T>(e);

            var match = _foreignKeyRegex.Match(e.Message);
            if (!match.Success)
                return ServiceResponse.FromException<T>(e);

            if (match.Groups.TryGetValue("PrimaryTable", out var primaryTable))
                return ServiceResponse.FromProblem<T>(HttpStatusCode.Conflict, $"Invalid relationship. '{primaryTable}' was not found.");

            if (match.Groups.TryGetValue("ForeignKeyName", out var foreignKeyName))
                return ServiceResponse.FromProblem<T>(HttpStatusCode.Conflict, $"Invalid relationship. The foreign key '{foreignKeyName}' was violated.");

            return ServiceResponse.FromException<T>(HttpStatusCode.Conflict, e);
        }

    }
}