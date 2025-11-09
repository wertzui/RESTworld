using Asp.Versioning;
using ExampleBlog.Business.Services;
using ExampleBlog.Common.Dtos;
using HAL.AspNetCore.OData.Abstractions;
using HAL.AspNetCore.Utils;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESTworld.AspNetCore.Caching;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Results.Abstractions;
using System;
using System.ComponentModel;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Controllers;

[Route("postwithauthor")]
[ApiVersion("2")]
public class MyCustomController : RestControllerBase
{
    private readonly MyCustomService _service;
    private readonly IResultFactory _resultFactory;

    public MyCustomController(
        MyCustomService service,
        IODataResourceFactory resourceFactory,
        IResultFactory resultFactory,
        ICacheHelper cache)
        : base(resourceFactory, cache)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
    }

    [HttpGet("{id:long}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    [ProducesResponseType(typeof(Resource<PostWithAuthorDto>), StatusCodes.Status200OK)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]
    [Description("Gets a blog post including the author information.")]
    public async Task<ActionResult<Resource<PostWithAuthorDto>>> GetPostWithAuthorAsync(
        [Description("The ID of the post")] long id,
        CancellationToken cancellationToken)
    {
        var response = await Cache.GetOrCreateWithCurrentUserAsync(nameof(GetPostWithAuthorAsync) + "_" + id, nameof(CachingOptions.Get), _ => _service.GetPostWithAuthorAsync(id, cancellationToken));

        var result = await _resultFactory.CreateOkResultBasedOnOutcomeAsync(response, action: ActionHelper.StripAsyncSuffix(nameof(GetPostWithAuthorAsync)));

        return result;
    }
}
