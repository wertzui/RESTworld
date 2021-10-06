using System;
using System.Linq;
using System.Net;

namespace RESTworld.Business.Authorization
{
    /// <summary>
    /// A helper class which can create AuthorizationResults with different generic parameters.
    /// </summary>
    public static class AuthorizationResult
    {
        /// <summary>
        /// Creates a forbidden AuthorizationResult.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>A forbidden AuthorizationResult.</returns>
        public static AuthorizationResult<TEntity> Forbidden<TEntity>() => FromStatus<TEntity>(HttpStatusCode.Forbidden);

        /// <summary>
        /// Creates a forbidden AuthorizationResult.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <returns>A forbidden AuthorizationResult.</returns>
        public static AuthorizationResult<TEntity, T1> Forbidden<TEntity, T1>(T1 value1) => FromStatus<TEntity, T1>(HttpStatusCode.Forbidden, value1);

        /// <summary>
        /// Creates a forbidden AuthorizationResult.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <typeparam name="T2">The type of the additional second parameter.</typeparam>
        /// <returns>A forbidden AuthorizationResult.</returns>
        public static AuthorizationResult<TEntity, T1, T2> Forbidden<TEntity, T1, T2>(T1 value1, T2 value2) => FromStatus<TEntity, T1, T2>(HttpStatusCode.Forbidden, value1, value2);

        /// <summary>
        /// Creates an AuthorizationResult from a status.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="status">The HTTP status code.</param>
        /// <returns>An AuthorizationResult from the status.</returns>
        public static AuthorizationResult<TEntity> FromStatus<TEntity>(HttpStatusCode status) => new AuthorizationResult<TEntity>(status);

        /// <summary>
        /// Creates an AuthorizationResult from a status.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="value1">The first parameter of the service method.</param>
        /// <returns>
        /// An AuthorizationResult from the status.
        /// </returns>
        public static AuthorizationResult<TEntity, T1> FromStatus<TEntity, T1>(HttpStatusCode status, T1 value1) => new AuthorizationResult<TEntity, T1>(status, value1);

        /// <summary>
        /// Creates an AuthorizationResult from a status.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <typeparam name="T2">The type of the additional second parameter.</typeparam>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="value1">The first parameter of the service method.</param>
        /// <param name="value2">The second parameter of the service method.</param>
        /// <returns>
        /// An AuthorizationResult from the status.
        /// </returns>
        public static AuthorizationResult<TEntity, T1, T2> FromStatus<TEntity, T1, T2>(HttpStatusCode status, T1 value1, T2 value2) => new AuthorizationResult<TEntity, T1, T2>(status, value1, value2);

        /// <summary>
        /// Creates a successfull AuthorizationResult.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>A successfull AuthorizationResult.</returns>
        public static AuthorizationResult<TEntity> Ok<TEntity>() => FromStatus<TEntity>(HttpStatusCode.OK);

        /// <summary>
        /// Creates a successfull AuthorizationResult.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <returns>A successfull AuthorizationResult.</returns>
        public static AuthorizationResult<TEntity, T1> Ok<TEntity, T1>(T1 value1) => FromStatus<TEntity, T1>(HttpStatusCode.OK, value1);

        /// <summary>
        /// Creates a successfull AuthorizationResult.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <typeparam name="T2">The type of the additional second parameter.</typeparam>
        /// <returns>A successfull AuthorizationResult.</returns>
        public static AuthorizationResult<TEntity, T1, T2> Ok<TEntity, T1, T2>(T1 value1, T2 value2) => FromStatus<TEntity, T1, T2>(HttpStatusCode.OK, value1, value2);

        /// <summary>
        /// Creates an unauthorized AuthorizationResult.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>An unauthorized AuthorizationResult.</returns>
        public static AuthorizationResult<TEntity> Unauthorized<TEntity>() => FromStatus<TEntity>(HttpStatusCode.Unauthorized);

        /// <summary>
        /// Creates an unauthorized AuthorizationResult.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <returns>An unauthorized AuthorizationResult.</returns>
        public static AuthorizationResult<TEntity, T1> Unauthorized<TEntity, T1>(T1 value1) => FromStatus<TEntity, T1>(HttpStatusCode.Unauthorized, value1);

        /// <summary>
        /// Creates an unauthorized AuthorizationResult.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <typeparam name="T2">The type of the additional second parameter.</typeparam>
        /// <returns>An unauthorized AuthorizationResult.</returns>
        public static AuthorizationResult<TEntity, T1, T2> Unauthorized<TEntity, T1, T2>(T1 value1, T2 value2) => FromStatus<TEntity, T1, T2>(HttpStatusCode.Unauthorized, value1, value2);

