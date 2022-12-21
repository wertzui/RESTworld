using Microsoft.AspNetCore.OData.Query;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using System;
using System.Linq;

namespace RESTworld.AspNetCore.Controller
{
    /// <summary>
    /// Contains extension methods for <see cref="ODataQueryOptions"/>.
    /// </summary>
    public static class ODataQueryOptionsExtensions
    {
        /// <summary>
        /// Apply the filter query to the given IQueryable.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the query.</typeparam>
        /// <param name="oDataQueryOptions">The query options to apply to the query.</param>
        /// <param name="query">The original System.Linq.IQueryable.</param>
        /// <returns>The new System.Linq.IQueryable after the filter query has been applied to.</returns>
        public static IQueryable<T> ApplyOnlyFilterTo<T>(this ODataQueryOptions<T> oDataQueryOptions, IQueryable<T> query)
            => oDataQueryOptions.Filter is null ? query : oDataQueryOptions.Filter.ApplyTo(query, new ODataQuerySettings()).Cast<T>();

        /// <summary>
        /// Apply the individual query to the given IQueryable in the right order.
        /// </summary>
        /// <typeparam name="T">The type of the elements of the query.</typeparam>
        /// <param name="oDataQueryOptions">The query options to apply to the query.</param>
        /// <param name="query">The original System.Linq.IQueryable.</param>
        /// <returns>The new System.Linq.IQueryable after the query has been applied to.</returns>
        public static IQueryable<T> ApplyTo<T>(this ODataQueryOptions<T> oDataQueryOptions, IQueryable<T> query)
            => oDataQueryOptions.ApplyTo(query).Cast<T>();

        /// <summary>
        /// Converts the <paramref name="oDataQueryOptions"/> into a <see cref="IGetListRequest{TEntity}"/>.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity which is queried in the database.</typeparam>
        /// <param name="oDataQueryOptions">The query options to convert from.</param>
        /// <param name="calculateTotalCount">A value indicating if the total count shall be retrieved from the database.</param>
        /// <returns>A request which can be used to call the service.</returns>
        public static IGetListRequest<TEntity> ToListRequest<TEntity>(this ODataQueryOptions<TEntity> oDataQueryOptions, bool calculateTotalCount)
        {
            Func<IQueryable<TEntity>, IQueryable<TEntity>> filter = oDataQueryOptions.Context.DefaultQuerySettings.MaxTop.HasValue ?
                source => oDataQueryOptions.ApplyTo<TEntity>(source).Take(oDataQueryOptions.Context.DefaultQuerySettings.MaxTop.Value) :
                source => oDataQueryOptions.ApplyTo<TEntity>(source);

            if (calculateTotalCount)
            {
                IQueryable<TEntity> filterForTotalCount(IQueryable<TEntity> source) => oDataQueryOptions.ApplyOnlyFilterTo(source);
                return new GetListRequest<TEntity>(filter, filterForTotalCount);
            }
            else
            {
                return new GetListRequest<TEntity>(filter);
            }
        }
    }
}