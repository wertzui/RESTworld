using Asp.Versioning;
using ExampleBlog.Business.Services;
using ExampleBlog.Common.Dtos;
using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.Errors.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Controllers
{
    [Route("postwithauthor")]
    [ApiVersion("1", Deprecated = true)]
    public class MyCustomController1 : RestControllerBase
    {
        private readonly MyCustomService _service;
        private readonly ILinkFactory _linkFactory;
        private readonly IErrorResultFactory _errorResultFactory;

        public MyCustomController1(
            MyCustomService service,
            IODataResourceFactory resourceFactory,
            ILinkFactory linkFactory,
            IErrorResultFactory errorResultFactory)
            : base(resourceFactory)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
            _errorResultFactory = errorResultFactory ?? throw new ArgumentNullException(nameof(errorResultFactory));
        }

        [HttpGet("postwithauthor/{id:long}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Resource<PostWithAuthorDto>>> GetPostWithAuthorAsync(
            long id,
            CancellationToken cancellationToken)
        {
            var response = await _service.GetPostWithAuthorAsync(id, cancellationToken);

            if (!response.Succeeded)
                return _errorResultFactory.CreateError(response, "Get");

            var result = ResourceFactory.CreateForGetEndpoint(response.ResponseObject, null);
            var authorId = result.State?.AuthorId;

            if (authorId is not null)
            {
                var link = _linkFactory.Create("Author", "Get", RestControllerNameConventionAttribute.CreateNameFromType<AuthorDtoV1>(), new { id = authorId });
                result.AddLink(link);
            }

            Url.AddSaveAndDeleteLinks(result);

            return Ok(result);
        }
    }
}
