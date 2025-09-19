using AutoFixture.Kernel;
using System;
using System.Reflection;

namespace RESTworld.AspNetCore.AutoFixture.Customizations;

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
