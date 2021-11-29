using HAL.Common;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Mvc.ModelBinding;
using Microsoft.AspNetCore.Routing;
using Microsoft.Extensions.Options;
using RESTworld.AspNetCore.Serialization;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;

namespace RESTworld.AspNetCore.Validation
{
    /// <summary>
    /// A factory to fix JSON naming of property keys and to fix the handling of <see cref="SingleObjectOrCollection{T}"/>.
    /// </summary>
    /// <remarks>Partly from https://github.com/dotnet/aspnetcore/blob/main/src/Mvc/Mvc.Core/src/Infrastructure/DefaultProblemDetailsFactory.cs</remarks>
    public class RestWorldProblemDetailsFactory : ProblemDetailsFactory
    {
        private readonly ApiBehaviorOptions _apiBehaviorOptions;
        private readonly JsonOptions _jsonOptions;
        private readonly LinkGenerator _linkGenerator;
        private readonly IActionContextAccessor _actionContextAccessor;

        /// <summary>
        /// Creates a new instance of the <see cref="RestWorldProblemDetailsFactory"/>.
        /// </summary>
        /// <param name="apiBehaviorOptions">The API behavior options.</param>
        /// <param name="jsonOptions">The JSON options.</param>
        /// <param name="linkGenerator">The link generator which is used to generate the self link.</param>
        /// <param name="actionContextAccessor"></param>
        /// <exception cref="ArgumentNullException"></exception>
        public RestWorldProblemDetailsFactory(
            IOptions<ApiBehaviorOptions> apiBehaviorOptions,
            IOptions<JsonOptions> jsonOptions,
            LinkGenerator linkGenerator,
            IActionContextAccessor actionContextAccessor)
        {
            _apiBehaviorOptions = apiBehaviorOptions?.Value ?? throw new ArgumentNullException(nameof(apiBehaviorOptions));
            _jsonOptions = jsonOptions?.Value ?? throw new ArgumentNullException(nameof(jsonOptions));
            _linkGenerator = linkGenerator ?? throw new ArgumentNullException(nameof(linkGenerator));
            _actionContextAccessor = actionContextAccessor ?? throw new ArgumentNullException(nameof(actionContextAccessor));
        }

        /// <inheritdoc/>
        public override ProblemDetails CreateProblemDetails(
            HttpContext httpContext,
            int? statusCode = null,
            string title = null,
            string type = null,
            string detail = null,
            string instance = null)
        {
            statusCode ??= 500;

            var problemDetails = new ProblemDetails
            {
                Status = statusCode,
                Title = title,
                Type = type,
                Detail = detail,
                Instance = instance
            };

            ApplyProblemDetailsDefaults(httpContext, problemDetails, statusCode.Value);

            return problemDetails;
        }

        /// <inheritdoc/>
        public override ValidationProblemDetails CreateValidationProblemDetails(
            HttpContext httpContext,
            ModelStateDictionary modelStateDictionary,
            int? statusCode = null,
            string title = null,
            string type = null,
            string detail = null,
            string instance = null)
        {
            if (modelStateDictionary == null)
            {
                throw new ArgumentNullException(nameof(modelStateDictionary));
            }

            statusCode ??= 400;

            var errors = modelStateDictionary
                .Where(kvp => kvp.Value.ValidationState == ModelValidationState.Invalid)
                .ToDictionary(
                kvp => CreateKey(kvp.Key),
                kvp => kvp.Value.Errors.Select(x => x.ErrorMessage).ToArray()
            );

            var problemDetails = new ValidationProblemDetails(errors)
            {
                Status = statusCode,
                Type = type,
                Detail = detail,
                Instance = instance,
            };

            if (title != null)
            {
                // For validation problem details, don't overwrite the default title with null.
                problemDetails.Title = title;
            }

            ApplyProblemDetailsDefaults(httpContext, problemDetails, statusCode.Value);

            return problemDetails;
        }

        private void ApplyProblemDetailsDefaults(HttpContext httpContext, ProblemDetails problemDetails, int statusCode)
        {
            problemDetails.Status ??= statusCode;

            if (_apiBehaviorOptions.ClientErrorMapping.TryGetValue(statusCode, out var clientErrorData))
            {
                problemDetails.Title ??= clientErrorData.Title;
                problemDetails.Type ??= clientErrorData.Link;
            }

            var traceId = Activity.Current?.Id ?? httpContext?.TraceIdentifier;
            if (traceId != null)
            {
                problemDetails.Extensions["traceId"] = traceId;
            }

            AddSelfLink(problemDetails);
        }

        private string CreateKey(string key)
        {
            var keyWithoutSinglePrefix = RemovePrefix(key, nameof(SingleObjectOrCollection<object>.SingleObject));
            var keyWithoutCollectionPrefix = RemovePrefix(keyWithoutSinglePrefix, nameof(SingleObjectOrCollection<object>.Collection));
            var casedKey = _jsonOptions?.JsonSerializerOptions?.PropertyNamingPolicy?.ConvertName(keyWithoutCollectionPrefix) ?? keyWithoutCollectionPrefix;

            return casedKey;
        }

        private static string RemovePrefix(string key, string prefix)
        {
            if (!key.StartsWith(prefix))
                return key;

            if (key == prefix)
                return "";

            // If the prefix ends with a ., remove that too.
            // Keep it otherwise, like in array[42] where just the "array" and not the "[" should be removed.
            var prefixWithDotLength = prefix.Length + (key[prefix.Length] == '.' ? 1 : 0);
            var keyWithoutPrefix = key[prefixWithDotLength..];

            return keyWithoutPrefix;
        }

        private void AddSelfLink(ProblemDetails problemDetails)
        {
            string path = _linkGenerator.GetUriByAction(_actionContextAccessor.ActionContext.HttpContext);
            QueryString queryString = _actionContextAccessor.ActionContext.HttpContext.Request.QueryString;
            var link = new Link { Name = Constants.SelfLinkName, Href = path + queryString };

            problemDetails.Extensions["_links"] = new Dictionary<string, ICollection<Link>>
            {
                {
                    Constants.SelfLinkName,
                    new List<Link>
                    {
                        link
                    }
                }
            };
        }
    }
}