        /// <summary>
        /// Adds the <see cref="AuthorizationResult{TEntity}.Filter"/> to the given query.
        /// If the filter is null, the query will be returned without modificaiton.
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
            => new AuthorizationResult<TEntity>(previousResult.Status, CombineFilters(previousResult.Filter, filter));

        /// <summary>
        /// Returns a new authorization result with the same status and value, but the new filter appended to the existing filter.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="filter">The filter.</param>
        /// <returns></returns>
        public static AuthorizationResult<TEntity, T1> WithFilter<TEntity, T1>(this AuthorizationResult<TEntity, T1> previousResult, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
            => new AuthorizationResult<TEntity, T1>(previousResult.Status, previousResult.Value1, CombineFilters(previousResult.Filter, filter));

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
            => new AuthorizationResult<TEntity, T1, T2>(previousResult.Status, previousResult.Value1, previousResult.Value2, CombineFilters(previousResult.Filter, filter));

        /// <summary>
        /// Returns a new authorization result with the same filter, but the new status.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static AuthorizationResult<TEntity> WithStatus<TEntity>(this AuthorizationResult<TEntity> previousResult, HttpStatusCode status)
            => new AuthorizationResult<TEntity>(status, previousResult.Filter);

        /// <summary>
        /// Returns a new authorization result with the same filter and value, but the new status.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the 1.</typeparam>
        /// <param name="previousResult">The previous result.</param>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static AuthorizationResult<TEntity, T1> WithStatus<TEntity, T1>(this AuthorizationResult<TEntity, T1> previousResult, HttpStatusCode status)
            => new AuthorizationResult<TEntity, T1>(status, previousResult.Value1, previousResult.Filter);

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
            => new AuthorizationResult<TEntity, T1, T2>(status, previousResult.Value1, previousResult.Value2, previousResult.Filter);

        private static Func<IQueryable<TEntity>, IQueryable<TEntity>> CombineFilters<TEntity>(Func<IQueryable<TEntity>, IQueryable<TEntity>> existingFilter, Func<IQueryable<TEntity>, IQueryable<TEntity>> newFilter)
        {
            if (existingFilter is null)
                return newFilter;

            if (newFilter is null)
                return existingFilter;

            return source => newFilter(existingFilter(source));
        }
    }

    /// <summary>
    /// The result of an authorization.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class AuthorizationResult<TEntity>
    {
        private static readonly Func<IQueryable<TEntity>, IQueryable<TEntity>> _defaultFilter = source => source;

        /// <summary>
        /// Creates a new instance of the <see cref="AuthorizationResult{TEntity}"/> class
        /// </summary>
        /// <param name="status">The status of the authorization.</param>
        /// <param name="filter">An optional filter to alter the request to the database.</param>
        public AuthorizationResult(HttpStatusCode status, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter = null)
        {
            Status = status;

            if (filter is null)
                Filter = _defaultFilter;
            else
                Filter = filter;
        }

        /// <summary>
        /// An optional filter to alter the request to the database.
        /// </summary>
        public Func<IQueryable<TEntity>, IQueryable<TEntity>> Filter { get; }

        /// <summary>
        /// The status of the authorization.
        /// </summary>
        public HttpStatusCode Status { get; }
    }

    /// <summary>
    /// The result of an authorization.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
    public class AuthorizationResult<TEntity, T1> : AuthorizationResult<TEntity>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AuthorizationResult{TEntity, T1}"/> class
        /// </summary>
        /// <param name="status">The status of the authorization.</param>
        /// <param name="value1">The value of the first parameter.</param>
        /// <param name="filter">An optional filter to alter the request to the database.</param>
        public AuthorizationResult(HttpStatusCode status, T1 value1, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter = null) : base(status, filter)
        {
            Value1 = value1;
        }

        /// <summary>
        /// The value of the first parameter.
        /// </summary>
        public T1 Value1 { get; }
    }

    /// <summary>
    /// The result of an authorization.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
    /// <typeparam name="T2">The type of the additional second parameter.</typeparam>
    public class AuthorizationResult<TEntity, T1, T2> : AuthorizationResult<TEntity, T1>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AuthorizationResult{TEntity, T1, T2}"/> class
        /// </summary>
        /// <param name="status">The status of the authorization.</param>
        /// <param name="value1">The value of the first parameter.</param>
        /// <param name="value2">The value of the second parameter.</param>
        /// <param name="filter">An optional filter to alter the request to the database.</param>
        public AuthorizationResult(HttpStatusCode status, T1 value1, T2 value2, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter = null) : base(status, value1, filter)
        {
            Value2 = value2;
        }

        /// <summary>
        /// The value of the second parameter.
        /// </summary>
        public T2 Value2 { get; }
    }
}