using ExampleBlog.Business;
using ExampleBlog.Common.Dtos;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESTworld.AspNetCore.Controller;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace ExampleBlog.Controllers
{

    public class MyCustomController : RestControllerBase
    {
        private readonly MyCustomService _service;

        public MyCustomController(
            MyCustomService service,
            IODataResourceFactory resourceFactory)
            : base(resourceFactory)
        {
            _service = service ?? throw new ArgumentNullException(nameof(service));
        }

        [HttpGet("postwithauthor/{id:long}")]
        [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
        [ProducesResponseType(200)]
        [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]
        public async Task<ActionResult<Resource<PostWithAuthorDto>>> GetPostWithAuthorAsync(long id)
        {
            var response = await _service.GetPostWithAuthor(id);

            if (!response.Succeeded)
                return CreateError(response);

            var result = _resourceFactory.CreateForGetEndpoint(response.ResponseObject);

            AddSaveAndDeleteLinks(result);

            return Ok(result);
        }
    }
}
