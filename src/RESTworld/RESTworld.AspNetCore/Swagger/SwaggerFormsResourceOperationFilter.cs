using HAL.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using RESTworld.Common.Client;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace RESTworld.AspNetCore.Swagger;

internal class SwaggerFormsResourceOperationFilter : IOperationFilter
{
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {

        if (operation.Responses is null)
            return;

        foreach (var pair in operation.Responses)
        {
            var statusCode = pair.Key;
            _ = int.TryParse(statusCode, out var statusCodeInt);

            var response = pair.Value;
            if (response.Content is null)
                continue;

            foreach (var (mediaType, responseSchema) in response.Content)
            {
                if (mediaType is Constants.MediaTypes.HalForms or Constants.MediaTypes.HalFormsPrs)
                {
                    var responseTypeFromApiDescription = context.ApiDescription.SupportedResponseTypes.FirstOrDefault(t => t.StatusCode == statusCodeInt);

                    if (!responseTypeFromApiDescription.Type.IsGenericType || responseTypeFromApiDescription.Type.GetGenericTypeDefinition() != typeof(Resource<>))
                        continue;

                    var stateType = responseTypeFromApiDescription.Type.GetGenericArguments()[0];
                    if (stateType == typeof(ProblemDetails) || stateType == typeof(ClientSettings))
                        continue;

                    var schemaId = stateType.Name + "FormsResource";
                    if (!context.SchemaRepository.Schemas.TryGetValue(schemaId, out var schemaReference))
                        continue;

                    responseSchema.Schema = schemaReference;
                }
            }
        }
    }
}
