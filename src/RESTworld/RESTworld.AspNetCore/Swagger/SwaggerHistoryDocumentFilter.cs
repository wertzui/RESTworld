using HAL.AspNetCore.Utils;
using Microsoft.AspNetCore.Mvc.Abstractions;
using Microsoft.AspNetCore.Mvc.Controllers;
using Microsoft.OpenApi;
using RESTworld.AspNetCore.Controller;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.ComponentModel.DataAnnotations;

namespace RESTworld.AspNetCore.Swagger;

/// <summary>
/// Filters out all history endpoints from the Swagger document if the DTO does not have a <see cref="HasHistoryAttribute"/>.
/// </summary>
public class SwaggerHistoryDocumentFilter : IDocumentFilter
{
    /// <inheritdoc/>
    public void Apply(OpenApiDocument swaggerDoc, DocumentFilterContext context)
    {
        foreach (var apiDescription in context.ApiDescriptions)
        {
            if (ShouldRemoveHistory(apiDescription.ActionDescriptor))
            {
                swaggerDoc.Paths.Remove($"/{apiDescription.RelativePath}");
            }
        }
    }

    private static bool ShouldRemoveHistory(ActionDescriptor actionDescriptor)
    {
        if (actionDescriptor is not ControllerActionDescriptor controllerActionDescriptor || !controllerActionDescriptor.ControllerTypeInfo.IsGenericType)
            return false;

        var indexOfGetFullDtoType = controllerActionDescriptor.ControllerTypeInfo.GetGenericTypeDefinition() == typeof(ReadController<,,>)
            ? RestControllerNameConventionAttribute.ReadControllerIndexOfFullDtoType
            : controllerActionDescriptor.ControllerTypeInfo.GetGenericTypeDefinition() == typeof(CrudController<,,,,>)
                ? RestControllerNameConventionAttribute.CrudControllerIndexOfFullDtoType
                : -1;

        if (indexOfGetFullDtoType == -1)
            return false;

        if (controllerActionDescriptor.ActionName != ActionHelper.StripAsyncSuffix(nameof(ReadController<,,>.GetHistoryAsync)))
            return false;

        var getFullDtoType = controllerActionDescriptor.ControllerTypeInfo.GenericTypeArguments[indexOfGetFullDtoType];

        var hasHistoryAttribute = getFullDtoType.GetCustomAttributes(typeof(HasHistoryAttribute), true).Length > 0;

        if (hasHistoryAttribute)
            return false;

        return true;
    }
}
