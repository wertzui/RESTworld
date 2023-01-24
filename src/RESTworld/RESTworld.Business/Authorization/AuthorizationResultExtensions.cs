using System;
using System.Linq;
using System.Net;

namespace RESTworld.Business.Authorization
{
    /// <summary>
    /// Contains extension methods for <see cref="AuthorizationResultWithoutDb"/> and the different generic overloads.
    /// </summary>
    public static class AuthorizationResultExtensions
    {
        /// <summary>
        /// Adds the <see cref="AuthorizationResult{TEntity}.Filter"/> to the given query.
        /// If the filter is null, the query will be returned without modification.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="source">The source query.</param>
        /// <param name="authorizationResult">The authorization result.</param>
        /// <returns></returns>
        public static IQueryable<TEntity> WithAuthorizationFilter<TEntity>(this IQueryable<TEntity> source, AuthorizationResult<TEntity> authorizationResult)
        {
            var filter = authorizationResult.Filter;
            if (filter is not null)
                return filter(source);

            return source;
        }

        /// <summary>
        /// Returns a new authorization result with the same status, but the new filter appended to the existing filter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static AuthorizationResult<TEntity> WithFilter<TEntity>(this AuthorizationResult<TEntity> previousResult, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
            => new(previousResult.Status, CombineFilters(previousResult.Filter, filter));

        /// <summary>
        /// Returns a new authorization result with the same status and value, but the new filter appended to the existing filter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static AuthorizationResult<TEntity, T1> WithFilter<TEntity, T1>(this AuthorizationResult<TEntity, T1> previousResult, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
            => new(previousResult.Status, previousResult.Value1, CombineFilters(previousResult.Filter, filter));

        /// <summary>
        /// Returns a new authorization result with the same status and values, but the new filter appended to the existing filter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static AuthorizationResult<TEntity, T1, T2> WithFilter<TEntity, T1, T2>(this AuthorizationResult<TEntity, T1, T2> previousResult, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
            => new(previousResult.Status, previousResult.Value1, previousResult.Value2, CombineFilters(previousResult.Filter, filter));

        /// <summary>
        /// Returns a new authorization result with the same filter, but the new status.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static AuthorizationResult<TEntity> WithStatus<TEntity>(this AuthorizationResult<TEntity> previousResult, HttpStatusCode status)
            => new(status, previousResult.Filter);

        /// <summary>
        /// Returns a new authorization result with the same filter and value, but the new status.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static AuthorizationResult<TEntity, T1> WithStatus<TEntity, T1>(this AuthorizationResult<TEntity, T1> previousResult, HttpStatusCode status)
            => new(status, previousResult.Value1, previousResult.Filter);

        /// <summary>
        /// Returns a new authorization result with the same filter and values, but the new status.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <typeparam name="T2">The type of the 2.</typeparam>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static AuthorizationResult<TEntity, T1, T2> WithStatus<TEntity, T1, T2>(this AuthorizationResult<TEntity, T1, T2> previousResult, HttpStatusCode status)
            => new(status, previousResult.Value1, previousResult.Value2, previousResult.Filter);

        private static Func<IQueryable<TEntity>, IQueryable<TEntity>> CombineFilters<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> existingFilter, Func<IQueryable<TEntity>, IQueryable<TEntity>> newFilter)
        {
            if (existingFilter is null)
                return newFilter;

            if (newFilter is null)
                return existingFilter;

            return source => newFilter(existingFilter(source));
        }
    }
}