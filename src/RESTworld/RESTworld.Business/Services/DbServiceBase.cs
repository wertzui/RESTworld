using AutoMapper;
using AutoMapper.Internal;
using Microsoft.Data.SqlClient;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using RESTworld.Business.Authorization;
using RESTworld.Business.Authorization.Abstractions;
using RESTworld.Business.Models;
using RESTworld.Business.Validation;
using RESTworld.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations.Schema;
using System.Linq;
using System.Net;
using System.Reflection;
using System.Text.RegularExpressions;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Services
{
    /// <summary>
    /// Serves as base class for services which make calls to the database.
    /// It provides logging, mapping, migration testing and most important authorization logic and exception handling.
    /// Whenever you implement your own service method, call <see cref="DbServiceBase{TContext}.TryExecuteWithAuthorizationAsync{TEntity, T1, TResponse, TAuthorizationHandler}(T1, Func{AuthorizationResult{TEntity, T1}, CancellationToken, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResult{TEntity, T1}, TAuthorizationHandler, CancellationToken, Task{AuthorizationResult{TEntity, T1}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, CancellationToken, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler}, CancellationToken)"/> so authorization and error handling is executed.
    /// </summary>
    /// <typeparam name="TContext">The type of the context.</typeparam>
    /// <seealso cref="ServiceBase" />
    public abstract partial class DbServiceBase<TContext> : ServiceBase
        where TContext : DbContextBase
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected static bool _databaseIsMigratedToLatestVersion;
        protected readonly IDbContextFactory<TContext> _contextFactory;
        protected static readonly Regex _foreignKeyRegex = GenerateForeignKeyRegex();
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member



        /// <summary>
        /// Initializes a new instance of the <see cref="DbServiceBase{TContext}"/> class.
        /// </summary>
        /// <param name="contextFactory">The context factory.</param>
        /// <param name="mapper">The mapper.</param>
        /// <param name="userAccessor">The user accessor.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException">contextFactory</exception>
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
        protected virtual async Task<IEnumerable<string>> GetPendingMigrationsAsync(CancellationToken cancellationToken)
        {
            await using var context = _contextFactory.CreateDbContext();

            if (context.Database.IsRelational())
            {
                var pendingMigrations = await context.Database.GetPendingMigrationsAsync(cancellationToken);
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>Either the result of the call or a ServiceResponse describing the problem.</returns>
        protected override async Task<ServiceResponse<T>> TryExecuteAsync<T>(Func<CancellationToken, Task<ServiceResponse<T>>> function, CancellationToken cancellationToken)
        {
            try
            {
                if (!_databaseIsMigratedToLatestVersion)
                {
                    var pendingMigrations = await GetPendingMigrationsAsync(cancellationToken);
                    _databaseIsMigratedToLatestVersion = !pendingMigrations.Any();
                    if (!_databaseIsMigratedToLatestVersion)
                        return ServiceResponse.FromProblem<T>(HttpStatusCode.ServiceUnavailable, $"The following migrations are still pending for {typeof(TContext).Name}:{Environment.NewLine}{string.Join(Environment.NewLine, pendingMigrations)}");
                }

                return await function(cancellationToken);
            }
            catch (DbUpdateConcurrencyException e)
            {
                return GetConcurrencyException<T>(e);
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// The result of the authorization. May contain a failed authorization or a modified parameter.
        /// </returns>
        protected virtual async Task<AuthorizationResult<TEntity, T1>> AuthorizeRequestAsync<TEntity, T1, TAuthorizationHandler>(
            T1 param1,
            Func<AuthorizationResult<TEntity, T1>, TAuthorizationHandler, CancellationToken, Task<AuthorizationResult<TEntity, T1>>> authorizeRequest,
            IEnumerable<TAuthorizationHandler> authorizationHandlers,
            CancellationToken cancellationToken)
            where TAuthorizationHandler : IAuthorizationHandler
        {
            var result = AuthorizationResult.Ok<TEntity, T1>(param1);

            foreach (var handler in authorizationHandlers)
            {
                result = await authorizeRequest(result, handler, cancellationToken);
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// The result of the authorization. May contain a failed authorization or modified parameters.
        /// </returns>
        protected virtual async Task<AuthorizationResult<TEntity, T1, T2>> AuthorizeRequestAsync<TEntity, T1, T2, TAuthorizationHandler>(
            T1 param1,
            T2 param2,
            Func<AuthorizationResult<TEntity, T1, T2>, TAuthorizationHandler, CancellationToken, Task<AuthorizationResult<TEntity, T1, T2>>> authorizeRequest,
            IEnumerable<TAuthorizationHandler> authorizationHandlers,
            CancellationToken cancellationToken)
            where TAuthorizationHandler : IAuthorizationHandler
        {
            var result = AuthorizationResult.Ok<TEntity, T1, T2>(param1, param2);

            foreach (var handler in authorizationHandlers)
            {
                result = await authorizeRequest(result, handler, cancellationToken);
            }

            return result;
        }

        /// <summary>
        /// Tries to execute a <paramref name="function" /> which accepts one parameter while performing <paramref name="authorizeRequest" /> before and <paramref name="authorizeResult" /> after the main invocation.
        /// This method combines <see cref="TryExecuteAsync{T}(Func{CancellationToken, Task{ServiceResponse{T}}}, CancellationToken)" /> with <see cref="WithAuthorizationAsync{TEntity, T1, TResponse, TAuthorizationHandler}(T1, Func{AuthorizationResult{TEntity, T1}, CancellationToken, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResult{TEntity, T1}, TAuthorizationHandler, CancellationToken, Task{AuthorizationResult{TEntity, T1}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, CancellationToken, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler}, CancellationToken)" />.
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        /// <seealso cref="TryExecuteAsync{T}(Func{CancellationToken, Task{ServiceResponse{T}}}, CancellationToken)" />
        /// <seealso cref="WithAuthorizationAsync{TEntity, T1, TResponse, TAuthorizationHandler}(T1, Func{AuthorizationResult{TEntity, T1}, CancellationToken, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResult{TEntity, T1}, TAuthorizationHandler, CancellationToken, Task{AuthorizationResult{TEntity, T1}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, CancellationToken, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler}, CancellationToken)" />
        protected virtual Task<ServiceResponse<TResponse>> TryExecuteWithAuthorizationAsync<TEntity, T1, TResponse, TAuthorizationHandler>(
            T1 param1,
            Func<AuthorizationResult<TEntity, T1>, CancellationToken, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResult<TEntity, T1>, TAuthorizationHandler, CancellationToken, Task<AuthorizationResult<TEntity, T1>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, TAuthorizationHandler, CancellationToken, Task<ServiceResponse<TResponse>>> authorizeResult,
            IEnumerable<TAuthorizationHandler> authorizationHandlers,
            CancellationToken cancellationToken)
            where TAuthorizationHandler : IAuthorizationHandler
            => TryExecuteAsync(token => WithAuthorizationAsync(param1, function, authorizeRequest, authorizeResult, authorizationHandlers, token), cancellationToken);

        /// <summary>
        /// Tries to execute a <paramref name="function" /> which accepts two parameters while performing <paramref name="authorizeRequest" /> before and <paramref name="authorizeResult" /> after the main invocation.
        /// This method combines <see cref="TryExecuteAsync{T}(Func{CancellationToken, Task{ServiceResponse{T}}}, CancellationToken)" /> with <see cref="WithAuthorizationAsync{TEntity, T1, T2, TResponse, TAuthorizationHandler}(T1, T2, Func{AuthorizationResult{TEntity, T1, T2}, CancellationToken, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResult{TEntity, T1, T2}, TAuthorizationHandler, CancellationToken, Task{AuthorizationResult{TEntity, T1, T2}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, CancellationToken, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler}, CancellationToken)" />.
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        /// <seealso cref="TryExecuteAsync{T}(Func{CancellationToken, Task{ServiceResponse{T}}}, CancellationToken)" />
        /// <seealso cref="WithAuthorizationAsync{TEntity, T1, T2, TResponse, TAuthorizationHandler}(T1, T2, Func{AuthorizationResult{TEntity, T1, T2}, CancellationToken, Task{ServiceResponse{TResponse}}}, Func{AuthorizationResult{TEntity, T1, T2}, TAuthorizationHandler, CancellationToken, Task{AuthorizationResult{TEntity, T1, T2}}}, Func{ServiceResponse{TResponse}, TAuthorizationHandler, CancellationToken, Task{ServiceResponse{TResponse}}}, IEnumerable{TAuthorizationHandler}, CancellationToken)" />
        protected virtual Task<ServiceResponse<TResponse>> TryExecuteWithAuthorizationAsync<TEntity, T1, T2, TResponse, TAuthorizationHandler>(
            T1 param1,
            T2 param2,
            Func<AuthorizationResult<TEntity, T1, T2>, CancellationToken, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResult<TEntity, T1, T2>, TAuthorizationHandler, CancellationToken, Task<AuthorizationResult<TEntity, T1, T2>>> authorizeRequest,
            Func<ServiceResponse<TResponse>, TAuthorizationHandler, CancellationToken, Task<ServiceResponse<TResponse>>> authorizeResult,
            IEnumerable<TAuthorizationHandler> authorizationHandlers,
            CancellationToken cancellationToken)
            where TAuthorizationHandler : IAuthorizationHandler
            => TryExecuteAsync(token => WithAuthorizationAsync(param1, param2, function, authorizeRequest, authorizeResult, authorizationHandlers, token), cancellationToken);

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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        protected virtual async Task<ServiceResponse<TResponse>> WithAuthorizationAsync<TEntity, T1, TResponse, TAuthorizationHandler>(
            T1 param1,
            Func<AuthorizationResult<TEntity, T1>, CancellationToken, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResult<TEntity, T1>, TAuthorizationHandler, CancellationToken, Task<AuthorizationResult<TEntity, T1>>> authorizeRequest,
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
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>
        /// The result of the function or a <see cref="ServiceResponse{T}" /> containing a problem description.
        /// </returns>
        protected virtual async Task<ServiceResponse<TResponse>> WithAuthorizationAsync<TEntity, T1, T2, TResponse, TAuthorizationHandler>(
            T1 param1,
            T2 param2,
            Func<AuthorizationResult<TEntity, T1, T2>, CancellationToken, Task<ServiceResponse<TResponse>>> function,
            Func<AuthorizationResult<TEntity, T1, T2>, TAuthorizationHandler, CancellationToken, Task<AuthorizationResult<TEntity, T1, T2>>> authorizeRequest,
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

        private ServiceResponse<T> GetConcurrencyException<T>(DbUpdateConcurrencyException e)
        {
            var validationResults = new ValidationResults("", "Concurrency validation failed. Please reload the resource.");

            var entry = e.Entries.FirstOrDefault();
            if (entry is null)
                return ServiceResponse.FromFailedValidation<T>(HttpStatusCode.Conflict, validationResults);

            var concurrencyPropertyNames = entry.CurrentValues.Properties.Where(p => p.IsConcurrencyToken).Select(p => p.Name).ToHashSet();
            if (concurrencyPropertyNames.Count == 0)
                return ServiceResponse.FromFailedValidation<T>(HttpStatusCode.Conflict, validationResults);

            var entityType = entry.Entity.GetType();
            var returnType = typeof(T);

            var destinationMemberNames = _mapper.ConfigurationProvider.Internal().GetAllTypeMaps()
                .Where(m => m.DestinationType == returnType && m.SourceType == entityType)
                .SelectMany(m => m.PropertyMaps.Where(p => concurrencyPropertyNames.Contains(p.SourceMember.Name)).Select(p => p.DestinationMember.Name))
                .ToList();

            if (destinationMemberNames.Count == 0)
                return ServiceResponse.FromFailedValidation<T>(HttpStatusCode.Conflict, validationResults);

            foreach (var destinationMemberName in destinationMemberNames)
            {
                validationResults.AddValidationFailure(destinationMemberName, "Concurrency validation failed.");
            }

            return ServiceResponse.FromFailedValidation<T>(HttpStatusCode.Conflict, validationResults);
        }

        private ServiceResponse<T> GetForeignKeyExceptionResponse<T>(SqlException e)
        {
            if (e.Message is null)
                return ServiceResponse.FromException<T>(e);

            var match = _foreignKeyRegex.Match(e.Message);
            if (!match.Success)
                return ServiceResponse.FromException<T>(e);

            if (match.Groups.TryGetValue("ForeignKeyName", out var foreignKeyName))
            {
                // If we have a foreign key name we try to walk the whole path backwards from the entity that defines the key, to the dto, to the property on the dto that caused the failure.
                using var context = _contextFactory.CreateDbContext();
                var foreignKeyContraint = context.Model.GetEntityTypes().SelectMany(t => t.GetForeignKeys()).FirstOrDefault(f => f.GetConstraintName() == foreignKeyName.Value);

                if (foreignKeyContraint is not null)
                {
                    var entityType = foreignKeyContraint.PrincipalEntityType.ClrType;
                    var returnType = typeof(T);
                    var returnTypeProperties = returnType.GetProperties();

                    var maps = _mapper.ConfigurationProvider.Internal().GetAllTypeMaps().Where(m => m.DestinationType == entityType);
                    var sourceTypes = maps.Select(m => m.SourceType).ToHashSet();

                    var foreignKeyPropertyOnDto = returnTypeProperties
                        .Where(p => sourceTypes.Contains(p.PropertyType))
                        .Select(p => FindForeignKeyProperty(returnType, returnTypeProperties, p))
                        .Where(a => a is not null)
                        .FirstOrDefault();

                    if (foreignKeyPropertyOnDto is not null)
                    {
                        var validationResults = new ValidationResults(foreignKeyPropertyOnDto, $"Foreign key was violated.");
                        return ServiceResponse.FromFailedValidation<T>(HttpStatusCode.Conflict, validationResults);
                    }
                }

                // If that fails, we simply return a global failure.
                return ServiceResponse.FromProblem<T>(HttpStatusCode.Conflict, $"Invalid relationship. The foreign key '{foreignKeyName}' was violated.");
            }

            if (match.Groups.TryGetValue("PrimaryTable", out var primaryTable))
                return ServiceResponse.FromProblem<T>(HttpStatusCode.Conflict, $"Invalid relationship. '{primaryTable}' was not found.");

            return ServiceResponse.FromException<T>(HttpStatusCode.Conflict, e);
        }

        private static string? FindForeignKeyProperty(Type dtoType, PropertyInfo[] returnTypeProperties, PropertyInfo property)
        {
            // First look for a ForeignKeyAttribute on the property itself
            var foreignKeyAttributeFromProperty = property.PropertyType.GetCustomAttribute<ForeignKeyAttribute>();
            if (foreignKeyAttributeFromProperty is not null)
                return foreignKeyAttributeFromProperty.Name;

            // Then look for a ForeignKeyAttribute pointing to this property
            var propertyWithForeignKeyAttributeToForeignKeyProperty = returnTypeProperties.FirstOrDefault(p => p.GetCustomAttribute<ForeignKeyAttribute>()?.Name == property.Name);
            if (propertyWithForeignKeyAttributeToForeignKeyProperty is not null)
                return propertyWithForeignKeyAttributeToForeignKeyProperty.Name;

            // Lastly check the convention with the Id suffix
            var nameWithId = property.Name + "Id";
            var propertyWithIdSuffix = dtoType.GetProperty(nameWithId, BindingFlags.Public | BindingFlags.Instance | BindingFlags.GetProperty | BindingFlags.IgnoreCase);

            // May be null if it is not found.
            return propertyWithIdSuffix?.Name;
        }

        [GeneratedRegex("FOREIGN KEY constraint \"(?<ForeignKeyName>(FK_(?<ForeignTable>\\w+)_(?<PrimaryTable>\\w+)_(?<ForeignKeyProperty>\\w+))|[^\"]+)\"")]
        private static partial Regex GenerateForeignKeyRegex();
    }
}