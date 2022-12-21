using Asp.Versioning;
using ExampleBlog.Business;
using ExampleBlog.Common.Dtos;
using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESTworld.AspNetCore.Controller;
using System;
using System.Threading.Tasks;

namespace ExampleBlog.Controllers
{
    [Route("postwithauthor")]
    [ApiVersion("1", Deprecated = true)]
    public class MyCustomController1 : RestControllerBase
    {
        private readonly MyCustomService _service;
        private readonly ILinkFactory _linkFactory;

        public MyCustomController1(
            MyCustomService service,
            IODataResourceFactory resourceFactory,
            ILinkFactory linkFactory)
            : base(resourceFactory)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
            _linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        }

        [HttpGet("postwithauthor/{id:long}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Resource<PostWithAuthorDto>>> GetPostWithAuthorAsync(long id)
        {
            var response = await _service.GetPostWithAuthor(id);

            if (!response.Succeeded)
                return ResourceFactory.CreateError(response);

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
