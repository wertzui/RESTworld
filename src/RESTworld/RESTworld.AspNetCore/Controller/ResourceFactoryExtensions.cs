using HAL.AspNetCore.Abstractions;
using Microsoft.AspNetCore.Mvc;
using RESTworld.Business.Models;
using System;
using System.Net;

namespace RESTworld.AspNetCore.Controller
{
    /// <summary>
    /// Contains extension methods for <see cref="IResourceFactory"/>.
    /// </summary>
    public static class ResourceFactoryExtensions
    {
        /// <summary>
        /// Creates an error to return out of the given <see cref="ServiceResponse{T}"/>s status and problem details.
        /// </summary>
        /// <typeparam name="T">The type of the service response.</typeparam>
        /// <param name="resourceFactory">The resource factory that is used to create the result.</param>
        /// <param name="response">The service response holding a status and problem details.</param>
        /// <returns>A result with the problem details and the given status code.</returns>
        public static ObjectResult CreateError<T>(this IResourceFactory resourceFactory, ServiceResponse<T> response)
            => resourceFactory.CreateError((int)response.Status, response.ProblemDetails);

        /// <summary>
        /// Creates an error to return out of the given status and problem details.
        /// </summary>
        /// <param name="resourceFactory">The resource factory that is used to create the result.</param>
        /// <param name="status">A valid HTTP status code.</param>
        /// <param name="problemDetails">Details of the problem to return to the user.</param>
        /// <returns>A result with the problem details and the given status code.</returns>
        public static ObjectResult CreateError(this IResourceFactory resourceFactory, int status, string? problemDetails)
        {
            var resource =
                resourceFactory.CreateForGetEndpoint(new ProblemDetails { Title = Enum.GetName(typeof(HttpStatusCode), status), Status = status, Detail = problemDetails }, null);
            var result = new ObjectResult(resource) { StatusCode = status };
            return result;
        }
    }
}
