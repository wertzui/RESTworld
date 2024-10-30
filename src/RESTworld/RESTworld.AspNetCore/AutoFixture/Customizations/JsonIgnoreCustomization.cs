using AutoFixture;
using AutoFixture.Kernel;
using System;
using System.Reflection;
using System.Text.Json.Serialization;

namespace RESTworld.AspNetCore.AutoFixture.Customizations;

/// <summary>
/// Customizes the <see cref="JsonIgnoreAttribute"/> to omit properties with this attribute.
/// </summary>
public class JsonIgnoreCustomization : ICustomization
{
    /// <inheritdoc/>
    public void Customize(IFixture fixture)
    {
        fixture.Customizations.Add(new PropertyAttributeOmitter<JsonIgnoreAttribute>());
    }
}


/// <summary>
/// Omit properties with the specified attribute.
/// </summary>
/// <typeparam name="TAttribute">The type of the attribute</typeparam>
public class PropertyAttributeOmitter<TAttribute> : ISpecimenBuilder
    where TAttribute : Attribute
{
    /// <inheritdoc/>
    public object Create(object request, ISpecimenContext context)
    {
        if (request is PropertyInfo propInfo && propInfo is not null && propInfo.GetCustomAttribute<TAttribute>(true) is not null)
            return new OmitSpecimen();

        return new NoSpecimen();
    }
}
