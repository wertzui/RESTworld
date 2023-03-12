using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using RESTworld.AspNetCore.Serialization;
using RESTworld.Business.Validation.Abstractions;

namespace RESTworld.AspNetCore.Validation.Abstractions
{
    /// <summary>
    /// A factory to fix JSON naming of property keys and to fix the handling of <see cref="SingleObjectOrCollection{T}"/>.
    /// </summary>
    public interface IRestWorldProblemDetailsFactory
    {
        /// <summary>
        /// Creates a <see cref="ProblemDetails" /> instance that configures defaults based on values specified in <see cref="ApiBehaviorOptions" />.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext" />.</param>
        /// <param name="statusCode">The value for <see cref="ProblemDetails.Status"/>.</param>
        /// <param name="title">The value for <see cref="ProblemDetails.Title" />.</param>
        /// <param name="type">The value for <see cref="ProblemDetails.Type" />.</param>
        /// <param name="detail">The value for <see cref="ProblemDetails.Detail" />.</param>
        /// <param name="instance">The value for <see cref="ProblemDetails.Instance" />.</param>
        /// <returns>The <see cref="ProblemDetails"/> instance.</returns>
        ProblemDetails CreateProblemDetails(HttpContext httpContext, int? statusCode = null, string? title = null, string? type = null, string? detail = null, string? instance = null);

        /// <summary>
        /// Creates a <see cref="ValidationProblemDetails" /> instance that configures defaults based on values specified in <see cref="ApiBehaviorOptions" />.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext" />.</param>
        /// <param name="validationResults">The <see cref="IValidationResults" />.</param>
        /// <param name="statusCode">The value for <see cref="ProblemDetails.Status"/>.</param>
        /// <param name="title">The value for <see cref="ProblemDetails.Title" />.</param>
        /// <param name="type">The value for <see cref="ProblemDetails.Type" />.</param>
        /// <param name="detail">The value for <see cref="ProblemDetails.Detail" />.</param>
        /// <param name="instance">The value for <see cref="ProblemDetails.Instance" />.</param>
        /// <returns>The <see cref="ValidationProblemDetails"/> instance.</returns>
        ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext, IValidationResults validationResults, int? statusCode = null, string? title = null, string? type = null, string? detail = null, string? instance = null);

        /// <summary>
        /// Creates a <see cref="ValidationProblemDetails" /> instance that configures defaults based on values specified in <see cref="ApiBehaviorOptions" />.
        /// </summary>
        /// <param name="httpContext">The <see cref="HttpContext" />.</param>
        /// <param name="modelStateDictionary">The <see cref="ModelStateDictionary" />.</param>
        /// <param name="statusCode">The value for <see cref="ProblemDetails.Status"/>.</param>
        /// <param name="title">The value for <see cref="ProblemDetails.Title" />.</param>
        /// <param name="type">The value for <see cref="ProblemDetails.Type" />.</param>
        /// <param name="detail">The value for <see cref="ProblemDetails.Detail" />.</param>
        /// <param name="instance">The value for <see cref="ProblemDetails.Instance" />.</param>
        /// <returns>The <see cref="ValidationProblemDetails"/> instance.</returns>
        ValidationProblemDetails CreateValidationProblemDetails(HttpContext httpContext, ModelStateDictionary modelStateDictionary, int? statusCode = null, string? title = null, string? type = null, string? detail = null, string? instance = null);
    }
}