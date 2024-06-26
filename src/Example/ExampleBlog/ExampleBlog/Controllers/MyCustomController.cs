﻿using Asp.Versioning;
using ExampleBlog.Business.Services;
using ExampleBlog.Common.Dtos;
using HAL.AspNetCore.Abstractions;
using HAL.AspNetCore.ContentNegotiation;
using HAL.AspNetCore.Forms.Abstractions;
using HAL.AspNetCore.OData.Abstractions;
using HAL.AspNetCore.Utils;
using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESTworld.AspNetCore.Caching;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.Results.Errors.Abstractions;
using System;
using System.Net.Http;
using System.Threading;
using System.Threading.Tasks;

namespace ExampleBlog.Controllers;

[Route("postwithauthor")]
[ApiVersion("2")]
public class MyCustomController : RestControllerBase
{
    private readonly MyCustomService _service;
    private readonly ILinkFactory _linkFactory;
    private readonly IFormFactory _formFactory;
    private readonly IErrorResultFactory _errorResultFactory;

    public MyCustomController(
        MyCustomService service,
        IODataResourceFactory resourceFactory,
        ILinkFactory linkFactory,
        IFormFactory formFactory,
        IErrorResultFactory errorResultFactory,
        ICacheHelper cache)
        : base(resourceFactory, cache)
    {
        _service = service ?? throw new ArgumentNullException(nameof(service));
        _linkFactory = linkFactory ?? throw new ArgumentNullException(nameof(linkFactory));
        _formFactory = formFactory ?? throw new ArgumentNullException(nameof(formFactory));
        _errorResultFactory = errorResultFactory ?? throw new ArgumentNullException(nameof(errorResultFactory));
    }

    [HttpGet("{id:long}")]
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

        var dto = response.ResponseObject;
        if (dto is null)
            return NotFound();

        if (HttpContext.GetAcceptHeaders().AcceptsHalFormsOverHal())
        {
            var result = _formFactory.CreateResourceForEndpoint(dto, HttpMethod.Get, "Post", action: ActionHelper.StripAsyncSuffix(nameof(GetPostWithAuthorAsync)));

            // When getting a form, instead of a link to the author, we just add another form with the author already filled in
            var authorLink = Url.ActionLink("Get", RestControllerNameConventionAttribute.CreateNameFromType<AuthorDto>(), new { id = dto.AuthorId }) ?? "";
            var authorForm = _formFactory.CreateForm(dto.Author, authorLink, "Get", "Author");
            result.Templates["The author"] = authorForm;

            return Ok(result);
        }
        else
        {
            var result = ResourceFactory.CreateForEndpoint(dto, null);

            _linkFactory.AddFormLinkForExistingLinkTo(result, Constants.SelfLinkName);

            // Add a link to the author
            var link = _linkFactory.Create("Author", "Get", RestControllerNameConventionAttribute.CreateNameFromType<AuthorDto>(), new { id = dto.AuthorId });
            result.AddLink(link);

            return Ok(result);
        }
    }
}
