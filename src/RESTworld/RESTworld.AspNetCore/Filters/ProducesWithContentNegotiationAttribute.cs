using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;
using System.Linq;

namespace RESTworld.AspNetCore.Filters
{
    /// <summary>
    /// Works just like <see cref="ProducesAttribute"/> but a little bit more relaxed.
    /// If the request Accept-header contains one of the specified content-types it is considered valid and used as the response content type.
    /// This is useful when you also wan to to allow "charset" or "versioning" information to be present in the header without having to specify every possible combination.
    /// </summary>
    public class ProducesWithContentNegotiationAttribute : ProducesAttribute
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="ProducesWithContentNegotiationAttribute"/> class.
        /// </summary>
        /// <param name="contentType">The allowed content type for a response.</param>
        /// <param name="additionalContentTypes">Additional allowed content types for a response.</param>
        public ProducesWithContentNegotiationAttribute(string contentType, params string[] additionalContentTypes)
            : base(contentType, additionalContentTypes)
        {

        }

        /// <inheritdoc/>
        public override void OnResultExecuting(ResultExecutingContext context)
        {
            base.OnResultExecuting(context);

            if (context.Result is ObjectResult objectResult &&
                context.HttpContext.Request.Headers.TryGetValue("Accept", out var accept) &&
                objectResult.ContentTypes.Any(c => accept.Any(a => a.Contains(c))))
            {
                objectResult.ContentTypes.Clear();
                objectResult.ContentTypes.Add(accept);
            }
        }
    }
}
