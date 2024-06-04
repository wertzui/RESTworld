using Microsoft.AspNetCore.Mvc;
using RESTworld.Business.Models;

namespace RESTworld.AspNetCore.Results.Errors.Abstractions;

/// <summary>
/// A factory to create Error results which may come from validation failures.
/// </summary>
public interface IErrorResultFactory
{
    /// <summary>
    /// Creates an error to return out of the given status and problem details.
    /// </summary>
    /// <param name="status">A valid HTTP status code.</param>
    /// <param name="problemDetails">Details of the problem to return to the user.</param>
    /// <param name="action">The action that was called.</param>
    /// <param name="controller">The controller that was called.</param>
    /// <param name="routeValues">The route values that were used.</param>
    /// <returns>A result with the problem details and the given status code.</returns>
    ObjectResult CreateError(int status, string? problemDetails, string action, string? controller = null, object? routeValues = null);

    /// <summary>
    /// Creates an error to return out of the given status and problem details.
    /// </summary>
    /// <param name="problemDetails">Details of the problem to return to the user.</param>
    /// <param name="action">The action that was called.</param>
    /// <param name="controller">The controller that was called.</param>
    /// <param name="routeValues">The route values that were used.</param>
    /// <returns>A result with the problem details and the given status code.</returns>
    ObjectResult CreateError<TProblemDetails>(TProblemDetails problemDetails, string action, string? controller = null, object? routeValues = null)
        where TProblemDetails : ProblemDetails;

    /// <summary>
    /// Creates an error to return out of the given <see cref="ServiceResponse{T}"/> s status
    /// and problem details.
    /// </summary>
    /// <typeparam name="T">The type of the service response.</typeparam>
    /// <param name="response">The service response holding a status and problem details.</param>
    /// <param name="action">The action that was called.</param>
    /// <param name="controller">The controller that was called.</param>
    /// <param name="routeValues">The route values that were used.</param>
    /// <returns>A result with the problem details and the given status code.</returns>
    ObjectResult CreateError<T>(ServiceResponse<T> response, string action, string? controller = null, object? routeValues = null);
}