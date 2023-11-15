using AutoFixture;
using RESTworld.Common.Dtos;
using System.Linq;

namespace RESTworld.AspNetCore.AutoFixture.Customizations;

/// <summary>
/// Customizes the Timestamp property of <see cref="ConcurrentDtoBase"/> to always contain 8 bytes.
/// </summary>
/// <seealso cref="ICustomization" />
public class DtoBaseCustomization : ICustomization
{
    /// <inheritdoc/>
    public void Customize(IFixture fixture)
    {
        fixture
            .CustomizeProperties<ConcurrentDtoBase>(composer =>
                composer
                    .With(d => d.Timestamp, () => fixture.CreateMany<byte>(8).ToArray()));
    }
}