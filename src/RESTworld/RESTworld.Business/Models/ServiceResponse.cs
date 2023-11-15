using RESTworld.Business.Validation.Abstractions;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Net;

namespace RESTworld.Business.Models;

/// <summary>
/// A response from a service call. Includes the result or the error which occurred during the call.
/// </summary>
/// <typeparam name="T">The type of the response object.</typeparam>
public record ServiceResponse<T>
{
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceResponse{T}"/> class.
    /// </summary>
    /// <param name="status">The status.</param>
    internal ServiceResponse(HttpStatusCode status)
    {
        Status = status;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceResponse{T}"/> class.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="responseObject">The response object.</param>
    internal ServiceResponse(HttpStatusCode status, T responseObject)
    {
        Status = status;
        if (!Succeeded)
            throw new ArgumentOutOfRangeException(nameof(status), status, "The status must be in the range of 2xx.");

        ResponseObject = responseObject;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceResponse{T}"/> class.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="problemDetails">The problem details.</param>
    internal ServiceResponse(HttpStatusCode status, string? problemDetails)
    {
        Status = status;
        if (Succeeded)
            throw new ArgumentOutOfRangeException(nameof(status), status, "The status must not be in the range of 2xx.");

        ProblemDetails = problemDetails;
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceResponse{T}"/> class.
    /// </summary>
    /// <param name="validationResults"></param>
    /// <exception cref="ArgumentNullException"></exception>
    internal ServiceResponse(IValidationResults validationResults)
        : this(HttpStatusCode.BadRequest, validationResults)
    {

    }
    /// <summary>
    /// Initializes a new instance of the <see cref="ServiceResponse{T}"/> class.
    /// </summary>
    /// <param name="status">The status.</param>
    /// <param name="validationResults"></param>
    /// <exception cref="ArgumentNullException"></exception>
    internal ServiceResponse(HttpStatusCode status, IValidationResults validationResults)
    {
        Status = status;
        ValidationResults = validationResults ?? throw new ArgumentNullException(nameof(validationResults));
        if (validationResults.ValidationSucceeded)
            throw new ArgumentException("A service response can only be created from validation failures if the given instance actually contains any failures.", nameof(validationResults));

        ProblemDetails = "Validation errors occurred.";
    }

    /// <summary>
    /// Gets the response object.
    /// </summary>
    /// <value>The response object.</value>
    public T? ResponseObject { get; }

    /// <summary>
    /// Gets a value indicating whether the service call succeeded.
    /// </summary>
    /// <value><c>true</c> if succeeded; otherwise, <c>false</c>.</value>
    [MemberNotNullWhen(true, nameof(ResponseObject))]
    [MemberNotNullWhen(false, nameof(ProblemDetails))]
    public bool Succeeded => (int)Status >= 200 && (int)Status < 300;

    /// <summary>
    /// Gets a value indicating whether there are any validation errors or not.
    /// </summary>
    /// <value><c>true</c> if the validation succeeded; otherwise, <c>false</c>.</value>
    [MemberNotNullWhen(false, nameof(ProblemDetails))]
    [MemberNotNullWhen(false, nameof(ValidationResults))]
    public bool ValidationSucceded => ValidationResults is null || ValidationResults.ValidationSucceeded;

    /// <summary>
    /// Gets the problem details if the service call failed.
    /// </summary>
    /// <value>The problem details.</value>
    public string? ProblemDetails { get; }

    /// <summary>
    /// Gets the status. This is either in the 2xx range if the call succeeded, or any other
    /// code if the call failed.
    /// </summary>
    /// <value>The status.</value>
    public HttpStatusCode Status { get; }

    /// <summary>
    /// Gets the results of the a validation process if there have been validation errors.
    /// </summary>
    public IValidationResults? ValidationResults { get; }
}

/// <summary>
/// A static helper class to create <see cref="ServiceResponse{T}"/> instances.
/// </summary>
public static class ServiceResponse
{
    /// <summary>
    /// Creates a response from an exception and a custom status code.
    /// </summary>
    /// <typeparam name="T">The type of the response object.</typeparam>
    /// <param name="status">The status.</param>
    /// <param name="exception">The exception.</param>
    /// <returns></returns>
    public static ServiceResponse<T> FromException<T>(HttpStatusCode status, Exception exception)
        => FromProblem<T>(status, exception.ToString());

    /// <summary>
    /// Creates a response from an exception and the status code 500.
    /// </summary>
    /// <typeparam name="T">The type of the response object.</typeparam>
    /// <param name="exception">The exception.</param>
    /// <returns></returns>
    public static ServiceResponse<T> FromException<T>(Exception exception)
        => FromException<T>(HttpStatusCode.InternalServerError, exception);

    /// <summary>
    /// Creates a response from a failed validation.
    /// </summary>
    /// <typeparam name="T">The type of the response object.</typeparam>
    /// <param name="validationResults">The results from the failed validation.</param>
    /// <returns></returns>
    public static ServiceResponse<T> FromFailedValidation<T>(IValidationResults validationResults)
        => new(validationResults);

    /// <summary>
    /// Creates a response from a failed validation.
    /// </summary>
    /// <typeparam name="T">The type of the response object.</typeparam>
    /// <param name="status">The status.</param>
    /// <param name="validationResults">The results from the failed validation.</param>
    /// <returns></returns>
    public static ServiceResponse<T> FromFailedValidation<T>(HttpStatusCode status, IValidationResults validationResults)
        => new(status, validationResults);

    /// <summary>
    /// Creates a response from a problem description and a custom status code.
    /// </summary>
    /// <typeparam name="T">The type of the response object.</typeparam>
    /// <param name="status">The status.</param>
    /// <param name="problemDetails">The problem details.</param>
    /// <returns></returns>
    public static ServiceResponse<T> FromProblem<T>(HttpStatusCode status, string problemDetails)
        => new(status, problemDetails);

    /// <summary>
    /// Creates a response from a problem description and the status code 500.
    /// </summary>
    /// <typeparam name="T">The type of the response object.</typeparam>
    /// <param name="problemDetails">The problem details.</param>
    /// <returns></returns>
    public static ServiceResponse<T> FromProblem<T>(string problemDetails)
        => FromProblem<T>(HttpStatusCode.InternalServerError, problemDetails);

    /// <summary>
    /// Creates a response from result object and the status code 200.
    /// </summary>
    /// <typeparam name="T">The type of the response object.</typeparam>
    /// <param name="responseObject">The response object.</param>
    /// <returns></returns>
    public static ServiceResponse<T> FromResult<T>(T responseObject)
        => new(HttpStatusCode.OK, responseObject);

    /// <summary>
    /// Creates a response from result object and a custom status code.
    /// </summary>
    /// <typeparam name="T">The type of the response object.</typeparam>
    /// <param name="status">The status.</param>
    /// <param name="responseObject">The response object.</param>
    /// <returns></returns>
    public static ServiceResponse<T> FromResult<T>(HttpStatusCode status, T responseObject)
        => new(status, responseObject);

    /// <summary>
    /// Creates a response a custom status code.
    /// </summary>
    /// <typeparam name="T">The type of the response object.</typeparam>
    /// <param name="status">The status.</param>
    /// <returns></returns>
    public static ServiceResponse<T> FromStatus<T>(HttpStatusCode status)
        => (int)status >= 200 && (int)status < 300 ? new(status) : FromProblem<T>(status, status.ToString());
}