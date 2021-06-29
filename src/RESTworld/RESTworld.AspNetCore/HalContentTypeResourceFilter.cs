using HAL.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Filters;

namespace HAL.AspNetCore.Filters
{
    public class HalContentTypeResourceFilter : IResultFilter
    {
        public void OnResultExecuted(ResultExecutedContext context)
        {
        }

        public void OnResultExecuting(ResultExecutingContext context)
        {
            if (context.Result is ObjectResult objectResult && objectResult.Value is Resource)
                context.HttpContext.Response.Headers["Content-Type"] = "application/hal+json";
        }
    }
}
