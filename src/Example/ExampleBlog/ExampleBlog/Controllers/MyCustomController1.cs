﻿using Asp.Versioning;
using ExampleBlog.Business.Services;
using ExampleBlog.Common.Dtos;
using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESTworld.AspNetCore.Caching;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Links.Abstractions;
using RESTworld.AspNetCore.Results.Errors.Abstractions;
using System;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Controllers;

[Route("postwithauthor")]
[ApiVersion("1", Deprecated = true)]
public class MyCustomController1 : RestControllerBase
{
    private readonly MyCustomService _service;
    private readonly ICrudLinkFactory _linkFactory;
    private readonly IErrorResultFactory _errorResultFactory;

    public MyCustomController1(
        MyCustomService service,
        IODataResourceFactory resourceFactory,
        ICrudLinkFactory linkFactory,
        IErrorResultFactory errorResultFactory,
        ICacheHelper cache)
        : base(resourceFactory, cache)
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
        var response = await Cache.GetOrCreateWithCurrentUserAsync(nameof(GetPostWithAuthorAsync) + "_" + id, nameof(CachingOptions.Get), _ => _service.GetPostWithAuthorAsync(id, cancellationToken));

        if (!response.Succeeded)
            return _errorResultFactory.CreateError(response, "Get");

        var result = ResourceFactory.CreateForEndpoint(response.ResponseObject, null);
        var authorId = result.State?.AuthorId;

        if (authorId is not null)
        {
            var link = _linkFactory.Create("Author", "Get", RestControllerNameConventionAttribute.CreateNameFromType<AuthorDtoV1>(), new { id = authorId });
            result.AddLink(link);
        }

        _linkFactory.AddSaveAndDeleteLinks(result);

        return Ok(result);
    }
}
