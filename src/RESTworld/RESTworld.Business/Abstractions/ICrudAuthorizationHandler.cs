using System.Collections.Generic;
using System.Threading.Tasks;

namespace RESTworld.Business.Abstractions
{
    /// <summary>
    /// Handles authorizations for <see cref="ICrudServiceBase{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>s.
    /// Each 'Handle...RequestAsync' method is called before the corresponding method is executed on the database.
    /// Each 'Handle...ResponseAsync' method is called AFTER the corresponding methods results are returned from the database.
    /// If multiple handlers are registered for the same types in the ServiceCollection, they will be executed in the direction they have been registered.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO which is used on creation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO which is used when getting a list.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO which is used when getting a full record.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO which is used for updating.</typeparam>
    public interface ICrudAuthorizationHandler<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto>
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

        /// <summary>
        /// This method is called BEFORE the DELETE request is executed on the database.
        /// Use it if you want to modify the query BEFORE it is executed.
        /// </summary>
        /// <param name="previousResult">The result from the handler which was executed before this one, or an initial unmodified result.</param>
        /// <returns>A result which might be modified depending on its internal authorization logic.</returns>
        Task<AuthorizationResult<TEntity, long, byte[]>> HandleDeleteRequestAsync(AuthorizationResult<TEntity, long, byte[]> previousResult);

        /// <summary>
        /// This method is called AFTER the DELETE request is executed on the database.
        /// Use it if you want to modify the result AFTER it has been retrieved from the database.
        /// </summary>
        /// <param name="previousResponse">The response from the handler which was executed before this one, or an initial unmodified response.</param>
        /// <returns>A response which might be modified depending on its internal authorization logic.</returns>
        Task<ServiceResponse<object>> HandleDeleteResponseAsync(ServiceResponse<object> previousResponse);

        /// <summary>
        /// This method is called BEFORE the GET-LIST request is executed on the database.
        /// Use it if you want to modify the query BEFORE it is executed.
        /// </summary>
        /// <param name="previousResult">The result from the handler which was executed before this one, or an initial unmodified result.</param>
        /// <returns>A result which might be modified depending on its internal authorization logic.</returns>
        Task<AuthorizationResult<TEntity, IGetListRequest<TEntity>>> HandleGetListRequestAsync(AuthorizationResult<TEntity, IGetListRequest<TEntity>> previousResult);

        /// <summary>
        /// This method is called AFTER the GET-LIST request is executed on the database.
        /// Use it if you want to modify the result AFTER it has been retrieved from the database.
        /// </summary>
        /// <param name="previousResponse">The response from the handler which was executed before this one, or an initial unmodified response.</param>
        /// <returns>A response which might be modified depending on its internal authorization logic.</returns>
        Task<ServiceResponse<IReadOnlyPagedCollection<TGetListDto>>> HandleGetListResponseAsync(ServiceResponse<IReadOnlyPagedCollection<TGetListDto>> previousResponse);

        /// <summary>
        /// This method is called BEFORE the GET-SINGLE request is executed on the database.
        /// Use it if you want to modify the query BEFORE it is executed.
        /// </summary>
        /// <param name="previousResult">The result from the handler which was executed before this one, or an initial unmodified result.</param>
        /// <returns>A result which might be modified depending on its internal authorization logic.</returns>
        Task<AuthorizationResult<TEntity, long>> HandleGetSingleRequestAsync(AuthorizationResult<TEntity, long> previousResult);

        /// <summary>
        /// This method is called AFTER the GET-SINGLE request is executed on the database.
        /// Use it if you want to modify the result AFTER it has been retrieved from the database.
        /// </summary>
        /// <param name="previousResponse">The response from the handler which was executed before this one, or an initial unmodified response.</param>
        /// <returns>A response which might be modified depending on its internal authorization logic.</returns>
        Task<ServiceResponse<TGetFullDto>> HandleGetSingleResponseAsync(ServiceResponse<TGetFullDto> previousResponse);

        /// <summary>
        /// This method is called BEFORE the UPDATE request is executed on the database.
        /// Use it if you want to modify the query BEFORE it is executed.
        /// </summary>
        /// <param name="previousResult">The result from the handler which was executed before this one, or an initial unmodified result.</param>
        /// <returns>A result which might be modified depending on its internal authorization logic.</returns>
        Task<AuthorizationResult<TEntity, TUpdateDto>> HandleUpdateRequestAsync(AuthorizationResult<TEntity, TUpdateDto> previousResult);

        /// <summary>
        /// This method is called BEFORE the UPDATE request is executed on the database.
        /// Use it if you want to modify the query BEFORE it is executed.
        /// </summary>
        /// <param name="previousResult">The result from the handler which was executed before this one, or an initial unmodified result.</param>
        /// <returns>A result which might be modified depending on its internal authorization logic.</returns>
        Task<AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>>> HandleUpdateRequestAsync(AuthorizationResult<TEntity, IUpdateMultipleRequest<TUpdateDto, TEntity>> previousResult);

        /// <summary>
        /// This method is called AFTER the UPDATE request is executed on the database.
        /// Use it if you want to modify the result AFTER it has been retrieved from the database.
        /// </summary>
        /// <param name="previousResponse">The response from the handler which was executed before this one, or an initial unmodified response.</param>
        /// <returns>A response which might be modified depending on its internal authorization logic.</returns>
        Task<ServiceResponse<TGetFullDto>> HandleUpdateResponseAsync(ServiceResponse<TGetFullDto> previousResponse);

        /// <summary>
        /// This method is called AFTER the UPDATE request is executed on the database.
        /// Use it if you want to modify the result AFTER it has been retrieved from the database.
        /// </summary>
        /// <param name="previousResponse">The response from the handler which was executed before this one, or an initial unmodified response.</param>
        /// <returns>A response which might be modified depending on its internal authorization logic.</returns>
        Task<ServiceResponse<IReadOnlyCollection<TGetFullDto>>> HandleUpdateResponseAsync(ServiceResponse<IReadOnlyCollection<TGetFullDto>> previousResponse);
    }
}