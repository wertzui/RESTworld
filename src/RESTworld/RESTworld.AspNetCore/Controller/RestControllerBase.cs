using HAL.AspNetCore.Controllers;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using RESTworld.Business.Models;
using RESTworld.Common.Dtos;
using System;
using System.Net;
using System.Net.Http;

namespace RESTworld.AspNetCore.Controller
{
    /// <summary>
    /// A base class for custom controllers which do not fit the <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>.
    /// </summary>
    [Route("[controller]")]
    [CrudControllerNameConvention]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesErrorResponseType(typeof(Resource<ProblemDetails>))]
    public abstract class RestControllerBase : HalControllerBase
    {
#pragma warning disable CS1591 // Missing XML comment for publicly visible type or member
        protected readonly IODataResourceFactory _resourceFactory;
#pragma warning restore CS1591 // Missing XML comment for publicly visible type or member

        /// <summary>
        /// Initializes a new instance of the <see cref="RestControllerBase"/> class.
        /// </summary>
        /// <param name="resourceFactory">The resource factory.</param>
        /// <exception cref="System.ArgumentNullException">resourceFactory</exception>
        protected RestControllerBase(IODataResourceFactory resourceFactory)
        {
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
        }

        /// <summary>
        /// Adds a delete link to the given resource.
        /// </summary>
        /// <typeparam name="TDto">The type of the resource.</typeparam>
        /// <param name="result">The resource to add the link to.</param>
        protected void AddDeleteLink<TDto>(Resource<TDto> result)
            where TDto : DtoBase
        {
            if (result.State?.Timestamp is null)
                return;

            result.AddLink(
                "delete",
                new Link
                {
                    Name = HttpMethod.Delete.Method,
                    Href = Url.ActionLink(
                        HttpMethod.Delete.Method,
                        values: new { id = result.State.Id, timestamp = Base64UrlTextEncoder.Encode(result.State.Timestamp) })
                });
        }

        /// <summary>
        /// Adds a save and a delete link to the given resource.
        /// </summary>
        /// <typeparam name="TDto">The type of the resource.</typeparam>
        /// <param name="result">The resource to add the links to.</param>
        protected void AddSaveAndDeleteLinks<TDto>(Resource<TDto> result)
            where TDto : DtoBase
        {
            AddSaveLink(result);
            AddDeleteLink(result);
        }

        /// <summary>
        /// Adds a save link to the given resource.
        /// </summary>
        /// <typeparam name="TDto">The type of the resource.</typeparam>
        /// <param name="result">The resource to add the link to.</param>
        protected void AddSaveLink<TDto>(Resource<TDto> result)
            where TDto : DtoBase
        {
            if (result.State is null)
                return;

            result.AddLink(
                "save",
                new Link
                {
                    Name = HttpMethod.Put.Method,
                    Href = Url.ActionLink(HttpMethod.Put.Method, values: new { id = result.State.Id })
                });
        }

        /// <summary>
        /// Creates an error to return out of the given <see cref="ServiceResponse{T}"/>s status and problem details.
        /// </summary>
        /// <typeparam name="T">The type of the service response.</typeparam>
        /// <param name="response">The service response holding a status and problem details.</param>
        /// <returns>A result with the problem details and the given status code.</returns>
        protected ObjectResult CreateError<T>(ServiceResponse<T> response)
            => CreateError((int)response.Status, response.ProblemDetails);

        /// <summary>
        /// Creates an error to return out of the given status and problem details.
        /// </summary>
        /// <param name="status">A valid HTTP status code.</param>
        /// <param name="problemDetails">Details of the problem to return to the user.</param>
        /// <returns>A result with the problem details and the given status code.</returns>
        protected ObjectResult CreateError(int status, string problemDetails)
        {
            var resource =
                _resourceFactory.Create(new ProblemDetails { Title = Enum.GetName(typeof(HttpStatusCode), status), Status = status, Detail = problemDetails });
            var result = StatusCode(resource.State.Status!.Value, resource);
            return result;
        }
    }
}