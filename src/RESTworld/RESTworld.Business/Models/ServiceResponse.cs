using System;
using System.Net;

namespace RESTworld.Business.Models
{
    /// <summary>
    /// A response from a service call.
    /// Includes the result or the error which occured during the call.
    /// </summary>
    /// <typeparam name="T">The type of the response object.</typeparam>
    public record ServiceResponse<T>
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceResponse{T}"/> class.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="responseObject">The response object.</param>
        internal ServiceResponse(HttpStatusCode status, T responseObject)
        {
            Status = status;
            ResponseObject = responseObject;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ServiceResponse{T}"/> class.
        /// </summary>
        /// <param name="status">The status.</param>
        /// <param name="problemDetails">The problem details.</param>
        internal ServiceResponse(HttpStatusCode status, string problemDetails)
        {
            Status = status;
            ProblemDetails = problemDetails;
        }

        /// <summary>
        /// Gets the response object.
        /// </summary>
        /// <value>
        /// The response object.
        /// </value>
        public T ResponseObject { get; }

        /// <summary>
        /// Gets a value indicating whether the service call succeeded.
        /// </summary>
        /// <value>
        ///   <c>true</c> if succeeded; otherwise, <c>false</c>.
        /// </value>
        public bool Succeeded => (int)Status >= 200 && (int)Status < 300;

        /// <summary>
        /// Gets the problem details if the service call failed.
        /// </summary>
        /// <value>
        /// The problem details.
        /// </value>
        public string ProblemDetails { get; }

        /// <summary>
        /// Gets the status.
        /// This is either in the 2xx range if the call succeeded, or any other code if the call failed.
        /// </summary>
        /// <value>
        /// The status.
        /// </value>
        public HttpStatusCode Status { get; }
    }

    /// <summary>
    /// A static helper class to create <see cref="ServiceResponse{T}"/> instances.
    /// </summary>
    public static class ServiceResponse
    {
        /// <summary>
        /// Creates a response from an exception and a custom status code.
        /// </summary>
        /// <typeparam name="T">The type of the response object.</typeparam>
        /// <param name="status">The status.</param>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public static ServiceResponse<T> FromException<T>(HttpStatusCode status, Exception exception)
            => FromProblem<T>(status, exception.ToString());

        /// <summary>
        /// Creates a response from an exception and the status code 500.
        /// </summary>
        /// <typeparam name="T">The type of the response object.</typeparam>
        /// <param name="exception">The exception.</param>
        /// <returns></returns>
        public static ServiceResponse<T> FromException<T>(Exception exception)
            => FromException<T>(HttpStatusCode.InternalServerError, exception);

        /// <summary>
        /// Creates a response from a problem description and a custom status code.
        /// </summary>
        /// <typeparam name="T">The type of the response object.</typeparam>
        /// <param name="status">The status.</param>
        /// <param name="problemDetails">The problem details.</param>
        /// <returns></returns>
        public static ServiceResponse<T> FromProblem<T>(HttpStatusCode status, string problemDetails)
            => new(status, problemDetails);

        /// <summary>
        /// Creates a response from a problem description and the status code 500.
        /// </summary>
        /// <typeparam name="T">The type of the response object.</typeparam>
        /// <param name="problemDetails">The problem details.</param>
        /// <returns></returns>
        public static ServiceResponse<T> FromProblem<T>(string problemDetails)
            => FromProblem<T>(HttpStatusCode.InternalServerError, problemDetails);

        /// <summary>
        /// Creates a response from result object and the status code 200.
        /// </summary>
        /// <typeparam name="T">The type of the response object.</typeparam>
        /// <param name="responseObject">The response object.</param>
        /// <returns></returns>
        public static ServiceResponse<T> FromResult<T>(T responseObject)
            => new(HttpStatusCode.OK, responseObject);

        /// <summary>
        /// Creates a response from result object and a custom status code.
        /// </summary>
        /// <typeparam name="T">The type of the response object.</typeparam>
        /// <param name="status">The status.</param>
        /// <param name="responseObject">The response object.</param>
        /// <returns></returns>
        public static ServiceResponse<T> FromResult<T>(HttpStatusCode status, T responseObject)
            => new(status, responseObject);

        /// <summary>
        /// Creates a response a custom status code.
        /// </summary>
        /// <typeparam name="T">The type of the response object.</typeparam>
        /// <param name="status">The status.</param>
        /// <returns></returns>
        public static ServiceResponse<T> FromStatus<T>(HttpStatusCode status)
            => FromProblem<T>(status, (int)status >= 200 && (int)status < 300 ? null : status.ToString());
    }
}