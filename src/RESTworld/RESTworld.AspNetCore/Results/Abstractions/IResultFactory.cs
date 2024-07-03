using HAL.Common;
using HAL.Common.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.OData.Query;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using RESTworld.Common.Dtos;
using System.Collections.Generic;
using System.Net.Http;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Results.Abstractions;

/// <summary>
/// A factory to create results which can be returned from controller actions.
/// </summary>
public interface IResultFactory
{
    /// <summary>
    /// Creates a non paged list result which is either a HAL resource, or a HAL-Forms resource
    /// based on the accept header.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTOs in the list.</typeparam>
    /// <param name="items">The items of the collection.</param>
    /// <param name="controller">The controller.</param>
    /// <param name="listGetMethod">
    /// The name of the get method for the list endpoint. Default is "GetList".
    /// </param>
    /// <param name="singleGetMethod">
    /// The name of the get method for the get-single endpoint. Default is "Get".
    /// </param>
    /// <param name="listPutMethod">
    /// The name of the put method for the update-multiple endpoint. Default is "Put".
    /// </param>
    /// <returns>Either a HAL resource, or a HAL-Forms resource containing the given <paramref name="items"/>.</returns>
    ValueTask<Resource> CreateCollectionResourceAsync<TDto>(IReadOnlyCollection<TDto> items, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put") where TDto : DtoBase;

    /// <summary>
    /// Creates a collection result based on the outcome of a service response. It either contains the
    /// successful result, or an error result. If the result is successful, it will be a
    /// <see cref="CreatedAtActionResult"/> containing a HAL resource, or a HAL-Forms resource based
    /// on the accept header.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <param name="serviceResponse">The response from the service call.</param>
    /// <param name="readOnly">
    /// When this method returns HAL-Forms, this parameter determines if the form is read only or not.
    /// </param>
    /// <param name="controller">The controller.</param>
    /// <param name="listGetMethod">
    /// The name of the get method for the list endpoint. Default is "GetList".
    /// </param>
    /// <param name="singleGetMethod">
    /// The name of the get method for the get-single endpoint. Default is "Get".
    /// </param>
    /// <param name="listPutMethod">
    /// The name of the put method for the update-multiple endpoint. Default is "Put".
    /// </param>
    /// <param name="routeValues">
    /// The route values. If none are given, the <see cref="DtoBase.Id"/> of the given
    /// <see cref="ServiceResponse{T}.ResponseObject"/> is used.
    /// </param>
    /// <returns>Either a successful or an error result.</returns>
    ValueTask<ObjectResult> CreateCreatedCollectionResultBasedOnOutcomeAsync<TDto>(ServiceResponse<IReadOnlyCollection<TDto>> serviceResponse, bool readOnly = false, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put", object? routeValues = null) where TDto : DtoBase;

    /// <summary>
    /// Creates a result based on the outcome of a service response. It either contains the
    /// successful result, or an error result. If the result is successful, it will be a
    /// <see cref="CreatedAtActionResult"/> containing a HAL resource, or a HAL-Forms resource based
    /// on the accept header.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <param name="serviceResponse">The response from the service call.</param>
    /// <param name="readOnly">
    /// When this method returns HAL-Forms, this parameter determines if the form is read only or not.
    /// </param>
    /// <param name="action">
    /// The name of the action that is used to generate the self link. This is normally the name of
    /// the controller method without the async suffix. If none is provided, "Get" is used as default.
    /// </param>
    /// <param name="controller">The controller.</param>
    /// <param name="routeValues">
    /// The route values. If none are given, the <see cref="DtoBase.Id"/> of the given
    /// <see cref="ServiceResponse{T}.ResponseObject"/> is used.
    /// </param>
    /// <returns>Either a successful or an error result.</returns>
    ValueTask<ObjectResult> CreateCreatedResultBasedOnOutcomeAsync<TDto>(ServiceResponse<TDto> serviceResponse, bool readOnly = false, string action = "Get", string? controller = null, object? routeValues = null);

    /// <summary>
    /// Creates an empty result based on the outcome of a service response. It is either empty, or
    /// contains an error result.
    /// </summary>
    /// <param name="serviceResponse">The response from the service call.</param>
    /// <param name="action">
    /// The name of the action that is used to generate the self link. This is normally the name of
    /// the controller method without the async suffix. If none is provided, "Get" is used as default.
    /// </param>
    /// <param name="controller">The controller.</param>
    /// <param name="routeValues">The route values.</param>
    /// <returns>Either a successful or an error result.</returns>
    IStatusCodeActionResult CreateEmptyResultBasedOnOutcome(ServiceResponse<object> serviceResponse, string action = "Delete", string? controller = null, object? routeValues = null);

    /// <summary>
    /// Creates a collection result based on the outcome of a service response. It either contains the
    /// successful result, or an error result. If the result is successful, it will be a
    /// <see cref="OkObjectResult"/> containing a HAL resource, or a HAL-Forms resource based on the
    /// accept header.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <param name="serviceResponse">The response from the service call.</param>
    /// <param name="readOnly">
    /// When this method returns HAL-Forms, this parameter determines if the form is read only or not.
    /// </param>
    /// <param name="action">
    /// The name of the action that is used to generate the self link. This is normally the name of
    /// the controller method without the async suffix. If none is provided, "Get" is used as default.
    /// </param>
    /// <param name="controller">The controller.</param>
    /// <param name="listGetMethod">
    /// The name of the get method for the list endpoint. Default is "GetList".
    /// </param>
    /// <param name="singleGetMethod">
    /// The name of the get method for the get-single endpoint. Default is "Get".
    /// </param>
    /// <param name="listPutMethod">
    /// The name of the put method for the update-multiple endpoint. Default is "Put".
    /// </param>
    /// <param name="routeValues">
    /// The route values. If none are given, the <see cref="DtoBase.Id"/> of the given
    /// <see cref="ServiceResponse{T}.ResponseObject"/> is used.
    /// </param>
    /// <returns>Either a successful or an error result.</returns>
    ValueTask<ObjectResult> CreateOkCollectionResultBasedOnOutcomeAsync<TDto>(ServiceResponse<IReadOnlyCollection<TDto>> serviceResponse, bool readOnly = false, string action = "Put", string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put", object? routeValues = null) where TDto : DtoBase;

    /// <summary>
    /// Creates a result based on the outcome of a service response. It either contains the
    /// successful result, or an error result. If the result is successful, it will be a
    /// <see cref="OkObjectResult"/> containing a HAL resource, or a HAL-Forms resource based on the
    /// accept header.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <param name="serviceResponse">The response from the service call.</param>
    /// <param name="readOnly">
    /// When this method returns HAL-Forms, this parameter determines if the form is read only or not.
    /// </param>
    /// <param name="action">
    /// The name of the action that is used to generate the self link. This is normally the name of
    /// the controller method without the async suffix. If none is provided, "Get" is used as default.
    /// </param>
    /// <param name="controller">The controller.</param>
    /// <param name="routeValues">
    /// The route values. If none are given, the <see cref="DtoBase.Id"/> of the given
    /// <see cref="ServiceResponse{T}.ResponseObject"/> is used.
    /// </param>
    /// <returns>Either a successful or an error result.</returns>
    ValueTask<ObjectResult> CreateOkResultBasedOnOutcomeAsync<TDto>(ServiceResponse<TDto> serviceResponse, bool readOnly = true, string action = "Get", string? controller = null, object? routeValues = null);

    /// <summary>
    /// Creates a paged collection result which is either a HAL resource, or a HAL-Forms resource
    /// based on the accept header.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The underlying entity type which has been queried through OData.
    /// </typeparam>
    /// <typeparam name="TDto">The type of the DTOs in the list.</typeparam>
    /// <param name="options">
    /// The OData query options which have been used to query the data for this page.
    /// </param>
    /// <param name="page">The page to return.</param>
    /// <param name="controller">The controller.</param>
    /// <param name="listGetMethod">
    /// The name of the get method for the list endpoint. Default is "GetList".
    /// </param>
    /// <param name="singleGetMethod">
    /// The name of the get method for the get-single endpoint. Default is "Get".
    /// </param>
    /// <param name="listPutMethod">
    /// The name of the put method for the update-multiple endpoint. Default is "Put".
    /// </param>
    /// <returns>Either a HAL resource, or a HAL-Forms resource containing the given <paramref name="page"/>.</returns>
    ValueTask<Resource> CreatePagedCollectionResourceAsync<TEntity, TDto>(ODataQueryOptions<TEntity> options, IReadOnlyPagedCollection<TDto> page, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put") where TDto : DtoBase;

    /// <summary>
    /// Creates a paged collection result which is either a HAL resource, or a HAL-Forms resource
    /// based on the accept header.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTOs in the list.</typeparam>
    /// <param name="options">
    /// The OData raw query options which have been used to query the data for this page.
    /// </param>
    /// <param name="maxTop">The default max top for pagination.</param>
    /// <param name="page">The page to return.</param>
    /// <param name="controller">The controller.</param>
    /// <param name="listGetMethod">
    /// The name of the get method for the list endpoint. Default is "GetList".
    /// </param>
    /// <param name="singleGetMethod">
    /// The name of the get method for the get-single endpoint. Default is "Get".
    /// </param>
    /// <param name="listPutMethod">
    /// The name of the put method for the update-multiple endpoint. Default is "Put".
    /// </param>
    /// <returns>Either a HAL resource, or a HAL-Forms resource containing the given <paramref name="page"/>.</returns>
    ValueTask<Resource> CreatePagedCollectionResourceAsync<TDto>(ODataRawQueryOptions options, int? maxTop, IReadOnlyPagedCollection<TDto> page, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put") where TDto : DtoBase;

    /// <summary>
    /// Creates a paged collection result based on the outcome of a service response. It either
    /// contains the successful result, or an error result. If the result is successful, it will be
    /// a HAL resource, or a HAL-Forms resource based on the accept header.
    /// </summary>
    /// <typeparam name="TEntity">
    /// The underlying entity type which has been queried through OData.
    /// </typeparam>
    /// <typeparam name="TDto">The type of the DTOs in the list.</typeparam>
    /// <param name="options">
    /// The OData query options which have been used to query the data for this page.
    /// </param>
    /// <param name="serviceResponse">The response from the service call.</param>
    /// <param name="controller">The controller.</param>
    /// <param name="listGetMethod">
    /// The name of the get method for the list endpoint. Default is "GetList".
    /// </param>
    /// <param name="singleGetMethod">
    /// The name of the get method for the get-single endpoint. Default is "Get".
    /// </param>
    /// <param name="listPutMethod">
    /// The name of the put method for the update-multiple endpoint. Default is "Put".
    /// </param>
    /// <returns>Either a successful or an error result.</returns>
    ValueTask<ObjectResult> CreatePagedCollectionResultBasedOnOutcomeAsync<TEntity, TDto>(ODataQueryOptions<TEntity> options, ServiceResponse<IReadOnlyPagedCollection<TDto>> serviceResponse, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put") where TDto : DtoBase;

    /// <summary>
    /// Creates a paged collection result based on the outcome of a service response. It either
    /// contains the successful result, or an error result. If the result is successful, it will be
    /// a HAL resource, or a HAL-Forms resource based on the accept header.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTOs in the list.</typeparam>
    /// <param name="options">
    /// The OData raw query options which have been used to query the data for this page.
    /// </param>
    /// <param name="maxTop">The default max top for pagination.</param>
    /// <param name="serviceResponse">The response from the service call.</param>
    /// <param name="controller">The controller.</param>
    /// <param name="listGetMethod">
    /// The name of the get method for the list endpoint. Default is "GetList".
    /// </param>
    /// <param name="singleGetMethod">
    /// The name of the get method for the get-single endpoint. Default is "Get".
    /// </param>
    /// <param name="listPutMethod">
    /// The name of the put method for the update-multiple endpoint. Default is "Put".
    /// </param>
    /// <returns>Either a successful or an error result.</returns>
    ValueTask<ObjectResult> CreatePagedCollectionResultBasedOnOutcomeAsync<TDto>(ODataRawQueryOptions options, int? maxTop, ServiceResponse<IReadOnlyPagedCollection<TDto>> serviceResponse, string? controller = null, string listGetMethod = "GetList", string singleGetMethod = "Get", string listPutMethod = "Put") where TDto : DtoBase;

    /// <summary>
    /// Creates a result which is either a HAL resource, or a HAL-Forms resource based on the accept header.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <param name="dto">The DTO to return.</param>
    /// <param name="method">The method to use when submitting the form.</param>
    /// <param name="readOnly">
    /// When this method returns HAL-Forms, this parameter determines if the form is read only or not.
    /// </param>
    /// <param name="action">
    /// The name of the action that is used to generate the self link. This is normally the name of
    /// the controller method without the async suffix. If none is provided, "Get" is used as default.
    /// </param>
    /// <param name="controller">The controller.</param>
    /// <param name="routeValues">The route values.</param>
    /// <returns>Either a HAL resource, or a HAL-Forms resource containing the given <paramref name="dto"/></returns>
    ValueTask<Resource> CreateResourceAsync<TDto>(TDto dto, HttpMethod method, bool readOnly = true, string action = "Get", string? controller = null, object? routeValues = null);

    /// <summary>
    /// Turns a HAL-Form read-only.
    /// </summary>
    /// <param name="form">The HAL-Form to make read-only</param>
    void MakeFormReadOnly(FormsResource? form);

    /// <summary>
    /// Turns a HAL-Form template read-only.
    /// </summary>
    /// <param name="template">The template to make read-only.</param>
    void MakeTemplateReadOnly(FormTemplate template);

    /// <summary>
    /// Turns a collection of HAL-Form templates read-only.
    /// </summary>
    /// <param name="templates">The templates to make read-only.</param>
    void MakeTemplatesReadOnly(IDictionary<string, FormTemplate>? templates);

    /// <summary>
    /// Turns a collection of HAL-Form templates read-only.
    /// </summary>
    /// <param name="templates">The templates to make read-only.</param>
    void MakeTemplatesReadOnly(IEnumerable<FormTemplate>? templates);
}