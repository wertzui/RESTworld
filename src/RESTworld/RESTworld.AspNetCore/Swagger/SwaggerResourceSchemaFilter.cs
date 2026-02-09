using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common;
using HAL.Common.Forms;
using Microsoft.AspNetCore.Mvc;
using Microsoft.OpenApi;
using RESTworld.Common.Client;
using Swashbuckle.AspNetCore.SwaggerGen;
using System;
using System.Collections.Generic;
using System.Reflection;

namespace RESTworld.AspNetCore.Swagger;

public class SwaggerResourceSchemaFilter : ISchemaFilter
{
    private readonly IFormTemplateFactory _formTemplateFactory;

    public SwaggerResourceSchemaFilter(IFormTemplateFactory formTemplateFactory)
    {
        _formTemplateFactory = formTemplateFactory ?? throw new ArgumentNullException(nameof(formTemplateFactory));
    }

    public void Apply(IOpenApiSchema schema, SchemaFilterContext context)
    {
        if (schema is not OpenApiSchema concrete)
            return;

        AllowAdditionalPropertiesOnResource(concrete, context);
        AddStateInformation(concrete, context);
        AddFormsResource(concrete, context);
    }

    private void AddFormsResource(OpenApiSchema concrete, SchemaFilterContext context)
    {
        if (!context.Type.IsGenericType || context.Type.GetGenericTypeDefinition() != typeof(Resource<>))
            return;

        if (!context.SchemaRepository.TryLookupByType(typeof(Resource), out var resourceSchemaRefereence))
            return;

        var stateType = context.Type.GetGenericArguments()[0];
        if (stateType == typeof(ProblemDetails) || stateType == typeof(ClientSettings))
            return;

        var schemaId = stateType.Name + "FormsResource";
        if (context.SchemaRepository.Schemas.ContainsKey(schemaId))
            return;

        var genericPropertyType = typeof(Property<>);

        var template = _formTemplateFactory.CreateTemplateForAsync(stateType, "").GetAwaiter().GetResult();

        var properties = new Dictionary<string, IOpenApiSchema>();

        foreach (var property in template.Properties)
        {
            var statePropertyType = stateType.GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase);
            if (statePropertyType == null)
                continue;

            var propertyType = genericPropertyType.MakeGenericType(statePropertyType.PropertyType);
            string propertySchemaId;
            // if nullable, use "Nullable" + "TypeName" as schema id to avoid conflicts with the non-nullable version of the same type
            if (statePropertyType.PropertyType.IsGenericType && statePropertyType.PropertyType.GetGenericTypeDefinition() == typeof(Nullable<>))
            {
                var underlyingType = Nullable.GetUnderlyingType(statePropertyType.PropertyType);
                propertySchemaId = underlyingType!.Name + "NullableProperty";
            }
            else
            {
                propertySchemaId = statePropertyType.PropertyType.Name + "Property";
            }

            IOpenApiSchema propertySchema;
            if (context.SchemaRepository.TryLookupByType(propertyType, out var propertySchemaReference))
            //if (context.SchemaRepository.Schemas.TryGetValue(propertySchemaId, out var propertySchema))
            {
                propertySchema = propertySchemaReference;
            }
            else
            {
                propertySchema = context.SchemaGenerator.GenerateSchema(propertyType, context.SchemaRepository);
                //context.SchemaRepository.RegisterType(propertyType, propertySchemaId);
                //context.SchemaRepository.Schemas.Add(propertySchemaId, propertySchema);
            }

            properties[property.Name] = propertySchema;

        }

        var templatesSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>
            {
                {"_templates", new OpenApiSchema
                    {
                        Type = JsonSchemaType.Object,
                        AdditionalPropertiesAllowed = true,
                        Properties = new Dictionary<string, IOpenApiSchema>()
                        {
                            { "default", new OpenApiSchema
                                {
                                    Type = JsonSchemaType.Object,
                                    Properties = new Dictionary<string, IOpenApiSchema>
                                    {
                                        { "method", new OpenApiSchema { Type = JsonSchemaType.String } },
                                        { "target", new OpenApiSchema { Type = JsonSchemaType.String } },
                                        { "properties", new OpenApiSchema
                                            {
                                                Type = JsonSchemaType.Object,
                                                AdditionalPropertiesAllowed = true,
                                                Properties = properties
                                            }
                                        }
                                    }
                                }
                             }
                        }
                    }
                 }
            }
        };

        var formsResourceSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            AllOf = [resourceSchemaRefereence, templatesSchema]
        };

        context.SchemaRepository.Schemas[schemaId] = formsResourceSchema;

        //var properties = template.Properties
        //    .Select(p =>
        //    new OpenApiSchema
        //    {
        //        Type = JsonSchemaType.Object,
        //        Properties = new Dictionary<string, IOpenApiSchema>
        //        {
        //            ["title"] = new OpenApiSchema
        //            {
        //                Type = JsonSchemaType.String,
        //                Example = new Microsoft.OpenApi.Any.OpenApiString(p.Title)
        //            },
        //            ["type"] = new OpenApiSchema
        //            {
        //                Type = JsonSchemaType.String,
        //                Example = new Microsoft.OpenApi.Any.OpenApiString(p.Type.ToString())
        //            },
        //            ["description"] = new OpenApiSchema
        //            {
        //                Type = JsonSchemaType.String,
        //                Example = new Microsoft.OpenApi.Any.OpenApiString(p.Description ?? "")
        //            },
        //            ["required"] = new OpenApiSchema
        //            {
        //                Type = JsonSchemaType.Boolean,
        //                Example = new Microsoft.OpenApi.Any.OpenApiBoolean(p.Required)
        //            },
        //        },

        //        Type = p.Type switch
        //        {
        //            PropertyType.Month => JsonSchemaType.Integer,
        //            PropertyType.Week => JsonSchemaType.Integer,
        //            PropertyType.Number => JsonSchemaType.Number,
        //            PropertyType.Range => JsonSchemaType.Array,
        //            PropertyType.Bool => JsonSchemaType.Boolean,
        //            PropertyType.Collection => JsonSchemaType.Array,
        //            PropertyType.Object => JsonSchemaType.Object,
        //            PropertyType.Percent => JsonSchemaType.Number,
        //            PropertyType.Currency => JsonSchemaType.Number,
        //            _ => JsonSchemaType.String,
        //        },

        //    })
        //    .ToList();
    }

    private static void AllowAdditionalPropertiesOnResource(OpenApiSchema concrete, SchemaFilterContext context)
    {
        if (!context.Type.IsAssignableTo(typeof(Resource)))
            return;

        concrete.AdditionalPropertiesAllowed = true;
    }

    private static void AddStateInformation(OpenApiSchema concrete, SchemaFilterContext context)
    {
        if (!context.Type.IsGenericType || context.Type.GetGenericTypeDefinition() != typeof(Resource<>))
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

        var objectWithEmbeddedItemsSchema = new OpenApiSchema
        {
            Type = JsonSchemaType.Object,
            Properties = new Dictionary<string, IOpenApiSchema>()
            {
                ["_embedded"] = embeddedSchema
            }
        };

        concrete.AllOf.Add(objectWithEmbeddedItemsSchema);
    }
}
