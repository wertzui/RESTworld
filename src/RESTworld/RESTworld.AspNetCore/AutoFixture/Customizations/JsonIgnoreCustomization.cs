using AutoFixture;
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
