using RESTworld.Business.Models;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Authorization.Abstractions;

/// <summary>
/// Handles basic authorization tasks without on entity or DTO.
/// Use it if you have your own service implementing a logic which does not fit into CRUD.
/// The 'HandleRequestAsync' method is called before the corresponding method is executed on the database.
/// The 'HandleResponseAsync' method is called AFTER the corresponding methods results are returned from the database.
/// If multiple handlers are registered for the same types in the ServiceCollection, they will be executed in the order they have been registered.
/// </summary>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <seealso cref="IAuthorizationHandler" />
public interface IBasicAuthorizationHandler<TResponse> : IAuthorizationHandler
{
    /// <summary>
    /// This method is called before the request is executed on the database.
    /// Use it if you want to modify the query BEFORE it is executed.
    /// </summary>
    /// <param name="previousResult">The result from the handler which was executed before this one, or an initial unmodified result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A result which might be modified depending on its internal authorization logic.</returns>
    Task<AuthorizationResultWithoutDb> HandleRequestAsync(AuthorizationResultWithoutDb previousResult, CancellationToken cancellationToken);

    /// <summary>
    /// This method is called AFTER the request has been executed on the database.
    /// Use it if you want to modify the result AFTER it has been retrieved from the database.
    /// </summary>
    /// <param name="previousResponse">The response from the handler which was executed before this one, or an initial unmodified response.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A response which might be modified depending on its internal authorization logic.</returns>
    Task<ServiceResponse<TResponse>> HandleResponseAsync(ServiceResponse<TResponse> previousResponse, CancellationToken cancellationToken);
}

/// <summary>
/// Handles basic authorization tasks with one DTO.
/// Use it if you have your own service implementing a logic which does not fit into CRUD.
/// The 'HandleRequestAsync' method is called before the corresponding method is executed on the database.
/// The 'HandleResponseAsync' method is called AFTER the corresponding methods results are returned from the database.
/// If multiple handlers are registered for the same types in the ServiceCollection, they will be executed in the order they have been registered.
/// </summary>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <seealso cref="IAuthorizationHandler" />
public interface IBasicAuthorizationHandler<TRequest, TResponse> : IAuthorizationHandler
{
    /// <summary>
    /// This method is called before the request is executed on the database.
    /// Use it if you want to modify the query BEFORE it is executed.
    /// </summary>
    /// <param name="previousResult">The result from the handler which was executed before this one, or an initial unmodified result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A result which might be modified depending on its internal authorization logic.</returns>
    Task<AuthorizationResultWithoutDb<TRequest>> HandleRequestAsync(AuthorizationResultWithoutDb<TRequest> previousResult, CancellationToken cancellationToken);

    /// <summary>
    /// This method is called AFTER the request has been executed on the database.
    /// Use it if you want to modify the result AFTER it has been retrieved from the database.
    /// </summary>
    /// <param name="previousResponse">The response from the handler which was executed before this one, or an initial unmodified response.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A response which might be modified depending on its internal authorization logic.</returns>
    Task<ServiceResponse<TResponse>> HandleResponseAsync(ServiceResponse<TResponse> previousResponse, CancellationToken cancellationToken);
}

/// <summary>
/// Handles basic authorization tasks with one entity and one DTO.
/// Use it if you have your own service implementing a logic which does not fit into CRUD.
/// The 'HandleRequestAsync' method is called before the corresponding method is executed on the database.
/// The 'HandleResponseAsync' method is called AFTER the corresponding methods results are returned from the database.
/// If multiple handlers are registered for the same types in the ServiceCollection, they will be executed in the order they have been registered.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TRequest">The type of the request.</typeparam>
/// <typeparam name="TResponse">The type of the response.</typeparam>
/// <seealso cref="IAuthorizationHandler" />
public interface IBasicAuthorizationHandler<TEntity, TRequest, TResponse> : IAuthorizationHandler
{
    /// <summary>
    /// This method is called before the request is executed on the database.
    /// Use it if you want to modify the query BEFORE it is executed.
    /// </summary>
    /// <param name="previousResult">The result from the handler which was executed before this one, or an initial unmodified result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A result which might be modified depending on its internal authorization logic.</returns>
    Task<AuthorizationResult<TEntity, TRequest>> HandleRequestAsync(AuthorizationResult<TEntity, TRequest> previousResult, CancellationToken cancellationToken);

    /// <summary>
    /// This method is called AFTER the request has been executed on the database.
    /// Use it if you want to modify the result AFTER it has been retrieved from the database.
    /// </summary>
    /// <param name="previousResponse">The response from the handler which was executed before this one, or an initial unmodified response.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A response which might be modified depending on its internal authorization logic.</returns>
    Task<ServiceResponse<TResponse>> HandleResponseAsync(ServiceResponse<TResponse> previousResponse, CancellationToken cancellationToken);

}