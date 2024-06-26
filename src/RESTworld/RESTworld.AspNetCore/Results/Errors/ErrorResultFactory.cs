﻿using HAL.AspNetCore.Abstractions;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using RESTworld.AspNetCore.Results.Errors.Abstractions;
using RESTworld.AspNetCore.Validation.Abstractions;
using RESTworld.Business.Models;
using System;
using System.Net;

namespace RESTworld.AspNetCore.Results.Errors;

/// <inheritdoc/>
public class ErrorResultFactory : IErrorResultFactory
{
    private readonly IHttpContextAccessor _httpContextAccessor;
    private readonly IRestWorldProblemDetailsFactory _problemDetailsFactory;
    private readonly IResourceFactory _resourceFactory;

    /// <summary>
    /// Creates a new instance of the <see cref="ErrorResultFactory"/> class.
    /// </summary>
    /// <param name="httpContextAccessor"></param>
    /// <param name="problemDetailsFactory"></param>
    /// <param name="resourceFactory">The resource factory that is used to create the result.</param>
    /// <exception cref="ArgumentNullException"></exception>
    /// <exception cref="ArgumentNullException"></exception>
    public ErrorResultFactory(IHttpContextAccessor httpContextAccessor, IRestWorldProblemDetailsFactory problemDetailsFactory, IResourceFactory resourceFactory)
    {
        _httpContextAccessor = httpContextAccessor ?? throw new ArgumentNullException(nameof(httpContextAccessor));
        _problemDetailsFactory = problemDetailsFactory ?? throw new ArgumentNullException(nameof(problemDetailsFactory));
        _resourceFactory = resourceFactory ?? throw new ArgumentNullException(nameof(resourceFactory));
    }

    /// <inheritdoc/>
    public ObjectResult CreateError<T>(ServiceResponse<T> response, string action, string? controller = null, object? routeValues = null)
    {
        ArgumentNullException.ThrowIfNull(response);

        if (!response.ValidationSucceded)
            return CreateError(_problemDetailsFactory.CreateValidationProblemDetails(GetHttpContext(), response.ValidationResults, (int)response.Status, response.Status.ToString()), action, controller, routeValues);

        return CreateError((int)response.Status, response.ProblemDetails, action, controller, routeValues);
    }

    /// <inheritdoc/>
    public ObjectResult CreateError(int status, string? problemDetails, string action, string? controller = null, object? routeValues = null)
    {
        var result = CreateError(new ProblemDetails { Title = Enum.GetName(typeof(HttpStatusCode), status), Status = status, Detail = problemDetails }, action, controller, routeValues);

        return result;
    }

    /// <inheritdoc/>
    public ObjectResult CreateError<TProblemDetails>(TProblemDetails problemDetails, string action, string? controller = null, object? routeValues = null)
        where TProblemDetails : ProblemDetails
    {
        ArgumentNullException.ThrowIfNull(problemDetails);

        var resource = _resourceFactory.CreateForEndpoint(problemDetails, action, controller, routeValues);

        var result = new ObjectResult(resource) { StatusCode = problemDetails.Status };
        return result;
    }

    private HttpContext GetHttpContext() => _httpContextAccessor.HttpContext ?? throw new InvalidOperationException("The HTTP context accessor returned no HTTP context.");
}