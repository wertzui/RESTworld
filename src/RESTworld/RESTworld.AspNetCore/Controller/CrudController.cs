using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Errors.Abstractions;
using RESTworld.AspNetCore.Serialization;
using RESTworld.AspNetCore.Swagger;
using RESTworld.Business.Models;
using RESTworld.Business.Services.Abstractions;
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
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Controller
{
    /// <summary>
    /// A basic CRUD controller offering operations for Create (also New for getting an empty
    /// instance), Read(List), Update and Delete.
    /// </summary>
    /// <typeparam name="TEntity">The type of the entity.</typeparam>
    /// <typeparam name="TCreateDto">The type of the DTO for a Create operation.</typeparam>
    /// <typeparam name="TGetListDto">The type of the DTO for a List operation.</typeparam>
    /// <typeparam name="TGetFullDto">The type of the DTO for a Get operation.</typeparam>
    /// <typeparam name="TUpdateDto">The type of the DTO for an Update operation.</typeparam>
    [Route("[controller]")]
    [RestControllerNameConvention(RestControllerNameConventionAttribute.CrudControllerIndexOfFullDtoType)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status401Unauthorized)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status403Forbidden)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status503ServiceUnavailable)]
    [ProducesErrorResponseType(typeof(Resource<ProblemDetails>))]
    public class CrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto> : ReadController<TEntity, TGetListDto, TGetFullDto>
        where TEntity : ConcurrentEntityBase
        where TGetListDto : ConcurrentDtoBase
        where TGetFullDto : ConcurrentDtoBase
        where TUpdateDto : ConcurrentDtoBase
    {
        /// <summary>
        /// These serializer options are used to create a JSON template for creating a new resource.
        /// We use <c>DefaultIgnoreCondition = JsonIgnoreCondition.Never</c> to keep all values.
        /// </summary>
        private static readonly JsonSerializerOptions _createNewResourceJsonSettings;

        private readonly ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto> _crudService;

        static CrudController()
        {
            _createNewResourceJsonSettings =
            new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.Never,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };
            _createNewResourceJsonSettings.Converters.Add(new JsonStringEnumMemberConverter(JsonNamingPolicy.CamelCase));
        }

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
        /// <param name="errorResultFactory">The factory to create error results.</param>
        /// <param name="options">
        /// The options which are used to determine the max number of entries for the List endpoint.
        /// </param>
        public CrudController(
            ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto> service,
            IODataResourceFactory resourceFactory,
            ILinkFactory linkFactory,
            IODataFormFactory formFactory,
            IErrorResultFactory errorResultFactory,
            IOptions<RestWorldOptions> options)
            : base(service, resourceFactory, linkFactory, formFactory, errorResultFactory, options)
        {
            _crudService = service ?? throw new ArgumentNullException(nameof(service));
        }

        /// <summary>
        /// Deletes the resource with the given ID and timestamp.
        /// </summary>
        /// <param name="id">The ID of the resource.</param>
        /// <param name="timestamp">
        /// The current timestamp of the resource. If the "If-Match" header is not present, this
        /// will be used for the timestamp.
        /// </param>
        /// <param name="timestampFromHeader">
        /// The current timestamp of the resource. This comes from the "If-Match" header. If that
        /// header is present, it will be used for the timestamp.
        /// </param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>An empty response.</returns>
        [HttpDelete("{id:long}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Delete))]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status409Conflict)]
        public virtual async Task<IActionResult> DeleteAsync(
            long id,
            [FromQuery] string? timestamp,
            [FromHeader(Name = "If-Match")] byte[]? timestampFromHeader,
            CancellationToken cancellationToken)
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

            var response = await _crudService.DeleteAsync(id, timestampBytes, cancellationToken);

            if (!response.Succeeded)
                return ErrorResultFactory.CreateError(response, "Delete");

            return Ok();

            static bool TryParseEncodedTimestamp(string timestamp, [NotNullWhen(true)] out byte[]? timestampBytes)
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
        /// Returns an empty resource which can be used as a template when creating a new resource.
        /// </summary>
        /// <param name="accept">The Accept header.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>An empty resource.</returns>
        [HttpGet("new")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(200)]
        public virtual Task<ActionResult<Resource<TCreateDto>>> NewAsync([FromHeader, SwaggerIgnore] string accept, CancellationToken cancellationToken)
        {
            var result = CreateEmpty();

            return Task.FromResult<ActionResult<Resource<TCreateDto>>>(Json(result, accept, _createNewResourceJsonSettings));
        }

        /// <summary>
        /// Creates the given new resource(s).
        /// </summary>
        /// <param name="dto">The resource(s) to create. Supply either a single object or a collection.</param>
        /// <param name="accept">The Accept header.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The full resource(s) as stored in the database.</returns>
        [HttpPost]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status409Conflict)]
        [ProducesResponseType(StatusCodes.Status201Created)]
        [ProducesResponseType(typeof(Resource), StatusCodes.Status201Created)]
        public virtual async Task<ActionResult<Resource<TGetFullDto>>> PostAsync(
            [FromBody] SingleObjectOrCollection<TCreateDto> dto,
            [FromHeader, SwaggerIgnore] string accept,
            CancellationToken cancellationToken)
        {
            if (dto == null || (dto.ContainsCollection && dto.Collection is null) || (dto.ContainsSingleObject && dto.SingleObject is null))
                return ErrorResultFactory.CreateError(StatusCodes.Status400BadRequest, "Unable to read either a collection or a single object from the request body.", "Post");

            if (dto.ContainsCollection)
                return await PostMultipleAsync(dto.Collection, cancellationToken);
            else
                return await PostSingleAsync(dto.SingleObject!, accept, cancellationToken);

        }

        /// <summary>
        /// Updates the given resource(s) with new values.
        /// </summary>
        /// <param name="id">
        /// The ID of the resource to update. It must match the ID of the DTO when updating a single
        /// object, or it must be absent when updating a collection.
        /// </param>
        /// <param name="dto">Updated values for the resource(s).</param>
        /// <param name="accept">The value of the Accept header.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The full resource(s) as stored in the database.</returns>
        [HttpPut("{id:long?}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
        [ProducesResponseType(StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Resource), StatusCodes.Status200OK)]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status409Conflict)]
        public virtual async Task<ActionResult<Resource<TGetFullDto>>> PutAsync(
            long? id,
            [FromBody] SingleObjectOrCollection<TUpdateDto> dto,
            [FromHeader, SwaggerIgnore] string accept,
            CancellationToken cancellationToken)
        {
            if (dto == null || (dto.ContainsCollection && dto.Collection is null) || (dto.ContainsSingleObject && dto.SingleObject is null))
                return ErrorResultFactory.CreateError(StatusCodes.Status400BadRequest, "Unable to read either a collection or a single object from the request body.", "Put");

            if (dto.ContainsCollection)
            {
                if (id.HasValue)
                    return ErrorResultFactory.CreateError(StatusCodes.Status400BadRequest, "The URL must not contain an ID when the request body contains a collection.", "Put");

                return await PutMultipleAsync(dto.Collection, cancellationToken);
            }
            else
            {
                if (id != dto.SingleObject?.Id)
                    return ErrorResultFactory.CreateError(StatusCodes.Status400BadRequest, "The URL must contain the same ID as the object in the request body when the request body contains a single object.", "Put");

                return await PutSingleAsync(dto.SingleObject!, accept, cancellationToken);
            }
        }

        /// <summary>
        /// This method is called by the <see cref="NewAsync"/> operation to create a template
        /// resource to return. The default implementation just calls the empty constructor and sets
        /// all strings to an empty string if they are not set initially.
        /// </summary>
        /// <returns></returns>
        protected virtual TCreateDto? CreateEmpty()
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
        /// Creates an Microsoft.AspNetCore.Mvc.OkObjectResult object that produces an Microsoft.AspNetCore.Http.StatusCodes.Status200OK
        /// response.
        /// </summary>
        /// <param name="dto">The DTO to return.</param>
        /// <param name="accept">The value of the Accept header.</param>
        /// <param name="jsonSerializerOptions">The options for the JSON serializer. Normally these are <see cref="_createNewResourceJsonSettings"/>.</param>
        /// <returns>The created Microsoft.AspNetCore.Mvc.OkObjectResult for the response.</returns>
        protected virtual JsonResult Json(TCreateDto? dto, string accept, JsonSerializerOptions jsonSerializerOptions)
            => new(CreateNewResource(dto, accept), jsonSerializerOptions);

        /// <summary>
        /// Creates an Microsoft.AspNetCore.Mvc.CreatedResult object that produces an Microsoft.AspNetCore.Http.StatusCodes.Status201Created
        /// response.
        /// </summary>
        /// <param name="dto">The DTO to return.</param>
        /// <param name="method">The method to use when submitting the form.</param>
        /// <param name="accept">The value of the Accept header.</param>
        /// <returns>The created Microsoft.AspNetCore.Mvc.CreatedResult for the response.</returns>
        protected virtual CreatedResult Created(TGetFullDto dto, HttpMethod method, string accept)
            => Created(Url.ActionLink(values: new { id = dto.Id }) ?? throw new UriFormatException("Unable to generate the CreatedAt URI"), CreateResource(dto, method, accept));

        /// <summary>
        /// Creates the result which is either a HAL resource, or a HAL-Forms resource based on the
        /// accept header.
        /// </summary>
        /// <param name="dto">The DTO to return.</param>
        /// <param name="accept">The value of the Accept header.</param>
        /// <returns>Either a HAL resource, or a HAL-Forms resource</returns>
        protected virtual Resource CreateNewResource(TCreateDto? dto, string accept)
        {
            if (accept.Contains("hal-forms+json"))
            {
                var result = FormFactory.CreateResourceForEndpoint(dto, HttpMethod.Post, "Create", action: HttpMethod.Post.Method);

                return result;
            }
            else
            {
                var result = ResourceFactory.CreateForGetEndpoint(dto, "New");

                var saveHref = Url.ActionLink(HttpMethod.Post.Method);
                if (saveHref is null)
                    throw new UriFormatException("Unable to generate the 'save' link.");

                result
                    .AddLink("save", new Link(saveHref) { Name = HttpMethod.Post.Method });

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
        /// <param name="accept">The value of the Accept header.</param>
        /// <returns>Either a HAL resource, or a HAL-Forms resource</returns>
        protected override Resource CreateResource(TGetFullDto dto, HttpMethod method, string accept)
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
        /// Creates the given new resources. This method is called when
        /// <see cref="PostAsync(SingleObjectOrCollection{TCreateDto}, string, CancellationToken)"/> is called with a collection.
        /// </summary>
        /// <param name="dtos">The resources to create.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The full resources as stored in the database.</returns>
        protected virtual async Task<ActionResult<Resource<TGetFullDto>>> PostMultipleAsync(IReadOnlyCollection<TCreateDto> dtos, CancellationToken cancellationToken)
        {
            var response = await _crudService.CreateAsync(dtos, cancellationToken);

            if (!response.Succeeded)
                return ErrorResultFactory.CreateError(response, "Post");

            var responseObject = response.ResponseObject ?? Array.Empty<TGetFullDto>();

            var resource = ResourceFactory.CreateForListEndpoint(responseObject, _ => "items", m => m.Id);

            if (resource.Embedded is not null)
            {
                foreach (var embedded in resource.Embedded.SelectMany(e => e.Value).Cast<Resource<TGetFullDto>>())
                {
                    Url.AddDeleteLink(embedded);
                }
            }

            var filter = CreateODataFilterForIds(responseObject.Select(d => d.Id));
            var url = Url.ActionLink() + "?$filter=" + filter;

            var selfLink = resource.Links?["self"].FirstOrDefault();
            if (selfLink is null)
                throw new Exception($"The result of {nameof(PostMultipleAsync)} does not have a 'self' link.");

            selfLink.Href = url;

            return Created(url, resource);
        }

        /// <summary>
        /// Creates the given new resource. This method is called when
        /// <see cref="PostAsync(SingleObjectOrCollection{TCreateDto}, string, CancellationToken)"/> is called with a single object.
        /// </summary>
        /// <param name="dto">The resource to create.</param>
        /// <param name="accept">The value of the Accept header.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The full resource as stored in the database.</returns>
        protected virtual async Task<ActionResult<Resource<TGetFullDto>>> PostSingleAsync(TCreateDto dto, string accept, CancellationToken cancellationToken)
        {
            var response = await _crudService.CreateAsync(dto, cancellationToken);

            if (!response.Succeeded)
                return ErrorResultFactory.CreateError(response, "Post");

            if (response.ResponseObject is null)
                return ErrorResultFactory.CreateError(StatusCodes.Status500InternalServerError, "The service did return null for the created object.", "Post");

            var resource = ResourceFactory.CreateForGetEndpoint(response.ResponseObject, routeValues: new { id = response.ResponseObject.Id });
            Url.AddSaveAndDeleteLinks(resource);

            return Created(response.ResponseObject, HttpMethod.Put, accept);
        }

        /// <summary>
        /// Updates the given resources with new values. This method is called
        /// when <see cref="PutAsync(long?, SingleObjectOrCollection{TUpdateDto}, string, CancellationToken)"/> is called with a collection.
        /// </summary>
        /// <param name="dtos">Updated values for the resources.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The full resources as stored in the database.</returns>
        protected virtual async Task<ActionResult<Resource<TGetFullDto>>> PutMultipleAsync(IReadOnlyCollection<TUpdateDto> dtos, CancellationToken cancellationToken)
        {
            var request = new UpdateMultipleRequest<TUpdateDto, TEntity>(dtos, x => x);
            var response = await _crudService.UpdateAsync(request, cancellationToken);

            if (!response.Succeeded)
                return ErrorResultFactory.CreateError(response, "Put");

            var responseObject = response.ResponseObject ?? Array.Empty<TGetFullDto>();

            var resource = ResourceFactory.CreateForListEndpoint(responseObject, _ => "items", m => m.Id);

            if (resource.Embedded is not null)
            {
                foreach (var embedded in resource.Embedded.SelectMany(e => e.Value).Cast<Resource<TGetFullDto>>())
                {
                    Url.AddDeleteLink(embedded);
                }
            }

            var filter = CreateODataFilterForIds(responseObject.Select(d => d.Id));
            var url = Url.ActionLink() + "?$filter=" + filter;

            var selfLink = resource.Links?["self"].FirstOrDefault();
            if (selfLink is null)
                throw new Exception($"The result of {nameof(PutMultipleAsync)} does not have a 'self' link.");

            selfLink.Href = url;

            return Ok(resource);
        }

        /// <summary>
        /// Updates the given resource with new values. This method is called
        /// when <see cref="PutAsync(long?, SingleObjectOrCollection{TUpdateDto}, string, CancellationToken)"/> is called with a single object.
        /// </summary>
        /// <param name="dto">Updated values for the resource.</param>
        /// <param name="accept">The Accept header.</param>
        /// <param name="cancellationToken">The <see cref="CancellationToken"/>.</param>
        /// <returns>The full resource as stored in the database.</returns>
        protected virtual async Task<ActionResult<Resource<TGetFullDto>>> PutSingleAsync(TUpdateDto dto, string accept, CancellationToken cancellationToken)
        {
            var response = await _crudService.UpdateAsync(dto, cancellationToken);

            if (!response.Succeeded || response.ResponseObject is null)
                return ErrorResultFactory.CreateError(response, "Put");

            return Ok(response.ResponseObject, HttpMethod.Put, accept);
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
    }
}