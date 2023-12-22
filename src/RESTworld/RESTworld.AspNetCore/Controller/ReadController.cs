using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.ContentNegotiation;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using HAL.Common.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Options;
using RESTworld.AspNetCore.Caching;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Errors.Abstractions;
using RESTworld.AspNetCore.Filters;
using RESTworld.Business.Services.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Controller;

/// <summary>
/// A basic READ controller offering operations for Read(List).
/// </summary>
/// <typeparam name="TEntity">The type of the entity.</typeparam>
/// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
/// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
[Route("[controller]")]
[RestControllerNameConvention(RestControllerNameConventionAttribute.ReadControllerIndexOfFullDtoType)]
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
    /// Creates a new instance of the
    /// <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/> class.
    /// </summary>
    /// <param name="service">The service which handles the business operations.</param>
    /// <param name="resourceFactory">
    /// The resource factory which creates HAL resources out of the service responses.
    /// </param>
    /// <param name="linkFactory"></param>
    /// <param name="formFactory">The form factory which created HAL-Form resources.</param>
    /// <param name="errorResultFactory">The factory to create error results.</param>
    /// <param name="cache">The cache for service responses.</param>
    /// <param name="options">
    /// The options which are used to determine the max number of entries for the List endpoint.
    /// </param>
    public ReadController(
        IReadServiceBase<TEntity, TGetListDto, TGetFullDto> service,
        IODataResourceFactory resourceFactory,
        ILinkFactory linkFactory,
        IODataFormFactory formFactory,
        IErrorResultFactory errorResultFactory,
        ICacheHelper cache,
        IOptions<RestWorldOptions> options)
        : base(resourceFactory, cache)
    {
        ArgumentNullException.ThrowIfNull(options);
        Options = options.Value;

        _readService = service ?? throw new ArgumentNullException(nameof(service));
        LinkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        FormFactory = formFactory ?? throw new ArgumentNullException(nameof(formFactory));
        ErrorResultFactory = errorResultFactory ?? throw new ArgumentNullException(nameof(errorResultFactory));
    }

    /// <summary>
    /// The factory to generate error results.
    /// </summary>
    protected IErrorResultFactory ErrorResultFactory { get; }

    /// <summary>
    /// The form factory to generate form responses.
    /// </summary>
    protected IODataFormFactory FormFactory { get; }

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
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The full representation for the requested resource.</returns>
    [HttpGet("{id:long}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesWithContentNegotiation("application/hal+json", "application/prs.hal-forms+json", "application/hal-forms+json")]
    public virtual async Task<ActionResult<Resource<TGetFullDto>>> GetAsync(long id, CancellationToken cancellationToken)
    {
        var response = await Cache.CacheGetWithCurrentUserAsync(id, _ => _readService.GetSingleAsync(id, cancellationToken));

        if (!response.Succeeded || response.ResponseObject is null)
            return ErrorResultFactory.CreateError(response, "Get");

        return Ok(response.ResponseObject, id == 0 ? HttpMethod.Post : HttpMethod.Put);
    }


    /// <summary>
    /// Gets a paged list of resources matching the filter criteria.
    /// </summary>
    /// <param name="options">The OData options used to filter, order an page the list.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A paged list of resources matching the filter criteria.</returns>
    [HttpGet]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    [ProducesResponseType(200)]
    [ProducesWithContentNegotiation("application/hal+json", "text/csv", "application/prs.hal-forms+json", "application/hal-forms+json")]
    public virtual async Task<ActionResult<Resource>> GetListAsync(
        ODataQueryOptions<TEntity> options,
        CancellationToken cancellationToken)
    {
        options.Context.DefaultQueryConfigurations.MaxTop = Options.MaxNumberForListEndpoint;
        var getListrequest = options.ToListRequest(Options.CalculateTotalCountForListEndpoint);

        var response = await Cache.CacheGetListWithCurrentUserAsync(options.RawValues, _ => _readService.GetListAsync(getListrequest, cancellationToken));

        if (!response.Succeeded)
            return ErrorResultFactory.CreateError(response, "GetList");

        var result = CreateListResource(options, response);

        return Ok(result);
    }

    /// <summary>
    /// Creates the result which is either a HAL resource, or a HAL-Forms resource based on the
    /// accept header.
    /// </summary>
    /// <param name="dto">The DTO to return.</param>
    /// <param name="method">The method to use when submitting the form.</param>
    /// <returns>Either a HAL resource, or a HAL-Forms resource</returns>
    protected virtual Resource CreateListResource(TGetFullDto dto, HttpMethod method)
    {
        if (HttpContext.GetAcceptHeaders().AcceptsHalFormsOverHal())
        {
            var result = FormFactory.CreateResourceForEndpoint(dto, method, "View", routeValues: new { id = dto.Id });

            MakeFormReadOnly(result);

            return result;
        }
        else
        {
            var result = ResourceFactory.CreateForEndpoint(dto, routeValues: new { id = dto.Id });

            LinkFactory.AddFormLinkForExistingLinkTo(result, Constants.SelfLinkName);

            return result;
        }
    }

    /// <summary>
    /// Creates the result which is either a HAL resource, or a HAL-Forms resource based on the
    /// accept header.
    /// </summary>
    /// <param name="dto">The DTO to return.</param>
    /// <param name="method">The method to use when submitting the form.</param>
    /// <returns>Either a HAL resource, or a HAL-Forms resource</returns>
    protected virtual Resource CreateResource(TGetFullDto dto, HttpMethod method)
    {
        if (HttpContext.GetAcceptHeaders().AcceptsHalFormsOverHal())
        {
            var result = FormFactory.CreateResourceForEndpoint(dto, method, "View", routeValues: new { id = dto.Id });

            MakeFormReadOnly(result);

            return result;
        }
        else
        {
            var result = ResourceFactory.CreateForEndpoint(dto, routeValues: new { id = dto.Id });

            LinkFactory.AddFormLinkForExistingLinkTo(result, Constants.SelfLinkName);

            return result;
        }
    }

    /// <summary>
    /// Creates an Microsoft.AspNetCore.Mvc.OkObjectResult object that produces an
    /// Microsoft.AspNetCore.Http.StatusCodes.Status200OK response.
    /// </summary>
    /// <param name="dto">The DTO to return.</param>
    /// <param name="method">The method to use when submitting the form.</param>
    /// <returns>The created Microsoft.AspNetCore.Mvc.OkObjectResult for the response.</returns>
    protected virtual OkObjectResult Ok(TGetFullDto dto, HttpMethod method)
        => Ok(CreateResource(dto, method));

    /// <summary>
    /// Since this is a read-only controller, every property is also read only.
    /// <see cref="CreateResource(TGetFullDto, HttpMethod)"/> is overridden in the
    /// <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/>
    /// and there the form is kept as it is.
    /// </summary>
    /// <param name="form">The HAL-Form to make read-only</param>
    private static void MakeFormReadOnly(FormsResource? form)
    {
        MakeFormReadOnly(form?.Templates);
    }

    private static void MakeFormReadOnly(IDictionary<string, FormTemplate>? templates)
    {
        if (templates is null)
            return;

        foreach (var template in templates.Values)
        {
            if (template.Properties is not null)
            {
                foreach (var property in template.Properties)
                {
                    property.ReadOnly = true;
                    MakeFormReadOnly(property.Templates);
                }
            }
        }
    }

    private Resource<Page> CreateListResource(ODataQueryOptions<TEntity> options, Business.Models.ServiceResponse<Business.Models.Abstractions.IReadOnlyPagedCollection<TGetListDto>> response)
    {
        if (!response.Succeeded)
            throw new ArgumentException($"{nameof(CreateListResource)}() may only be called if the response succeeded.", nameof(response));

        Resource<Page> result;

        if (HttpContext.GetAcceptHeaders().AcceptsHalFormsOverHal())
        {
            result = FormFactory.CreateForODataListEndpointUsingSkipTopPaging(response.ResponseObject.Items, _ => Common.Constants.ListItems, m => m.Id, options.RawValues, options.Context.DefaultQueryConfigurations.MaxTop ?? 50, response.ResponseObject.TotalCount);
        }
        else
        {
            result = ResourceFactory.CreateForODataListEndpointUsingSkipTopPaging(response.ResponseObject.Items, _ => Common.Constants.ListItems, m => m.Id, options.RawValues, options.Context.DefaultQueryConfigurations.MaxTop ?? 50, response.ResponseObject.TotalCount);

            LinkFactory.AddFormLinkForExistingLinkTo(result, Constants.SelfLinkName);
        }

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

        Url.AddNewLink(result);
        return result;
    }
}