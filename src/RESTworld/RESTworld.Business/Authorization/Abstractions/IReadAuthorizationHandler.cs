using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Authorization.Abstractions;

/// <summary>
/// Handles authorizations for the READ part of CRUD.
/// Each 'Handle...RequestAsync' method is called before the corresponding method is executed on the database.
/// Each 'Handle...ResponseAsync' method is called AFTER the corresponding methods results are returned from the database.
/// If multiple handlers are registered for the same types in the ServiceCollection, they will be executed in the direction they have been registered.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TGetListDto">The type of the DTO which is used when getting a list.</typeparam>
/// <typeparam name="TGetFullDto">The type of the DTO which is used when getting a full record.</typeparam>
public interface IReadAuthorizationHandler<TEntity, TGetListDto, TGetFullDto> : IAuthorizationHandler
{
    /// <summary>
    /// This method is called BEFORE the GET-LIST request is executed on the database.
    /// Use it if you want to modify the query BEFORE it is executed.
    /// </summary>
    /// <param name="previousResult">The result from the handler which was executed before this one, or an initial unmodified result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A result which might be modified depending on its internal authorization logic.</returns>
    Task<AuthorizationResult<TEntity, IGetListRequest<TEntity>>> HandleGetListRequestAsync(AuthorizationResult<TEntity, IGetListRequest<TEntity>> previousResult, CancellationToken cancellationToken);

    /// <summary>
    /// This method is called AFTER the GET-LIST request is executed on the database.
    /// Use it if you want to modify the result AFTER it has been retrieved from the database.
    /// </summary>
    /// <param name="previousResponse">The response from the handler which was executed before this one, or an initial unmodified response.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A response which might be modified depending on its internal authorization logic.</returns>
    Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> HandleGetListResponseAsync(ServiceResponse<IReadOnlyPagedCollection<TGetListDto>> previousResponse, CancellationToken cancellationToken);

    /// <summary>
    /// This method is called BEFORE the GET-SINGLE request is executed on the database.
    /// Use it if you want to modify the query BEFORE it is executed.
    /// </summary>
    /// <param name="previousResult">The result from the handler which was executed before this one, or an initial unmodified result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A result which might be modified depending on its internal authorization logic.</returns>
    Task<AuthorizationResult<TEntity, long>> HandleGetSingleRequestAsync(AuthorizationResult<TEntity, long> previousResult, CancellationToken cancellationToken);

    /// <summary>
    /// This method is called AFTER the GET-SINGLE request is executed on the database.
    /// Use it if you want to modify the result AFTER it has been retrieved from the database.
    /// </summary>
    /// <param name="previousResponse">The response from the handler which was executed before this one, or an initial unmodified response.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A response which might be modified depending on its internal authorization logic.</returns>
    Task<ServiceResponse<TGetFullDto>> HandleGetSingleResponseAsync(ServiceResponse<TGetFullDto> previousResponse, CancellationToken cancellationToken);
}