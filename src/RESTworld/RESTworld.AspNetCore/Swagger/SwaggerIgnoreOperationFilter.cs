using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Reflection;

namespace RESTworld.AspNetCore.Swagger
{
    /// <summary>
    /// Removes all parameters which have the <see cref="SwaggerIgnoreAttribute"/> from the Open API document.
    /// </summary>
    public class SwaggerIgnoreOperationFilter : IOperationFilter
    {
        /// <inheritdoc/>
        public void Apply(OpenApiOperation operation, OperationFilterContext context)
        {
            var ignoredParameterNames = context.MethodInfo.GetParameters()
                .Where(p => p.GetCustomAttribute<SwaggerIgnoreAttribute>() is not null)
                .Select(p => p.Name)
                .ToHashSet();

            if (!ignoredParameterNames.Any())
                return;

            var parametersToIgnore = operation.Parameters
                .Where(p => ignoredParameterNames.Contains(p.Name))
                .ToList();

            foreach (var parameter in parametersToIgnore)
            {
                operation.Parameters.Remove(parameter);
            }
        }
    }
}