using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNet.OData.Query;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.WebUtilities;
using Microsoft.Extensions.Options;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Swagger;
using RESTworld.Business;
using RESTworld.Business.Abstractions;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Controller
{
    [ApiController]
    [Route("[controller]")]
    [CrudControllerNameConvention]
    public class CrudController<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto> : ControllerBase
        where TEntity : EntityBase
        where TGetListDto : DtoBase
        where TGetFullDto : DtoBase
        where TUpdateDto : DtoBase
    {
        /// <summary>
        /// These serializer options are used to create a json template for creating a new resource.
        /// We use <c>DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull</c> to keep value type properties like
        /// <c>bool</c> and <c>int</c> but omit properties with <c>null</c> values.
        /// Other properties that should be included in the json have to be set to a non <c>null</c> value.
        /// </summary>
        private static readonly JsonSerializerOptions _createNewResourceJsonSettings =
            new()
            {
                DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
                PropertyNamingPolicy = JsonNamingPolicy.CamelCase
            };

        private readonly RestWorldOptions _options;
        private readonly IODataResourceFactory _resourceFactory;
        private readonly ICrudServiceBase<TEntity, TCreateDto, TGetListDto, TGetFullDto, TUpdateDto> _service;

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

        [HttpDelete("{id:long}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Delete))]
        public virtual async Task<IActionResult> DeleteAsync(
            long id,
            [FromQuery] string timestamp,
            [FromHeader(Name = "If-Match")] byte[] timestampFromHeader)
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

        [HttpGet("{id:long}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public virtual async Task<ActionResult<Resource<TGetFullDto>>> GetAsync(long id)
        {
            var response = await _service.GetSingleAsync(id);

            if (!response.Succeeded)
                return CreateError(response);

            var result = _resourceFactory.CreateForGetEndpoint(response.ResponseObject);

            AddSaveAndDeleteLinks(result);

            return Ok(result);
        }

        [HttpGet]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public virtual async Task<ActionResult<Resource>> GetListAsync(
            [SwaggerIgnore] ODataQueryOptions<TEntity> options,
            [FromQuery(Name = "$filter")] string filter = default,
            [FromQuery(Name = "$orderby")] string orderby = default,
            [FromQuery(Name = "$top")] long? top = default,
            [FromQuery(Name = "$skip")] long? skip = default)
        {
            var response = await _service.GetListAsync(set => options.ApplyTo(set).Cast<TEntity>().Take(_options.MaxNumberForListEndpoint));

            if (!response.Succeeded)
                return CreateError(response);

            var result = _resourceFactory.CreateForOdataListEndpointUsingSkipTopPaging(response.ResponseObject, m => m.Id, options, _options.MaxNumberForListEndpoint);

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

        [HttpGet("new")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        public virtual Task<ActionResult<Resource<TCreateDto>>> NewAsync()
        {
            var result = _resourceFactory.Create(CreateEmpty());

            result.AddLink("save",
                new Link { Name = HttpMethod.Post.Method, Href = Url.ActionLink(HttpMethod.Post.Method) });
            return Task.FromResult<ActionResult<Resource<TCreateDto>>>(new JsonResult(result, _createNewResourceJsonSettings));
        }

        [HttpPost]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Post))]
        public virtual async Task<ActionResult<Resource<TUpdateDto>>> PostAsync([FromBody] TCreateDto dto)
        {
            if (dto == null)
                return BadRequest();

            var response = await _service.CreateAsync(dto);

            if (!response.Succeeded)
                return CreateError(response);

            var resource = _resourceFactory.CreateForGetEndpoint(response.ResponseObject);
            AddSaveAndDeleteLinks(resource);

            return Created(Url.ActionLink(values: new { id = response.ResponseObject!.Id }), resource);
        }

        [HttpPut("{id:long}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Put))]
        public async Task<ActionResult<Resource<TUpdateDto>>> PutAsync(long id, [FromBody] TUpdateDto dto)
        {
            if (dto == null)
                return BadRequest();

            if (id != dto.Id)
                return BadRequest();

            var response = await _service.UpdateAsync(dto);

            if (!response.Succeeded)
                return CreateError(response);

            var resource = _resourceFactory.CreateForGetEndpoint(response.ResponseObject);
            AddSaveAndDeleteLinks(resource);

            return Ok(resource);
        }

        protected virtual TCreateDto CreateEmpty() => default;

        private void AddDeleteLink<TDto>(Resource<TDto> result)
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

        private void AddSaveAndDeleteLinks<TDto>(Resource<TDto> result)
            where TDto : DtoBase
        {
            AddSaveLink(result);
            AddDeleteLink(result);
        }

        private void AddSaveLink<TDto>(Resource<TDto> result)
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

        private ObjectResult CreateError<T>(ServiceResponse<T> response)
        {
            var resource =
                _resourceFactory.Create(new ProblemDetails { Status = (int)response.Status, Detail = response.ProblemDetails });
            var result = StatusCode(resource.State.Status!.Value, resource);
            return result;
        }
    }
}