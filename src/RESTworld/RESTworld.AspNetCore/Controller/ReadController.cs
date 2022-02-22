﻿using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.Forms.Abstractions;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Filters;
using RESTworld.AspNetCore.Swagger;
using RESTworld.Business.Services.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Controller
{
    /// <summary>
    /// A basic READ controller offering operations for Read(List).
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    [Route("[controller]")]
    [RestControllerNameConvention(RestControllerNameConventionAttribute.ReadControllerIndexOfDtoType)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesErrorResponseType(typeof(Resource<ProblemDetails>))]
    public class ReadController<TEntity, TGetListDto, TGetFullDto> : RestControllerBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
    {
        private readonly IReadServiceBase<TEntity, TGetListDto, TGetFullDto> _readService;

        /// <summary>
        /// Creates a new instance of the <see cref="CrudController{TEntity, TCreateDto,
        /// TGetListDto, TGetFullDto, TUpdateDto}"/> class.
        /// </summary>
        /// <param name="service">The service which handles the business operations.</param>
        /// <param name="resourceFactory">
        /// The resource factory which creates HAL resources out of the service responses.
        /// </param>
        /// <param name="linkFactory"></param>
        /// <param name="formFactory">The form factory which created HAL-Form resources.</param>
        /// <param name="options">
        /// The options which are used to determine the max number of entries for the List endpoint.
        /// </param>
        public ReadController(
            IReadServiceBase<TEntity, TGetListDto, TGetFullDto> service,
            IODataResourceFactory resourceFactory,
            ILinkFactory linkFactory,
            IFormFactory formFactory,
            IOptions<RestWorldOptions> options)
            : base(resourceFactory)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            Options = options.Value;

            _readService = service ?? throw new ArgumentNullException(nameof(service));
            LinkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
            FormFactory = formFactory ?? throw new ArgumentNullException(nameof(formFactory));
        }

        /// <summary>
        /// The form factory to generate form responses.
        /// </summary>
        protected IFormFactory FormFactory { get; }

        /// <summary>
        /// The link factory to add links to resources.
        /// </summary>
        protected ILinkFactory LinkFactory { get; }

        /// <summary>
        /// The options which contain default values read from the app settings.
        /// </summary>
        protected RestWorldOptions Options { get; }

        /// <summary>
        /// Gets a full representation of the resource with the given ID.
        /// </summary>
        /// <param name="id">The ID of the resource.</param>
        /// <param name="accept">The Accept header.</param>
        /// <returns>The full representation for the requested resource.</returns>
        [HttpGet("{id:long}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]
        [ProducesWithContentNegotiation("application/hal+json", "application/prs.hal-forms+json", "application/hal-forms+json")]
        public virtual async Task<ActionResult<Resource<TGetFullDto>>> GetAsync(long id, [FromHeader, SwaggerIgnore] string accept)
        {
            var response = await _readService.GetSingleAsync(id);

            if (!response.Succeeded || response.ResponseObject is null)
                return ResourceFactory.CreateError(response);

            return Ok(response.ResponseObject, id == 0 ? HttpMethod.Post : HttpMethod.Put, accept);
        }

        /// <summary>
        /// Gets a paged list of resources matching the filter criteria.
        /// </summary>
        /// <param name="options">The OData options used to filter, order an page the list.</param>
        /// <param name="filter">The filter to filter resources by.</param>
        /// <param name="orderby">
        /// The order of the resources. If none is given, the resources are returned as they appear
        /// in the database.
        /// </param>
        /// <param name="top">The maximum number of resources to return. THis is used for paging.</param>
        /// <param name="skip">The number of resources to skip. This is used for paging.</param>
        /// <returns></returns>
        [HttpGet]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(200)]
        [ProducesWithContentNegotiation("application/hal+json", "text/csv")]
        public virtual async Task<ActionResult<Resource>> GetListAsync(
            [SwaggerIgnore] ODataQueryOptions<TEntity> options,
            [FromQuery(Name = "$filter")] string? filter = default,
            [FromQuery(Name = "$orderby")] string? orderby = default,
            [FromQuery(Name = "$top")] long? top = default,
            [FromQuery(Name = "$skip")] long? skip = default)
        {
            options.Context.DefaultQuerySettings.MaxTop = Options.MaxNumberForListEndpoint;
            var getListrequest = options.ToListRequest(Options.CalculateTotalCountForListEndpoint);

            var response = await _readService.GetListAsync(getListrequest);

            if (!response.Succeeded || response.ResponseObject is null)
                return ResourceFactory.CreateError(response);

            var result = ResourceFactory.CreateForOdataListEndpointUsingSkipTopPaging(response.ResponseObject.Items, _ => Common.Constants.ListItems, m => m.Id, options, options.Context.DefaultQuerySettings.MaxTop.Value, response.ResponseObject.TotalCount);

            if (result.Embedded is not null)
            {
                foreach (var embedded in result.Embedded.SelectMany(e => e.Value).Cast<Resource<TGetListDto>>())
                {
                    Url.AddDeleteLink(embedded);
                }
            }
            else
            {
                // Ensure an empty collection so the consumer can be sure it is never null
                result.Embedded = new Dictionary<string, ICollection<Resource>> { { Common.Constants.ListItems, new List<Resource>() } };
            }

            var href = Url.ActionLink("new");
            if (href is null)
                throw new UriFormatException("Unable top generate the 'new' link.");

            result.AddLink(new Link(href) { Name = "new" });

            return Ok(result);
        }

        /// <summary>
        /// Creates the result which is either a HAL resource, or a HAL-Forms resource based on the
        /// accept header.
        /// </summary>
        /// <param name="dto">The DTO to return.</param>
        /// <param name="method">The method to use when submitting the form.</param>
        /// <param name="accept">The value of the Accept header.</param>
        /// <returns>Either a HAL resource, or a HAL-Forms resource</returns>
        protected virtual Resource CreateResource(TGetFullDto dto, HttpMethod method, string accept)
        {
            if (accept.Contains("hal-forms+json"))
            {
                var result = FormFactory.CreateResourceForEndpoint(dto, method, "Edit", action: method.Method, routeValues: new { id = dto.Id });
                Url.AddDeleteLink(result);

                return result;
            }
            else
            {
                var result = ResourceFactory.CreateForGetEndpoint(dto, routeValues: new { id = dto.Id });

                LinkFactory.AddFormLinkForExistingLinkTo(result, Constants.SelfLinkName);
                Url.AddSaveAndDeleteLinks(result);

                return result;
            }
        }

        /// <summary>
        /// Creates an Microsoft.AspNetCore.Mvc.OkObjectResult object that produces an
        /// Microsoft.AspNetCore.Http.StatusCodes.Status200OK response.
        /// </summary>
        /// <param name="dto">The DTO to return.</param>
        /// <param name="method">The method to use when submitting the form.</param>
        /// <param name="accept">The value of the Accept header.</param>
        /// <returns>The created Microsoft.AspNetCore.Mvc.OkObjectResult for the response.</returns>
        protected virtual OkObjectResult Ok(TGetFullDto dto, HttpMethod method, string accept)
            => Ok(CreateResource(dto, method, accept));
    }
}