using HAL.AspNetCore.Controllers;
using Microsoft.AspNetCore.Mvc;
using Microsoft.AspNetCore.Mvc.ApiExplorer;
using Microsoft.OpenApi.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;
using System.Text.Json;

namespace RESTworld.AspNetCore.Swagger;

/// <summary>
/// This filter will add the correct versioned Media Types to Swagger, because Swagger will not use them if the controller has a <see cref="ProducesAttribute"/>.
/// The ProducesAttribute is present on <see cref="HalControllerBase"/> so we need to work around this Swagger issue.
/// </summary>
/// <seealso cref="Swashbuckle.AspNetCore.SwaggerGen.IOperationFilter" />
/// <remarks>REF: https://github.com/dotnet/aspnet-api-versioning/blob/e16b579d240574053ebd8e7ee38c8686beaee174/samples/aspnetcore/SwaggerSample/SwaggerDefaultValues.cs</remarks>
public class SwaggerVersioningOperationFilter : IOperationFilter
{
    private readonly string _versionparameterName;

    /// <summary>
    /// Initializes a new instance of the <see cref="SwaggerVersioningOperationFilter"/> class.
    /// </summary>
    /// <param name="versionparameterName">Name of the versionparameter.</param>
    public SwaggerVersioningOperationFilter(string versionparameterName)
    {
        _versionparameterName = versionparameterName;
    }

    /// <inheritdoc/>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var apiDescription = context.ApiDescription;

        operation.Deprecated |= apiDescription.IsDeprecated();

        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/1752#issue-663991077
        foreach (var responseType in context.ApiDescription.SupportedResponseTypes)
        {
            // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/blob/b7cf75e7905050305b115dd96640ddd6e74c7ac9/src/Swashbuckle.AspNetCore.SwaggerGen/SwaggerGenerator/SwaggerGenerator.cs#L383-L387
            var responseKey = responseType.IsDefaultResponse ? "default" : responseType.StatusCode.ToString();
            var response = operation.Responses[responseKey];

            if (_versionparameterName is null)
                continue;

            foreach (var content in response.Content.ToList())
            {
                var contentType = content.Key;
                var contentTypeWithVersion = $"{contentType}; v=";
                foreach (var apiResponseFormat in responseType.ApiResponseFormats)
                {
                    if (apiResponseFormat.MediaType.StartsWith(contentTypeWithVersion))
                    {
                        response.Content.Remove(contentType);
                        response.Content[apiResponseFormat.MediaType] = content.Value;
                    }
                }
            }
        }

        if (operation.Parameters == null)
        {
            return;
        }

        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/issues/412
        // REF: https://github.com/domaindrivendev/Swashbuckle.AspNetCore/pull/413
        foreach (var parameter in operation.Parameters)
        {
            var description = apiDescription.ParameterDescriptions.First(p => p.Name == parameter.Name);

            if (parameter.Description == null)
            {
                parameter.Description = description.ModelMetadata?.Description;
            }

            if (parameter.Schema.Default == null && description.DefaultValue != null)
            {
                // REF: https://github.com/Microsoft/aspnet-api-versioning/issues/429#issuecomment-605402330
                var modelType = description.ModelMetadata?.ModelType;
                if (modelType is not null)
                {
                    var json = JsonSerializer.Serialize(description.DefaultValue, modelType);
                    parameter.Schema.Default = OpenApiAnyFactory.CreateFromJson(json);
                }
            }

            parameter.Required |= description.IsRequired;
        }
    }
}