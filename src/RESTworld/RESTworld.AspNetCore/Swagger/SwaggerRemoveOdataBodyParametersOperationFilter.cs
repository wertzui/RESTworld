using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using RESTworld.AspNetCore.Controller;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace RESTworld.AspNetCore.Swagger;

internal class SwaggerRemoveOdataBodyParametersOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        if (context.ApiDescription.ActionDescriptor is not ControllerActionDescriptor controllerActionDescriptor ||
            !controllerActionDescriptor.ControllerTypeInfo.IsGenericType ||
            controllerActionDescriptor.ControllerTypeInfo.GetGenericTypeDefinition() != typeof(CrudController<,,,,>))
            return;

        var bodyContent = operation.RequestBody?.Content;
        if (bodyContent is null)
            return;

        var nonJsonTypes = bodyContent.Keys.Where(k => k != "application/json" && !k.StartsWith("application/json; v=")).ToList();

        foreach (var nonJsonType in nonJsonTypes)
        {
            bodyContent.Remove(nonJsonType);
        }
    }
}
