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
        public static AuthorizationResult<TEntity> FromStatus<TEntity>(HttpStatusCode status) => new(status);

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
        public static AuthorizationResult<TEntity, T1> FromStatus<TEntity, T1>(HttpStatusCode status, T1 value1) => new(status, value1);

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
        public static AuthorizationResult<TEntity, T1, T2> FromStatus<TEntity, T1, T2>(HttpStatusCode status, T1 value1, T2 value2) => new(status, value1, value2);

        /// <summary>
        /// Creates a successful AuthorizationResult.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <returns>A successful AuthorizationResult.</returns>
        public static AuthorizationResult<TEntity> Ok<TEntity>() => FromStatus<TEntity>(HttpStatusCode.OK);

        /// <summary>
        /// Creates a successful AuthorizationResult.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <returns>A successful AuthorizationResult.</returns>
        public static AuthorizationResult<TEntity, T1> Ok<TEntity, T1>(T1 value1) => FromStatus<TEntity, T1>(HttpStatusCode.OK, value1);

        /// <summary>
        /// Creates a successful AuthorizationResult.
        /// </summary>
        /// <typeparam name="TEntity">The type of the entity.</typeparam>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <typeparam name="T2">The type of the additional second parameter.</typeparam>
        /// <returns>A successful AuthorizationResult.</returns>
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
    }

    /// <summary>
    /// The result of an authorization.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    public class AuthorizationResult<TEntity> : AuthorizationResultWithoutDb
    {
        private static readonly Func<IQueryable<TEntity>, IQueryable<TEntity>> _defaultFilter = source => source;

        /// <summary>
        /// Creates a new instance of the <see cref="AuthorizationResult{TEntity}"/> class
        /// </summary>
        /// <param name="status">The status of the authorization.</param>
        /// <param name="filter">An optional filter to alter the request to the database.</param>
        public AuthorizationResult(HttpStatusCode status, Func<IQueryable<TEntity>, IQueryable<TEntity>>? filter = null)
            : base(status)
        {
            if (filter is null)
                Filter = _defaultFilter;
            else
                Filter = filter;
        }

        /// <summary>
        /// An optional filter to alter the request to the database.
        /// </summary>
        public Func<IQueryable<TEntity>, IQueryable<TEntity>> Filter { get; }
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
        public AuthorizationResult(HttpStatusCode status, T1 value1, Func<IQueryable<TEntity>, IQueryable<TEntity>>? filter = null) : base(status, filter)
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
        public AuthorizationResult(HttpStatusCode status, T1 value1, T2 value2, Func<IQueryable<TEntity>, IQueryable<TEntity>>? filter = null) : base(status, value1, filter)
        {
            Value2 = value2;
        }

        /// <summary>
        /// The value of the second parameter.
        /// </summary>
        public T2 Value2 { get; }
    }
}