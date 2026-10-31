using RESTworld.Business.Models;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Authorization.Abstractions;

/// <summary>
/// Handles authorizations for the PATCH operation which can create and update entities through a single JSON Patch (RFC 6902) document.
/// The 'HandlePatchRequestAsync' method is called before the patch is executed on the database.
/// The 'HandlePatchResponseAsync' method is called AFTER the patch results are returned from the database.
/// If multiple handlers are registered for the same types in the ServiceCollection, they will be executed in the direction they have been registered.
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TCreateDto">The type of the DTO which is used on creation.</typeparam>
/// <typeparam name="TUpdateDto">The type of the DTO which is used for updating.</typeparam>
/// <typeparam name="TGetFullDto">The type of the DTO which is used when getting a full record.</typeparam>
public interface IPatchAuthorizationHandler<TEntity, TCreateDto, TUpdateDto, TGetFullDto> : IAuthorizationHandler
    where TUpdateDto : class
{
    /// <summary>
    /// This method is called BEFORE the PATCH request is executed on the database.
    /// Use it if you want to deny the patch, modify the patch document, or add a filter which scopes all entity reads of the patch.
    /// </summary>
    /// <param name="previousResult">The result from the handler which was executed before this one, or an initial unmodified result.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A result which might be modified depending on its internal authorization logic.</returns>
    Task<AuthorizationResult<TEntity, JsonPatchDocument<TCreateDto, TUpdateDto>>> HandlePatchRequestAsync(AuthorizationResult<TEntity, JsonPatchDocument<TCreateDto, TUpdateDto>> previousResult, CancellationToken cancellationToken);

    /// <summary>
    /// This method is called AFTER the PATCH request is executed on the database.
    /// Use it if you want to modify the result AFTER it has been retrieved from the database.
    /// </summary>
    /// <param name="previousResponse">The response from the handler which was executed before this one, or an initial unmodified response.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A response which might be modified depending on its internal authorization logic.</returns>
    Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandlePatchResponseAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse, CancellationToken cancellationToken);
}
