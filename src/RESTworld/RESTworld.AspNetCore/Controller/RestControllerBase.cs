using HAL.AspNetCore.Controllers;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using System;

namespace RESTworld.AspNetCore.Controller
{
    /// <summary>
    /// A base class for custom controllers which do not fit the <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>.
    /// </summary>
    [Route("[controller]")]
    [RestControllerNameConvention(0)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesErrorResponseType(typeof(Resource<ProblemDetails>))]
    public abstract class RestControllerBase : HalControllerBase
    {
        /// <summary>
        /// The resource factory that is used to create responses.
        /// </summary>
        protected IODataResourceFactory ResourceFactory { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RestControllerBase"/> class.
        /// </summary>
        /// <param name="resourceFactory">The resource factory.</param>
        /// <exception cref="System.ArgumentNullException">resourceFactory</exception>
        protected RestControllerBase(IODataResourceFactory resourceFactory)
        {
            ResourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
        }
    }
}