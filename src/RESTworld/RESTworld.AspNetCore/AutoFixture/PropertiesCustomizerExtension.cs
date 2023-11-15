using System;

namespace AutoFixture;

/// <summary>
/// Allows easy customization of how properties are assigned during creation.
/// In contrast to the inbuild features of Auto Fixture, this works with base classes.
/// The syntax is <code>fixture.CustomizeProperties&lt;T&gt;(c => c.With(d => d.Property, Value));</code>
/// </summary>
public static class PropertiesCustomizerExtension
{
    /// <summary>
    /// Allows easy customization of how properties are assigned during creation.
    /// In contrast to the inbuild features of Auto Fixture, this works with base classes.
    /// The syntax is <code>fixture.CustomizeProperties&lt;T&gt;(c => c.With(d => d.Property, Value));</code>
    /// </summary>
    public static IFixture CustomizeProperties<T>(this IFixture fixture, Func<PropertiesCustomizer<T>, ICustomization> customization)
    {
        fixture.Customize(customization.Invoke(new PropertiesCustomizer<T>()));

        return fixture;
    }
}