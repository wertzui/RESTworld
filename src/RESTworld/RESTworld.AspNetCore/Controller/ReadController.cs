using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.OData.Abstractions;
using HAL.AspNetCore.Utils;
using HAL.Common;
using HAL.Common.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Options;
using RESTworld.AspNetCore.Caching;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Filters;
using RESTworld.AspNetCore.Results.Abstractions;
using RESTworld.AspNetCore.Results.Errors.Abstractions;
using RESTworld.Business.Models.Abstractions;
using RESTworld.Business.Services.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.ComponentModel;
using System.ComponentModel.DataAnnotations;
using System.Diagnostics.CodeAnalysis;
using System.Net;
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
    /// <param name="listRequestFactory">The factory to create list requests out of OData parameters.</param>
    /// <param name="resultFactory">The factory to create results.</param>
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
        IListRequestFactory listRequestFactory,
        IResultFactory resultFactory,
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
        ResultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
        ListRequestFactory = listRequestFactory ?? throw new ArgumentNullException(nameof(listRequestFactory));
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
    /// The factory to create successful results.
    /// </summary>
    public IResultFactory ResultFactory { get; }

    /// <summary>
    /// The factory to create list requests out of OData parameters.
    /// </summary>
    public IListRequestFactory ListRequestFactory { get; }

    /// <summary>
    /// The link factory to add links to resources.
    /// </summary>
    protected ILinkFactory LinkFactory { get; }

    /// <summary>
    /// The options which contain default values read from the app settings.
    /// </summary>
    protected RestWorldOptions Options { get; }

    /// <summary>
    /// Determines whether HalForms responses returned from this controller should be readonly.
    /// The default is <see langword="true"/>.
    /// </summary>
    protected bool ReturnsReadOnlyFormsResponses { get; set; } = true;

    /// <summary>
    /// Gets a full representation of the resource with the given ID.
    /// </summary>
    /// <param name="id">The ID of the resource.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>The full representation for the requested resource.</returns>
    [HttpGet("{id:long}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    [ProducesResponseType(typeof(Resource), 200, "application/hal+json")]
    [ProducesResponseType(typeof(FormsResource), 200, "application/prs.hal-forms+json", "application/hal-forms+json")]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]
    [ProducesWithContentNegotiation("application/hal+json", "application/prs.hal-forms+json", "application/hal-forms+json")]
    [Description("Gets a full representation of the resource with the given ID.")]
    public virtual async Task<ActionResult<Resource<TGetFullDto>>> GetAsync(long id, CancellationToken cancellationToken)
    {
        var response = await Cache.CacheGetWithCurrentUserAsync(id, _ => _readService.GetSingleAsync(id, cancellationToken));

        var result = await ResultFactory.CreateOkResultBasedOnOutcomeAsync(response, ReturnsReadOnlyFormsResponses);

        return result;
    }


    /// <summary>
    /// Gets a paged list of resources matching the filter criteria.
    /// </summary>
    /// <param name="options">The OData options used to filter, order an page the list.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A paged list of resources matching the filter criteria.</returns>
    [HttpGet]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    [ProducesResponseType(typeof(Resource<Page>), 200, "application/hal+json")]
    [ProducesResponseType(typeof(FormsResource<Page>), 200, "application/prs.hal-forms+json", "application/hal-forms+json")]
    [ProducesWithContentNegotiation("application/hal+json", "text/csv", "application/prs.hal-forms+json", "application/hal-forms+json")]
    [Description("Gets a paged list of resources matching the filter criteria.")]
    public virtual async Task<ActionResult<Resource<Page>>> GetListAsync(
        ODataQueryOptions<TGetListDto> options,
        CancellationToken cancellationToken)
    {
        options.Context.DefaultQueryConfigurations.MaxTop = Options.MaxNumberForListEndpoint;

        var getListrequest = ListRequestFactory.CreateListRequest<TGetListDto, TEntity>(options, Options.CalculateTotalCountForListEndpoint);

        var response = await Cache.CacheGetListWithCurrentUserAsync(options.RawValues, _ => _readService.GetListAsync(getListrequest, cancellationToken));

        var result = await ResultFactory.CreatePagedCollectionResultBasedOnOutcomeAsync(options, response);

        return result;
    }

    /// <summary>
    /// Gets a paged list of historical resources matching the filter criteria.
    /// </summary>
    /// <param name="options">The OData options used to filter, order an page the list.</param>
    /// <param name="at">Specifies a specific point in time.</param>
    /// <param name="from">Specifies the start of a time range.</param>
    /// <param name="to">Specifies the exclusive end of a time range.</param>
    /// <param name="toInclusive">Specifies the inclusive end of a time range.</param>
    /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
    /// <returns>A paged list of resources matching the filter criteria.</returns>
    [HttpGet("history")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    [ProducesResponseType(typeof(Resource<Page>), 200, "application/hal+json")]
    [ProducesResponseType(typeof(FormsResource<Page>), 200, "application/prs.hal-forms+json", "application/hal-forms+json")]
    [ProducesWithContentNegotiation("application/hal+json", "text/csv", "application/prs.hal-forms+json", "application/hal-forms+json")]
    [Description("Gets a paged list of historical resources matching the filter criteria.")]
    public virtual async Task<ActionResult<Resource>> GetHistoryAsync(
        ODataQueryOptions<TGetFullDto> options,
        [FromQuery(Name = "$at")] DateTimeOffset? at,
        [FromQuery(Name = "$from")] DateTimeOffset? from,
        [FromQuery(Name = "$to")] DateTimeOffset? to,
        [FromQuery(Name = "$toInclusive")] DateTimeOffset? toInclusive,
        CancellationToken cancellationToken)
    {
        options.Context.DefaultQueryConfigurations.MaxTop = Options.MaxNumberForListEndpoint;

        if (TryParseTemporalQuery(options, at, from, to, toInclusive, out var getHistoryRequest, out var error))
        {
            var response = await Cache.CacheGetListWithCurrentUserAsync(options.RawValues, _ => _readService.GetHistoryAsync(getHistoryRequest, cancellationToken));
            var result = await ResultFactory.CreatePagedCollectionResultBasedOnOutcomeAsync(options, response);
            return result;
        }

        return error;
    }

    private bool TryParseTemporalQuery(
        ODataQueryOptions<TGetFullDto> options,
        DateTimeOffset? at,
        DateTimeOffset? from,
        DateTimeOffset? to,
        DateTimeOffset? toInclusive,
        [NotNullWhen(true)] out IGetHistoryRequest<TGetFullDto, TEntity>? getHistoryRequest,
        [NotNullWhen(false)] out ObjectResult? error)
    {
        getHistoryRequest = null;
        error = null;

        if (typeof(TGetFullDto).GetCustomAttributes(typeof(HasHistoryAttribute), true).Length == 0)
        {
            error = ErrorResultFactory.CreateError((int)HttpStatusCode.NotFound, "This resource does not have a history.", ActionHelper.StripAsyncSuffix(nameof(GetHistoryAsync)));
            return false;
        }

        if (at.HasValue)
        {
            if (from.HasValue || to.HasValue || toInclusive.HasValue)
            {
                error = ErrorResultFactory.CreateError((int)HttpStatusCode.BadRequest, "If $at is specified, $from, $to and $toInclusive must not be specified.", ActionHelper.StripAsyncSuffix(nameof(GetHistoryAsync)));
                return false;
            }

            getHistoryRequest = ListRequestFactory.CreateHistoryRequest<TGetFullDto, TEntity>(options, at, at.Value.AddTicks(1), Options.CalculateTotalCountForListEndpoint);
            return true;
        }

        if (to.HasValue && toInclusive.HasValue)
        {
            error = ErrorResultFactory.CreateError((int)HttpStatusCode.BadRequest, "If $from is specified, only one of $to and $toInclusive may be specified.", ActionHelper.StripAsyncSuffix(nameof(GetHistoryAsync)));
            return false;
        }

        getHistoryRequest = ListRequestFactory.CreateHistoryRequest<TGetFullDto, TEntity>(options, from ?? DateTimeOffset.MinValue, to ?? toInclusive?.AddTicks(1) ?? DateTimeOffset.MaxValue, Options.CalculateTotalCountForListEndpoint);
        return true;
    }
}