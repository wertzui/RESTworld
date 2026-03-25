using HAL.Common;
using HAL.Common.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using RESTworld.Common.Client;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Collections.Generic;

namespace RESTworld.AspNetCore.Swagger;

public class SwaggerResourceSchemaFilter : ISchemaFilter
{
    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema concrete)
            return;

        AllowAdditionalPropertiesOnResource(concrete, context);
        AddStateInformation(concrete, context);
        AddFormsResource(concrete, context);
    }

    private static void AddFormsResource(OpenApiSchema concrete, SchemaFilterContext context)
    {
        if (!context.Type.IsGenericType || context.Type.GetGenericTypeDefinition() != typeof(Resource<>))
            return;

        var stateType = context.Type.GetGenericArguments()[0];
        if (stateType == typeof(ProblemDetails) || stateType == typeof(ClientSettings))
            return;

        var schemaId = stateType.Name + "FormsResource";
        if (context.SchemaRepository.Schemas.ContainsKey(schemaId))
            return;

        var templateType = typeof(FormTemplate);
        var templateSchema = context.SchemaGenerator.GenerateSchema(templateType, context.SchemaRepository);

        // Get Single and Get List have different templates.
        var isPage = stateType == typeof(Page);

        var templateProperties = new Dictionary<string, IOpenApiSchema>();
        if (isPage)
        {
            templateProperties["Search"] = templateSchema;
            templateProperties["Edit"] = templateSchema;

        }
        else
        {
            templateProperties[Constants.DefaultFormTemplateName] = templateSchema;
        }

        var templatesSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                {
                    Constants.FormTemplatesPropertyName,
                    new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        AdditionalPropertiesAllowed = true,
                        Properties = templateProperties
                    }
                }
            }
        };

        var formsResourceSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            AllOf = [concrete, templatesSchema]
        };

        context.SchemaRepository.Schemas[schemaId] = formsResourceSchema;
    }

    private static void AllowAdditionalPropertiesOnResource(OpenApiSchema concrete, SchemaFilterContext context)
    {
        if (!context.Type.IsAssignableTo(typeof(Resource)))
            return;

        concrete.AdditionalPropertiesAllowed = true;
    }

    private static void AddStateInformation(OpenApiSchema concrete, SchemaFilterContext context)
    {
        if (!context.Type.IsGenericType)
            return;
        var genericType = context.Type.GetGenericTypeDefinition();
        if (genericType != typeof(Resource<>) && genericType != typeof(FormsResource<>))
            return;

        if (concrete.Properties is null)
            return;

        if (!concrete.Properties.TryGetValue("state", out var stateSchema))
            return;

        if (!context.SchemaRepository.TryLookupByType(typeof(Resource), out var resourceSchemaRefereence))
            return;

        // Make this a union type of Resource and TState
        concrete.AllOf = [resourceSchemaRefereence, stateSchema];
        concrete.Properties = null;

        // Special logic for pages which add an _embedded property which contains the items. We want to make sure that this is properly documented in the OpenAPI schema.
        var stateType = context.Type.GetGenericArguments()[0];
        if (stateType != typeof(Page))
            return;

        var itemsSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Array,
            Items = resourceSchemaRefereence
        };

        var embeddedSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>()
            {
                ["items"] = itemsSchema
            }
        };

        var pageSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>()
            {
                ["_embedded"] = embeddedSchema
            }
        };

        if (context.SchemaRepository.TryLookupByType(typeof(Link), out var linkSchemaReference))
        {
            var linkCollectionSchema = new OpenApiSchema
            {
                Type = JsonSchemaType.Array,
                Items = linkSchemaReference
            };

            var linksSchemas = new OpenApiSchema
            {
                Type = JsonSchemaType.Object,
                Properties = new Dictionary<string, IOpenApiSchema>()
                {
                    ["first"] = linkCollectionSchema,
                    ["previous"] = linkCollectionSchema,
                    ["next"] = linkCollectionSchema,
                    ["last"] = linkCollectionSchema
                }
            };

            pageSchema.Properties["_links"] = linksSchemas;
        }

        concrete.AllOf.Add(pageSchema);
    }
}
