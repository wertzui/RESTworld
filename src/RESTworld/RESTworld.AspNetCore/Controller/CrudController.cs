using HAL.AspNetCore.Controllers;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Serialization;
using RESTworld.AspNetCore.Swagger;
using RESTworld.Business;
using RESTworld.Business.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Controller
{
    /// <summary>
    /// A basic crud controller offering operations for Create (also New for getting an empty instance), Read(List), Update and Delete.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
    [Route("[controller]")]
    [CrudControllerNameConvention]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesErrorResponseType(typeof(Resource<ProblemDetails>))]
    public class CrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto> : HalControllerBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
        where TUpdateDto : DtoBase
    {
        /// <summary>
        /// These serializer options are used to create a json template for creating a new resource.
        /// We use <c>DefaultIgnoreCondition = JsonIgnoreCondition.Never</c> to keep all values.
        /// </summary>
        private static readonly JsonSerializerOptions _createNewResourceJsonSettings;

        private readonly RestWorldOptions _options;
        private readonly IODataResourceFactory _resourceFactory;
        private readonly ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto> _service;

        static CrudController()
        {
            _createNewResourceJsonSettings =
            new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _createNewResourceJsonSettings.Converters.Add(new JsonStringEnumConverter(JsonNamingPolicy.CamelCase));
        }

        /// <summary>
        /// Creates a new instance of the <see cref="CrudController{TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto}"/> class.
        /// </summary>
        /// <param name="service">The service which handles the business operations.</param>
        /// <param name="resourceFactory">The resource factory which creates HAL resources out of the service responses.</param>
        /// <param name="options">The options which are used to determine the max number of entries for the List endpoint.</param>
        public CrudController(
            ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto> service,
            IODataResourceFactory resourceFactory,
            IOptions<RestWorldOptions> options)
        {
            if (options is null)
                throw new ArgumentNullException(nameof(options));
            _options = options.Value;

            _service = service ?? throw new ArgumentNullException(nameof(service));
            _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
        }

        /// <summary>
        /// Deletes the resource with the given ID and timestamp.
        /// </summary>
        /// <param name="id">The ID of the resource.</param>
        /// <param name="timestamp">The current timestamp of the resource. If the "If-Match" header is not present, this will be used for the timestamp.</param>
        /// <param name="timestampFromHeader">The current timestamp of the resource. This comes from the "If-Match" header. If that header is present, it will be used for the timestamp.</param>
        /// <returns>An empty response.</returns>
        [HttpDelete("{id:long}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Delete))]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status409Conflict)]
        public virtual async Task<IActionResult> DeleteAsync(
            long id,
            [FromQuery] string? timestamp,
            [FromHeader(Name = "If-Match")] byte[]? timestampFromHeader)
        {
            var timestampBytes = timestampFromHeader;

            if (timestampBytes is null)
            {
                if (timestamp is null)
                {
                    ModelState.AddModelError("timestamp",
                        "You have to either pass the timestamp as query parameter or set the 'If-Match' header.");
                    return base.ValidationProblem();
                }

                if (!TryParseEncodedTimestamp(timestamp, out timestampBytes))
                {
                    ModelState.AddModelError("timestamp", "Invalid timestamp format. Format should be 8 base64 encoded bytes.");
                    return base.ValidationProblem();
                }
            }

            var response = await _service.DeleteAsync(id, timestampBytes);

            if (!response.Succeeded)
                return CreateError(response);

            return Ok();

            static bool TryParseEncodedTimestamp(string timestamp, [NotNullWhen(true)] out byte[] timestampBytes)
            {
                try
                {
                    timestampBytes = Base64UrlTextEncoder.Decode(timestamp);
                    return true;
                }
                catch (FormatException)
                {
                    timestampBytes = null;
                    return false;
                }
            }
        }

        /// <summary>
        /// Gets a full representation of the resource with the given ID.
        /// </summary>
        /// <param name="id">The ID of the resource.</param>
        /// <returns>The full representation fo the requested resource.</returns>
        [HttpGet("{id:long}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]
        public virtual async Task<ActionResult<Resource<TGetFullDto>>> GetAsync(long id)
        {
            var response = await _service.GetSingleAsync(id);

            if (!response.Succeeded)
                return CreateError(response);

            var result = _resourceFactory.CreateForGetEndpoint(response.ResponseObject);

            AddSaveAndDeleteLinks(result);

            return Ok(result);
        }

        /// <summary>
        /// Gets a paged list of resources matching the filter criteria.
        /// </summary>
        /// <param name="options">The OData options used to filter, order an page the list.</param>
        /// <param name="filter">The filter to filter resources by.</param>
        /// <param name="orderby">The order of the resources. If none is given, the resources are returned as they appear in the database.</param>
        /// <param name="top">The maximum number of resources to return. THis is used for paging.</param>
        /// <param name="skip">The number of resources to skip. This is used for paging.</param>
        /// <returns></returns>
        [HttpGet]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(200)]
        public virtual async Task<ActionResult<Resource>> GetListAsync(
            [SwaggerIgnore] ODataQueryOptions<TEntity> options,
            [FromQuery(Name = "$filter")] string? filter = default,
            [FromQuery(Name = "$orderby")] string? orderby = default,
            [FromQuery(Name = "$top")] long? top = default,
            [FromQuery(Name = "$skip")] long? skip = default)
        {
            options.Context.DefaultQuerySettings.MaxTop = _options.MaxNumberForListEndpoint;
            var getListrequest = options.ToListRequest(_options.CalculateTotalCountForListEndpoint);

            var response = await _service.GetListAsync(getListrequest);

            if (!response.Succeeded)
                return CreateError(response);

            var result = _resourceFactory.CreateForOdataListEndpointUsingSkipTopPaging(response.ResponseObject.Items, _ => "items", m => m.Id, options, options.Context.DefaultQuerySettings.MaxTop.Value, response.ResponseObject.TotalCount);

            if (result.Embedded is not null)
            {
                foreach (var embedded in result.Embedded.SelectMany(e => e.Value).Cast<Resource<TGetListDto>>())
                {
                    AddDeleteLink(embedded);
                }
            }

            result.AddLink(new Link { Name = "New", Href = Url.ActionLink("new") });

            return Ok(result);
        }

        /// <summary>
        /// Returns an empty resource which can be used as a template when creating a new resource.
        /// </summary>
        /// <returns>An empty resource.</returns>
        [HttpGet("new")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(200)]
        public virtual Task<ActionResult<Resource<TCreateDto>>> NewAsync()
        {
            var result = _resourceFactory.Create(CreateEmpty());

            result.AddLink("save",
                new Link { Name = HttpMethod.Post.Method, Href = Url.ActionLink(HttpMethod.Post.Method) });
            return Task.FromResult<ActionResult<Resource<TCreateDto>>>(new JsonResult(result, _createNewResourceJsonSettings));
        }

        /// <summary>
        /// Creates the given new resource(s).
        /// </summary>
        /// <param name="dto">The resource(s) to create. Supply either a single object or a collection.</param>
        /// <returns>The full resource(s) as stored in the database.</returns>
        [HttpPost]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(201)]
        public virtual async Task<ActionResult<Resource<TUpdateDto>>> PostAsync([FromBody] SingleObjectOrCollection<TCreateDto> dto)
        {
            if (dto == null)
                return CreateError(StatusCodes.Status400BadRequest, "Unable to read either a collection or a single object from the request body.");

            if (dto.ContainsCollection)
                return await PostMultipleAsync(dto.Collection);
            else
                return await PostSingleAsync(dto.SingleObject);
        }

        protected virtual async Task<ActionResult<Resource<TUpdateDto>>> PostSingleAsync(TCreateDto dto)
        {
            var response = await _service.CreateAsync(dto);

            if (!response.Succeeded)
                return CreateError(response);

            var resource = _resourceFactory.CreateForGetEndpoint(response.ResponseObject);
            AddSaveAndDeleteLinks(resource);

            return Created(Url.ActionLink(values: new { id = response.ResponseObject!.Id }), resource);
        }

        protected virtual async Task<ActionResult<Resource<TUpdateDto>>> PostMultipleAsync(IReadOnlyCollection<TCreateDto> dtos)
        {
            var response = await _service.CreateAsync(dtos);

            if (!response.Succeeded)
                return CreateError(response);

            var resource = _resourceFactory.CreateForListEndpoint(response.ResponseObject, _ => "items", m => m.Id);

            if (resource.Embedded is not null)
            {
                foreach (var embedded in resource.Embedded.SelectMany(e => e.Value).Cast<Resource<TGetFullDto>>())
                {
                    AddDeleteLink(embedded);
                }
            }

            var filter = CreateODataFilterForIds(response.ResponseObject.Select(d => d.Id));
            var url = Url.ActionLink() + "?$filter=" + filter;

            resource.Links["self"].First().Href = url;

            return Created(url, resource);
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

        /// <summary>
        /// Updates the given resource(s) with new values.
        /// </summary>
        /// <param name="id">The ID of the resource to update. It must match the ID of the DTO when updating a single object, or it must be absent when updating a collection.</param>
        /// <param name="dto">Updated values for the resource(s).</param>
        /// <returns>The full resource(s) as stored in the database.</returns>
        [HttpPut("{id:long?}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status409Conflict)]
        public virtual async Task<ActionResult<Resource<TUpdateDto>>> PutAsync(long? id, [FromBody] SingleObjectOrCollection<TUpdateDto> dto)
        {
            if (dto == null)
                return CreateError(StatusCodes.Status400BadRequest, "Unable to read either a collection or a single object from the request body.");

            if (dto.ContainsCollection)
            {
                if (id.HasValue)
                    return CreateError(StatusCodes.Status400BadRequest, "The URL must not contain an ID when the request body contains a collection.");

                return await PutMultipleAsync(dto.Collection);
            }
            else
            {
                if (id != dto.SingleObject.Id)
                    return CreateError(StatusCodes.Status400BadRequest, "The URL must contain the same ID as the object in the request body when the request body contains a single object.");

                return await PutSingleAsync(dto.SingleObject);
            }
        }

        /// <summary>
        /// Updates the given resources with new values.
        /// This method is called when <see cref="PutAsync(long?, SingleObjectOrCollection{TUpdateDto})"/> is called with a collection.
        /// </summary>
        /// <param name="dtos">Updated values for the resources.</param>
        /// <returns>The full resources as stored in the database.</returns>
        protected virtual async Task<ActionResult<Resource<TUpdateDto>>> PutMultipleAsync(IReadOnlyCollection<TUpdateDto> dtos)
        {
            var request = new UpdateMultipleRequest<TUpdateDto, TEntity>(dtos, x => x);
            var response = await _service.UpdateAsync(request);

            if (!response.Succeeded)
                return CreateError(response);

            var resource = _resourceFactory.CreateForListEndpoint(response.ResponseObject, _ => "items", m => m.Id);

            if (resource.Embedded is not null)
            {
                foreach (var embedded in resource.Embedded.SelectMany(e => e.Value).Cast<Resource<TGetFullDto>>())
                {
                    AddDeleteLink(embedded);
                }
            }

            var filter = CreateODataFilterForIds(response.ResponseObject.Select(d => d.Id));
            var url = Url.ActionLink() + "?$filter=" + filter;

            resource.Links["self"].First().Href = url;

            return Ok(resource);
        }

        /// <summary>
        /// Updates the given resource with new values.
        /// This method is called when <see cref="PutAsync(long?, SingleObjectOrCollection{TUpdateDto})"/> is called with a single object.
        /// </summary>
        /// <param name="dto">Updated values for the resource.</param>
        /// <returns>The full resource as stored in the database.</returns>
        protected virtual async Task<ActionResult<Resource<TUpdateDto>>> PutSingleAsync(TUpdateDto dto)
        {
            var response = await _service.UpdateAsync(dto);

            if (!response.Succeeded)
                return CreateError(response);

            var resource = _resourceFactory.CreateForGetEndpoint(response.ResponseObject);
            AddSaveAndDeleteLinks(resource);

            return Ok(resource);
        }

        /// <summary>
        /// This method is called by the <see cref="NewAsync"/> operation to create a template resource to return.
        /// The default implementation just calls the empty constructor and sets all strings to an empty string if they are not set initially.
        /// </summary>
        /// <returns></returns>
        protected virtual TCreateDto CreateEmpty()
        {
            var type = typeof(TCreateDto);
            var constructor = type.GetConstructor(Type.EmptyTypes);

            if (constructor is not null)
            {
                var dto = (TCreateDto)constructor.Invoke(Array.Empty<object>());
                var properties = type.GetProperties(BindingFlags.Public | BindingFlags.Instance | BindingFlags.SetProperty);
                foreach (var property in properties)
                {
                    if (property.PropertyType == typeof(string) && property.GetValue(dto) != default)
                    {
                        property.SetValue(dto, "");
                    }
                }

                return dto;
            }

            return default;
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
                _resourceFactory.Create(new ProblemDetails { Status = status, Detail = problemDetails });
            var result = StatusCode(resource.State.Status!.Value, resource);
            return result;
        }
    }
}