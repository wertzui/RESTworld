using System.Net;

namespace RESTworld.Business.Authorization
{
    /// <summary>
    /// A helper class which can create AuthorizationResults without database access with different generic parameters.
    /// </summary>
    public class AuthorizationResultWithoutDb
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AuthorizationResult{TEntity}"/> class
        /// </summary>
        /// <param name="status">The status of the authorization.</param>
        public AuthorizationResultWithoutDb(HttpStatusCode status)
        {
            Status = status;
        }

        /// <summary>
        /// The status of the authorization.
        /// </summary>
        public HttpStatusCode Status { get; }

        /// <summary>
        /// Creates a forbidden AuthorizationResult.
        /// </summary>
        /// <returns>A forbidden AuthorizationResult.</returns>
        public static AuthorizationResultWithoutDb Forbidden() => FromStatus(HttpStatusCode.Forbidden);

        /// <summary>
        /// Creates a forbidden AuthorizationResult.
        /// </summary>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <param name="value1">The first parameter of the service method.</param>
        /// <returns>A forbidden AuthorizationResult.</returns>
        public static AuthorizationResultWithoutDb<T1> Forbidden<T1>(T1 value1) => FromStatus(HttpStatusCode.Forbidden, value1);

        /// <summary>
        /// Creates a forbidden AuthorizationResult.
        /// </summary>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <typeparam name="T2">The type of the additional second parameter.</typeparam>
        /// <param name="value1">The first parameter of the service method.</param>
        /// <param name="value2">The second parameter of the service method.</param>
        /// <returns>A forbidden AuthorizationResult.</returns>
        public static AuthorizationResultWithoutDb<T1, T2> Forbidden<T1, T2>(T1 value1, T2 value2) => FromStatus(HttpStatusCode.Forbidden, value1, value2);

        /// <summary>
        /// Creates an AuthorizationResult from a status.
        /// </summary>
        /// <param name="status">The HTTP status code.</param>
        /// <returns>An AuthorizationResult from the status.</returns>
        public static AuthorizationResultWithoutDb FromStatus(HttpStatusCode status) => new(status);

        /// <summary>
        /// Creates an AuthorizationResult from a status.
        /// </summary>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="value1">The first parameter of the service method.</param>
        /// <returns>An AuthorizationResult from the status.</returns>
        public static AuthorizationResultWithoutDb<T1> FromStatus<T1>(HttpStatusCode status, T1 value1) => new(status, value1);

        /// <summary>
        /// Creates an AuthorizationResult from a status.
        /// </summary>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <typeparam name="T2">The type of the additional second parameter.</typeparam>
        /// <param name="status">The HTTP status code.</param>
        /// <param name="value1">The first parameter of the service method.</param>
        /// <param name="value2">The second parameter of the service method.</param>
        /// <returns>An AuthorizationResult from the status.</returns>
        public static AuthorizationResultWithoutDb<T1, T2> FromStatus<T1, T2>(HttpStatusCode status, T1 value1, T2 value2) => new(status, value1, value2);

        /// <summary>
        /// Creates a successful AuthorizationResult.
        /// </summary>
        /// <returns>A successful AuthorizationResult.</returns>
        public static AuthorizationResultWithoutDb Ok() => FromStatus(HttpStatusCode.OK);

        /// <summary>
        /// Creates a successful AuthorizationResult.
        /// </summary>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <param name="value1">The first parameter of the service method.</param>
        /// <returns>An unauthorized AuthorizationResult.</returns>
        public static AuthorizationResultWithoutDb<T1> Ok<T1>(T1 value1) => FromStatus(HttpStatusCode.OK, value1);

        /// <summary>
        /// Creates a successful AuthorizationResult.
        /// </summary>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <typeparam name="T2">The type of the additional second parameter.</typeparam>
        /// <param name="value1">The first parameter of the service method.</param>
        /// <param name="value2">The second parameter of the service method.</param>
        /// <returns>A successful AuthorizationResult.</returns>
        public static AuthorizationResultWithoutDb<T1, T2> Ok<T1, T2>(T1 value1, T2 value2) => FromStatus(HttpStatusCode.OK, value1, value2);

        /// <summary>
        /// Creates an unauthorized AuthorizationResult.
        /// </summary>
        /// <returns>A successful AuthorizationResult.</returns>
        public static AuthorizationResultWithoutDb Unauthorized() => FromStatus(HttpStatusCode.Unauthorized);

        /// <summary>
        /// Creates an unauthorized AuthorizationResult.
        /// </summary>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <param name="value1">The first parameter of the service method.</param>
        /// <returns>An unauthorized AuthorizationResult.</returns>
        public static AuthorizationResultWithoutDb<T1> Unauthorized<T1>(T1 value1) => FromStatus(HttpStatusCode.Unauthorized, value1);

        /// <summary>
        /// Creates an unauthorized AuthorizationResult.
        /// </summary>
        /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
        /// <typeparam name="T2">The type of the additional second parameter.</typeparam>
        /// <param name="value1">The first parameter of the service method.</param>
        /// <param name="value2">The second parameter of the service method.</param>
        /// <returns>An unauthorized AuthorizationResult.</returns>
        public static AuthorizationResultWithoutDb<T1, T2> Unauthorized<T1, T2>(T1 value1, T2 value2) => FromStatus(HttpStatusCode.Unauthorized, value1, value2);
    }

    /// <summary>
    /// The result of an authorization.
    /// </summary>
    /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
    public class AuthorizationResultWithoutDb<T1> : AuthorizationResultWithoutDb
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AuthorizationResultWithoutDb{T1}"/> class
        /// </summary>
        /// <param name="status">The status of the authorization.</param>
        /// <param name="value1">The value of the first parameter.</param>
        public AuthorizationResultWithoutDb(HttpStatusCode status, T1 value1) : base(status)
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
    /// <typeparam name="T1">The type of the additional first parameter.</typeparam>
    /// <typeparam name="T2">The type of the additional second parameter.</typeparam>
    public class AuthorizationResultWithoutDb<T1, T2> : AuthorizationResultWithoutDb<T1>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="AuthorizationResult{TEntity, T1, T2}"/> class
        /// </summary>
        /// <param name="status">The status of the authorization.</param>
        /// <param name="value1">The value of the first parameter.</param>
        /// <param name="value2">The value of the second parameter.</param>
        public AuthorizationResultWithoutDb(HttpStatusCode status, T1 value1, T2 value2) : base(status, value1)
        {
            Value2 = value2;
        }

        /// <summary>
        /// The value of the second parameter.
        /// </summary>
        public T2 Value2 { get; }
    }
}