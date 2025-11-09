using HAL.AspNetCore.Forms.Abstractions;
using HAL.Common;
using HAL.Common.Forms;
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;
using System.Net.Http;
using System.Reflection;
using System.Threading.Tasks;

namespace ExampleBlog.HalFormsCustomizations;

/// <summary>
/// OData needs enum values in the Pascal-Case form with single quotes around them.
/// By default, the forms generation returns these in camel-case without quotes.
/// This customization fixes that.
/// This customization is only applied to the Search form of Get-List endpoints.
/// </summary>
public class ODataEnumFix : HAL.AspNetCore.Forms.Customization.IFormsResourceGenerationCustomization
{
    public bool Exclusive => false;

    public int Order => int.MaxValue;

    public bool AppliesTo<TDto>(FormsResource formsResource, TDto value, HttpMethod method, string title, string contentType, string action, string? controller, object? routeValues)
    {
        return title == "Search" && contentType == "application/x-www-form-urlencoded" && formsResource.Embedded is not null;
    }

    private static bool TryGetItemType(FormsResource formsResource, [NotNullWhen(true)] out Type? type)
    {
        type = null;
        if (formsResource.Embedded is null || !formsResource.Embedded.TryGetValue("items", out var items))
            return false;

        if (items is not IEnumerable<Resource> itemResources)
            return false;

        var firstResource = itemResources.FirstOrDefault();
        if (firstResource is null)
            return false;

        var state = firstResource.CastState<object?>().State;
        if (state is null)
            return false;

        type = state.GetType();
        return true;
    }

    public ValueTask ApplyAsync<TDto>(FormsResource formsResource, TDto value, HttpMethod method, string title, string contentType, string action, string? controller, object? routeValues, IFormFactory formFactory)
    {
        if (!TryGetItemType(formsResource, out var dtoType))
            return ValueTask.CompletedTask;

        foreach (var (name, template) in formsResource.Templates)
        {
            if (name == "Search" && template.ContentType == "application/x-www-form-urlencoded" && template.Properties is not null)
            {
                foreach (var property in template.Properties)
                {
                    if (property.Options is Options<object?> options && options.Inline is not null && options.Inline.Count > 0 && options.Link is null)
                    {
                        var propertyType = dtoType.GetProperty(property.Name, BindingFlags.Public | BindingFlags.Instance | BindingFlags.IgnoreCase)?.PropertyType;
                        if (propertyType is not null && propertyType.IsEnum)
                        {
                            var enumNames = propertyType.GetEnumNames();
                            var newInline = options.Inline
                                .Select(option =>
                                {
                                    var newValue = option.Value?.ToString();
                                    if (newValue is not null && Enum.TryParse(propertyType, newValue, true, out var enumValue))
                                    {
                                        newValue = $"'{enumValue}'";
                                        return new OptionsItem<object?>(option.Prompt, newValue);
                                    }

                                    return option;
                                })
                                .ToList();

                            property.Options = new Options<object?>(newInline)
                            {
                                MaxItems = options.MaxItems,
                                MinItems = options.MinItems,
                                PromptField = options.PromptField,
                                ValueField = options.ValueField
                            };

                            if (options.SelectedValues is not null)
                            {
                                var newSelectedValues = options.SelectedValues
                                    .Select(selectedValue =>
                                    {
                                        var newValue = selectedValue?.ToString();
                                        if (newValue is not null && Enum.TryParse(propertyType, newValue, true, out var enumValue))
                                        {
                                            return $"'{enumValue}'";
                                        }

                                        return selectedValue;
                                    })
                                    .ToList();

                                property.Options.SelectedValues = newSelectedValues;
                            }
                        }
                    }
                }
            }
        }

        return ValueTask.CompletedTask;
    }
}
