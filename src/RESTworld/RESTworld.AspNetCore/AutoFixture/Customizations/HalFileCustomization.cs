using AutoFixture;
using HAL.Common.Binary;
using System;
using System.Text;

namespace RESTworld.AspNetCore.AutoFixture.Customizations;

/// <summary>
/// Customizes the creation of <see cref="HalFile"/> to always have a valid uri.
/// </summary>
/// <seealso cref="ICustomization" />
public class HalFileCustomization : ICustomization
{
    /// <inheritdoc/>
    public void Customize(IFixture fixture)
    {
        fixture
            .Customize<HalFile>(composer =>
                composer.FromFactory(() => new HalFile($"https://example.org/file{fixture.Create<int>()}")));
    }

    private static bool createBase64;
    private static HalFile Factory()
    {
        createBase64 = !createBase64;

        return createBase64 ?
            new HalFile($"https://example.org/file{Random.Shared.Next(1, 10)}") :
            new HalFile($"data:text/plain;base64,{CreateRandomBase64Text()}");
    }

    private static string CreateRandomBase64Text()
    {
        var text = $"Hello World! This is example number {Random.Shared.Next(1, 10)}.";
        var bytes = Encoding.UTF8.GetBytes(text);
        var base64 = Convert.ToBase64String(bytes);

        return base64;
    }
}