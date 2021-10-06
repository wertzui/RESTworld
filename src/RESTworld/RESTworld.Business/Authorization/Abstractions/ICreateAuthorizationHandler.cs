using RESTworld.Business.Models;
using System.Collections.Generic;
using System.Threading.Tasks;

namespace RESTworld.Business.Authorization.Abstractions
{
    /// <summary>
    /// Handles authorizations for the CREATE part of CRUD.
    /// Each 'Handle...RequestAsync' method is called before the corresponding method is executed on the database.
    /// Each 'Handle...ResponseAsync' method is called AFTER the corresponding methods results are returned from the database.
    /// If multiple handlers are registered for the same types in the ServiceCollection, they will be executed in the direction they have been registered.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO which is used on creation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO which is used when getting a full record.</typeparam>
    public interface ICreateAuthorizationHandler<TEntity, TCreateDto, TGetFullDto> : IAuthorizationHandler
    {
        /// <summary>
        /// This method is called before the CREATE request is executed on the database.
        /// Use it if you want to modify the query BEFORE it is executed.
        /// </summary>
        /// <param name="previousResult">The result from the handler which was executed before this one, or an initial unmodified result.</param>
        /// <returns>A result which might be modified depending on its internal authorization logic.</returns>
        Task<AuthorizationResult<TEntity, TCreateDto>> HandleCreateRequestAsync(AuthorizationResult<TEntity, TCreateDto> previousResult);

        /// <summary>
        /// This method is called before the CREATE request is executed on the database.
        /// Use it if you want to modify the query BEFORE it is executed.
        /// </summary>
        /// <param name="previousResult">The result from the handler which was executed before this one, or an initial unmodified result.</param>
        /// <returns>A result which might be modified depending on its internal authorization logic.</returns>
        Task<AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>>> HandleCreateRequestAsync(AuthorizationResult<TEntity, IReadOnlyCollection<TCreateDto>> previousResult);

        /// <summary>
        /// This method is called AFTER the CREATE request is executed on the database.
        /// Use it if you want to modify the result AFTER it has been retrieved from the database.
        /// </summary>
        /// <param name="previousResponse">The response from the handler which was executed before this one, or an initial unmodified response.</param>
        /// <returns>A response which might be modified depending on its internal authorization logic.</returns>
        Task<ServiceResponse<TGetFullDto>> HandleCreateResponseAsync(ServiceResponse<TGetFullDto> previousResponse);

        /// <summary>
        /// This method is called AFTER the CREATE request is executed on the database.
        /// Use it if you want to modify the result AFTER it has been retrieved from the database.
        /// </summary>
        /// <param name="previousResponse">The response from the handler which was executed before this one, or an initial unmodified response.</param>
        /// <returns>A response which might be modified depending on its internal authorization logic.</returns>
        Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleCreateResponseAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse);
    }
}