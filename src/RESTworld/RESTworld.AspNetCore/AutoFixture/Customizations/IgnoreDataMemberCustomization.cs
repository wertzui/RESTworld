using AutoFixture;
using System.Runtime.Serialization;

namespace RESTworld.AspNetCore.AutoFixture.Customizations;

/// <summary>
/// Customizes the <see cref="IgnoreDataMemberAttribute"/> to omit properties with this attribute.
/// </summary>
public class IgnoreDataMemberCustomization : ICustomization
{
    /// <inheritdoc/>
    public void Customize(IFixture fixture)
    {
        fixture.Customizations.Add(new PropertyAttributeOmitter<IgnoreDataMemberAttribute>());
    }
}