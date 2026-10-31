using Microsoft.AspNetCore.JsonPatch.SystemTextJson;
using Microsoft.OpenApi;
using RESTworld.Business.Models;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace RESTworld.AspNetCore.Swagger;

/// <summary>
/// Describes every <see cref="JsonPatchDocument{TCreate, TUpdate}"/> and <see cref="JsonPatchDocument{TModel}"/> as what they are on the wire:
/// an array of RFC 6902 operations, and not an object with the properties of the C# class.
/// </summary>
internal class SwaggerJsonPatchSchemaFilter : ISchemaFilter
{
    private static bool IsJsonPatchDocument(System.Type type)
        => type.IsGenericType && type.GetGenericTypeDefinition() is var definition
            && (definition == typeof(RESTworld.Business.Models.JsonPatchDocument<,>) || definition == typeof(Microsoft.AspNetCore.JsonPatch.SystemTextJson.JsonPatchDocument<>));

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema concrete || !IsJsonPatchDocument(context.Type))
            return;

        concrete.Type = JsonSchemaType.Array;
        concrete.Properties?.Clear();
        concrete.Required?.Clear();
        concrete.Items = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Required = new HashSet<string> { "op", "path" },
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                ["op"] = new OpenApiSchema { Type = JsonSchemaType.String, Enum = ["add", "remove", "replace", "move", "copy", "test"], Description = "The operation. 'add' with the path '/-' creates a new resource." },
                ["path"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = "'/-' to create a resource, otherwise '/{id}/{property}'." },
                ["from"] = new OpenApiSchema { Type = JsonSchemaType.String, Description = "Only for 'move' and 'copy'. Must point to the same resource as 'path'." },
                ["value"] = new OpenApiSchema { Description = "The value for 'add', 'replace' and 'test'." }
            }
        };
    }
}
