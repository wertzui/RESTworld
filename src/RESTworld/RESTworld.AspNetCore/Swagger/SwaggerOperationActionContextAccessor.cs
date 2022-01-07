using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.Infrastructure;
using Microsoft.AspNetCore.Routing;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;

namespace RESTworld.AspNetCore.Swagger
{
    /// <summary>
    /// This class is used to provide an <see cref="IActionContextAccessor"/> inside the <see cref="SwaggerExampleOperationFilter"/>.
    /// </summary>
    /// <seealso cref="IActionContextAccessor" />
    public class SwaggerOperationActionContextAccessor : IActionContextAccessor
    {
        private readonly ActionContext _actionContext;

        /// <summary>
        /// Initializes a new instance of the <see cref="SwaggerOperationActionContextAccessor"/> class.
        /// </summary>
        /// <param name="operationFilterContext">The operation filter context.</param>
        /// <param name="httpContextAccessor">The HTTP context accessor.</param>
        public SwaggerOperationActionContextAccessor(OperationFilterContext operationFilterContext, IHttpContextAccessor httpContextAccessor)
        {
            _actionContext = new ActionContext();
            _actionContext.ActionDescriptor = operationFilterContext.ApiDescription.ActionDescriptor;
            if (httpContextAccessor.HttpContext is not null)
            {
                _actionContext.HttpContext = httpContextAccessor.HttpContext;
                _actionContext.RouteData = _actionContext.HttpContext.GetRouteData();
            }
        }

        /// <inheritdoc/>
        public ActionContext ActionContext { get => _actionContext; set => throw new NotSupportedException(); }
    }
}