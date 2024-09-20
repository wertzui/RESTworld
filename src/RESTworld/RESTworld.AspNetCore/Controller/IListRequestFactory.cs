using AutoMapper.AspNet.OData;
using Microsoft.AspNetCore.OData.Query;
using RESTworld.Business.Models.Abstractions;
using System;
using System.Linq;

namespace RESTworld.AspNetCore.Controller
{
    /// <summary>
    /// Creates <see cref="IGetListRequest{TDto, TEntity}"/> and <see cref="IGetHistoryRequest{TDto, TEntity}"/>. This is used to convert the
    /// ODataQueryOptions into a request which can be used to call the service.
    /// </summary>
    public interface IListRequestFactory
    {
        /// <summary>
        /// Converts the <paramref name="oDataQueryOptions"/> into a <see cref="IGetHistoryRequest{TDto, TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity which is queried in the database.</typeparam>
        /// <typeparam name="TDto">The type of the DTO which is returned.</typeparam>
        /// <param name="oDataQueryOptions">The query options to convert from.</param>
        /// <param name="validFrom">The valid from date.</param>
        /// <param name="validTo">The valid to date.</param>
        /// <param name="calculateTotalCount">
        /// A value indicating if the total count shall be retrieved from the database.
        /// </param>
        /// <returns>A request which can be used to call the service.</returns>
        IGetHistoryRequest<TDto, TEntity> CreateHistoryRequest<TDto, TEntity>(ODataQueryOptions<TDto> oDataQueryOptions, DateTimeOffset? validFrom, DateTimeOffset? validTo, bool calculateTotalCount)
            where TDto : class
            where TEntity : class;

        /// <summary>
        /// Converts the <paramref name="oDataQueryOptions"/> into a <see cref="IGetListRequest{TDto, TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity which is queried in the database.</typeparam>
        /// <typeparam name="TDto">The type of the DTO which is returned.</typeparam>
        /// <param name="oDataQueryOptions">The query options to convert from.</param>
        /// <param name="calculateTotalCount">
        /// A value indicating if the total count shall be retrieved from the database.
        /// </param>
        /// <returns>A request which can be used to call the service.</returns>
        IGetListRequest<TDto, TEntity> CreateListRequest<TDto, TEntity>(ODataQueryOptions<TDto> oDataQueryOptions, bool calculateTotalCount)
            where TDto : class
            where TEntity : class;

        /// <summary>
        /// Gets a query which only contains the filter and not $skip and $top.
        /// </summary>
        /// <typeparam name="TDto"></typeparam>
        /// <typeparam name="TEntity"></typeparam>
        /// <param name="oDataQueryOptions">The query options to convert from.</param>
        /// <param name="querySettings">The query settings.</param>
        /// <returns>
        /// A function which applies the $filter to the entity collection and returns a Dto collection.
        /// </returns>
        Func<IQueryable<TEntity>, IQueryable<TDto>> GetFilterOnlyQuery<TDto, TEntity>(ODataQueryOptions<TDto> oDataQueryOptions, QuerySettings? querySettings = null) where TDto : class;
    }
}