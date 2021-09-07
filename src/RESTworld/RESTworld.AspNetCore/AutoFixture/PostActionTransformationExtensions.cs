using AutoFixture.Kernel;
using System;

namespace AutoFixture
{
    /// <summary>
    /// Allows easy customization of how properties are assigned after creation.
    /// In contrast to the inbuild features of Auto Fixture, this works with base classes.
    /// The syntax is <code>fixture.CustomizePostActions&lt;T&gt;(c => c.With(d => d.Property, Value));</code>
    /// </summary>
    public static class PostActionTransformationExtensions
    {
        /// <summary>
        /// Allows easy customization of how properties are assigned after creation.
        /// In contrast to the inbuild features of Auto Fixture, this works with base classes.
        /// The syntax is <code>fixture.CustomizePostActions&lt;T&gt;(c => c.With(d => d.Property, Value));</code>
        /// </summary>
        public static IFixture CustomizePostActions<T>(
            this IFixture fixture,
            Func<PostActionTransformation<T>, ISpecimenBuilderTransformation> customizations)
        {
            fixture.Behaviors.Add(customizations.Invoke(new PostActionTransformation<T>()));

            return fixture;
        }
    }
}