using HAL.AspNetCore.ContentNegotiation;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using HAL.Common.Forms;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.Routing;
using Microsoft.AspNetCore.OData.Query;
using RESTworld.AspNetCore.Links.Abstractions;
using RESTworld.AspNetCore.Results.Abstractions;
using RESTworld.AspNetCore.Results.Errors.Abstractions;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using RESTworld.Common.Dtos;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Http;
using System.Text;

namespace RESTworld.AspNetCore.Results;

/// <inheritdoc/>
public class ResultFactory : IResultFactory
{
    private readonly IActionContextAccessor _actionContextAccessor;
    private readonly IErrorResultFactory _errorResultFactory;
    private readonly IODataFormFactory _formFactory;
    private readonly ICrudLinkFactory _linkFactory;
    private readonly IODataResourceFactory _resourceFactory;

    /// <summary>
    /// Creates a new instance of the <see cref="ResultFactory"/> class.
    /// </summary>
    /// <param name="resourceFactory">The resource factory.</param>
    /// <param name="formFactory">The form factory.</param>
    /// <param name="linkFactory">The link factory.</param>
    /// <param name="errorResultFactory">The error result factory.</param>
    /// <param name="actionContextAccessor">The ActionContext accessor.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public ResultFactory(
        IODataResourceFactory resourceFactory,
        IODataFormFactory formFactory,
        ICrudLinkFactory linkFactory,
        IErrorResultFactory errorResultFactory,
        IActionContextAccessor actionContextAccessor)
    {
        _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
        _formFactory = formFactory ?? throw new ArgumentNullException(nameof(formFactory));
        _linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        _errorResultFactory = errorResultFactory ?? throw new ArgumentNullException(nameof(errorResultFactory));
        _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
    }

    /// <inheritdoc/>
    public Resource CreateCollectionResource<TDto>(IReadOnlyCollection<TDto> items, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put")
        where TDto : DtoBase
    {
        var urlHelper = GetUrlHelper();

        var options = new ODataRawQueryOptions();
        var filter = CreateODataFilterForIds(items.Select(i => i.Id));
        options.SetFilter(filter);

        Resource result;

        if (GetHttpContext().GetAcceptHeaders().AcceptsHalFormsOverHal())
        {
            result = _formFactory.CreateForODataListEndpointUsingSkipTopPaging(items, _ => Common.Constants.ListItems, m => m.Id, options, items.Count, items.Count, controller, listGetMethod, singleGetMethod, listPutMethod);
        }
        else
        {
            result = _resourceFactory.CreateForODataListEndpointUsingSkipTopPaging(items, _ => Common.Constants.ListItems, m => m.Id, options, items.Count, items.Count, controller, listGetMethod, singleGetMethod);

            _linkFactory.AddFormLinkForExistingLinkTo(result, Constants.SelfLinkName);
        }

        if (result.Embedded is not null)
        {
            foreach (var embedded in result.Embedded.SelectMany(e => e.Value).Cast<Resource<TDto>>())
            {
                _linkFactory.AddDeleteLink(embedded);
            }
        }
        else
        {
            // Ensure an empty collection so the consumer can be sure it is never null
            result.Embedded = new Dictionary<string, ICollection<Resource>> { { Common.Constants.ListItems, [] } };
        }

        _linkFactory.AddNewLink(result);
        return result;
    }

    /// <inheritdoc/>
    public ObjectResult CreateCreatedCollectionResultBasedOnOutcome<TDto>(ServiceResponse<IReadOnlyCollection<TDto>> serviceResponse, bool readOnly = false, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put", object? routeValues = null)
        where TDto : DtoBase
    {
        if (!serviceResponse.Succeeded)
            return _errorResultFactory.CreateError(serviceResponse, "Post");

        var resource = CreateCollectionResource(serviceResponse.ResponseObject, controller, listGetMethod, singleGetMethod, listPutMethod);

        return new CreatedResult(resource.GetSelfLink().Href, resource);
    }

    /// <inheritdoc/>
    public ObjectResult CreateCreatedResultBasedOnOutcome<TDto>(ServiceResponse<TDto> serviceResponse, bool readOnly = false, string getAction = "Get", string? controller = null, object? routeValues = null)
    {
        if (!serviceResponse.Succeeded)
            return _errorResultFactory.CreateError(serviceResponse, "Post");

        var resource = CreateResource(serviceResponse.ResponseObject, DetermineHttpMethodFromId(serviceResponse.ResponseObject), readOnly, getAction, controller, routeValues);

        return new CreatedAtActionResult(resource.GetSelfLink().Href, controller, UseIdAsRouteValuesIfPresent(serviceResponse.ResponseObject, routeValues), serviceResponse.ResponseObject);
    }

    /// <inheritdoc/>
    public IStatusCodeActionResult CreateEmptyResultBasedOnOutcome(ServiceResponse<object> serviceResponse, string action = "Delete", string? controller = null, object? routeValues = null)
    {
        if (!serviceResponse.Succeeded)
            return _errorResultFactory.CreateError(serviceResponse, action, controller, routeValues);

        return new NoContentResult();
    }

    /// <inheritdoc/>
    public ObjectResult CreateOkResultBasedOnOutcome<TDto>(ServiceResponse<TDto> serviceResponse, bool readOnly = true, string action = "Get", string? controller = null, object? routeValues = null)
    {
        if (!serviceResponse.Succeeded)
            return _errorResultFactory.CreateError(serviceResponse, action);

        var resource = CreateResource(serviceResponse.ResponseObject, DetermineHttpMethodFromId(serviceResponse.ResponseObject), readOnly, action, controller, routeValues);

        return new OkObjectResult(resource);
    }

    /// <inheritdoc/>
    public ObjectResult CreateOkCollectionResultBasedOnOutcome<TDto>(ServiceResponse<IReadOnlyCollection<TDto>> serviceResponse, bool readOnly = false, string action = "Put", string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put", object? routeValues = null)
        where TDto : DtoBase
    {
        if (!serviceResponse.Succeeded)
            return _errorResultFactory.CreateError(serviceResponse, action);

        var resource = CreateCollectionResource(serviceResponse.ResponseObject, controller, listGetMethod, singleGetMethod, listPutMethod);

        return new OkObjectResult(resource);
    }

    /// <inheritdoc/>
    public Resource CreatePagedCollectionResource<TEntity, TDto>(ODataQueryOptions<TEntity> options, IReadOnlyPagedCollection<TDto> page, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put")
        where TDto : DtoBase
        => CreatePagedCollectionResource(options.RawValues, options.Context.DefaultQueryConfigurations.MaxTop, page, controller, listGetMethod, singleGetMethod, listPutMethod);

    /// <inheritdoc/>
    public Resource CreatePagedCollectionResource<TDto>(ODataRawQueryOptions options, int? maxTop, IReadOnlyPagedCollection<TDto> page, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put")
        where TDto : DtoBase
    {
        var urlHelper = GetUrlHelper();
        Resource result;

        if (GetHttpContext().GetAcceptHeaders().AcceptsHalFormsOverHal())
        {
            result = _formFactory.CreateForODataListEndpointUsingSkipTopPaging(page.Items, _ => Common.Constants.ListItems, m => m.Id, options, maxTop ?? 50, page.TotalCount, controller, listGetMethod, singleGetMethod, listPutMethod);
        }
        else
        {
            result = _resourceFactory.CreateForODataListEndpointUsingSkipTopPaging(page.Items, _ => Common.Constants.ListItems, m => m.Id, options, maxTop ?? 50, page.TotalCount, controller, listGetMethod, singleGetMethod);

            _linkFactory.AddFormLinkForExistingLinkTo(result, Constants.SelfLinkName);
        }

        if (result.Embedded is not null)
        {
            foreach (var embedded in result.Embedded.SelectMany(e => e.Value).Cast<Resource<TDto>>())
            {
                _linkFactory.AddDeleteLink(embedded);
            }
        }
        else
        {
            // Ensure an empty collection so the consumer can be sure it is never null
            result.Embedded = new Dictionary<string, ICollection<Resource>> { { Common.Constants.ListItems, [] } };
        }

        _linkFactory.AddNewLink(result);
        return result;
    }

    /// <inheritdoc/>
    public ObjectResult CreatePagedCollectionResultBasedOnOutcome<TEntity, TDto>(ODataQueryOptions<TEntity> options, ServiceResponse<IReadOnlyPagedCollection<TDto>> serviceResponse, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put")
        where TDto : DtoBase
    {
        if (!serviceResponse.Succeeded)
            return _errorResultFactory.CreateError(serviceResponse, listGetMethod);

        var result = CreatePagedCollectionResource(options, serviceResponse.ResponseObject, controller, listGetMethod, singleGetMethod, listPutMethod);

        return new OkObjectResult(result);
    }

    /// <inheritdoc/>
    public ObjectResult CreatePagedCollectionResultBasedOnOutcome<TDto>(ODataRawQueryOptions options, int? maxTop, ServiceResponse<IReadOnlyPagedCollection<TDto>> serviceResponse, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put")
        where TDto : DtoBase
    {
        if (!serviceResponse.Succeeded)
            return _errorResultFactory.CreateError(serviceResponse, listGetMethod, controller);

        var result = CreatePagedCollectionResource(options, maxTop, serviceResponse.ResponseObject, controller, listGetMethod, singleGetMethod, listPutMethod);

        return new OkObjectResult(result);
    }

    /// <inheritdoc/>
    public Resource CreateResource<TDto>(TDto dto, HttpMethod method, bool readOnly = true, string action = "Get", string? controller = null, object? routeValues = null)
    {
        routeValues = UseIdAsRouteValuesIfPresent(dto, routeValues);

        if (GetHttpContext().GetAcceptHeaders().AcceptsHalFormsOverHal())
        {
            var result = _formFactory.CreateResourceForEndpoint(dto, method, "View", action: action, controller: controller, routeValues: routeValues);

            if (readOnly)
                MakeFormReadOnly(result);

            return result;
        }
        else
        {
            var result = _resourceFactory.CreateForEndpoint(dto, action: action, controller: controller, routeValues: routeValues);

            _linkFactory.AddFormLinkForExistingLinkTo(result, Constants.SelfLinkName);

            return result;
        }
    }

    /// <inheritdoc/>
    public void MakeFormReadOnly(FormsResource? form)
    {
        MakeTemplatesReadOnly(form?.Templates);
    }

    /// <inheritdoc/>
    public void MakeTemplateReadOnly(FormTemplate template)
    {
        if (template.Properties is not null)
        {
            foreach (var property in template.Properties)
            {
                property.ReadOnly = true;
                MakeTemplatesReadOnly(property.Templates);
            }
        }
    }

    /// <inheritdoc/>
    public void MakeTemplatesReadOnly(IEnumerable<FormTemplate>? templates)
    {
        if (templates is null)
            return;

        foreach (var template in templates)
        {
            MakeTemplateReadOnly(template);
        }
    }

    /// <inheritdoc/>
    public void MakeTemplatesReadOnly(IDictionary<string, FormTemplate>? templates)
        => MakeTemplatesReadOnly(templates?.Values);

    private static void AppendRange(StringBuilder sb, long inclusiveStart, long inclusiveEnd)
    {
        if (sb.Length != 0)
            sb.Append(" or ");

        sb.Append('(');

        if (inclusiveStart == inclusiveEnd)
            sb.AppendFormat("id eq {0}", inclusiveStart);
        else
            sb.AppendFormat("id ge {0} and id le {1}", inclusiveStart, inclusiveEnd);

        sb.Append(')');
    }

    private static string CreateODataFilterForIds(IEnumerable<long> ids)
    {
        using var enumerator = ids.OrderBy(i => i).GetEnumerator();
        enumerator.MoveNext();
        var currentStart = enumerator.Current;
        var currentEnd = currentStart;

        var sb = new StringBuilder();

        while (enumerator.MoveNext())
        {
            if (enumerator.Current == currentEnd + 1)
            {
                currentEnd++;
            }
            else
            {
                AppendRange(sb, currentStart, currentEnd);

                currentStart = enumerator.Current;
                currentEnd = currentStart;
            }
        }
        AppendRange(sb, currentStart, currentEnd);

        return sb.ToString();
    }

    private static HttpMethod DetermineHttpMethodFromId<TDto>(TDto dto)
    {
        return dto is DtoBase dtoBase && dtoBase.Id != 0 ? HttpMethod.Put : HttpMethod.Post;
    }

    private static object? UseIdAsRouteValuesIfPresent<TDto>(TDto dto, object? routeValues)
    {
        if (routeValues is null && dto is DtoBase dtoBase)
            routeValues = new { id = dtoBase.Id };
        return routeValues;
    }

    private ActionContext GetActionContext() => _actionContextAccessor.ActionContext ?? throw new InvalidOperationException("Unable to get the current HttpContext.");

    private HttpContext GetHttpContext() => GetActionContext().HttpContext;

    private UrlHelper GetUrlHelper() => new(GetActionContext());
}