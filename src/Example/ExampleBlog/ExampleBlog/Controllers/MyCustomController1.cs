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
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Controllers;

[Route("postwithauthor")]
[ApiVersion("1", Deprecated = true)]
public class MyCustomController1 : RestControllerBase
{
    private readonly MyCustomService _service;
    private readonly IResultFactory _resultFactory;

    public MyCustomController1(
        MyCustomService service,
        IODataResourceFactory resourceFactory,
        IResultFactory resultFactory,
        ICacheHelper cache)
        : base(resourceFactory, cache)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _resultFactory = resultFactory ?? throw new ArgumentNullException(nameof(resultFactory));
    }

    [HttpGet("postwithauthor/{id:long}")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    [ProducesResponseType(200)]
    [ProducesResponseType(typeof(Resource<ProblemDetails>), StatusCodes.Status404NotFound)]
    public async Task<ActionResult<Resource<PostWithAuthorDto>>> GetPostWithAuthorAsync(
        long id,
        CancellationToken cancellationToken)
    {
        var response = await Cache.GetOrCreateWithCurrentUserAsync(nameof(GetPostWithAuthorAsync) + "_" + id, nameof(CachingOptions.Get), _ => _service.GetPostWithAuthorAsync(id, cancellationToken));

        var result = await _resultFactory.CreateOkResultBasedOnOutcomeAsync(response, action: ActionHelper.StripAsyncSuffix(nameof(GetPostWithAuthorAsync)));

        return result;
    }
}
