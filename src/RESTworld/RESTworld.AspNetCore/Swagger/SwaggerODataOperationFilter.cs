using Microsoft.AspNetCore.OData.Query;
using Microsoft.OpenApi;
using Swashbuckle.AspNetCore.SwaggerGen;
using System.Linq;

namespace RESTworld.AspNetCore.Swagger;

/// <summary>
/// Replaces all parameters which have the <see cref="ODataQueryOptions{T}"/> type with the OData parameters.
/// </summary>
public class SwaggerODataOperationFilter : IOperationFilter
{
    private static readonly OpenApiParameter[] _oDataParameters =
    [
        new OpenApiParameter
        {
            Name = "$filter",
            In = ParameterLocation.Query,
            Description = "Filter the results using OData syntax.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        },
        new OpenApiParameter
        {
            Name = "$orderby",
            In = ParameterLocation.Query,
            Description = "Order the results using OData syntax.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.String }
        },
        new OpenApiParameter
        {
            Name = "$top",
            In = ParameterLocation.Query,
            Description = "Limit the results to the first n results. This is used for paging.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.Integer }
        },
        new OpenApiParameter
        {
            Name = "$skip",
            In = ParameterLocation.Query,
            Description = "Skip the first n results. This is used for paging.",
            Schema = new OpenApiSchema { Type = JsonSchemaType.Integer }
        }
    ];

    /// <inheritdoc/>
    public void Apply(OpenApiOperation operation, OperationFilterContext context)
    {
        var oDataParameterNames = context.MethodInfo.GetParameters()
            .Where(p => p.ParameterType.IsGenericType && p.ParameterType.GetGenericTypeDefinition().IsAssignableTo(typeof(ODataQueryOptions<>)))
            .Select(p => p.Name)
            .ToHashSet();

        if (oDataParameterNames.Count == 0 || operation.Parameters is null)
            return;

        var oDataParameters = operation.Parameters
            .Where(p => oDataParameterNames.Contains(p.Name))
            .ToList();

        foreach (var parameter in oDataParameters)
        {
            operation.Parameters.Remove(parameter);
        }

        foreach (var parameter in _oDataParameters)
            operation.Parameters.Add(parameter);
    }
}
