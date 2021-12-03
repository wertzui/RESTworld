using AutoFixture.Kernel;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoFixture
{
    /// <summary>
    /// Allows easy customization of how properties are assigned during creation.
    /// In contrast to the inbuild features of Auto Fixture, this works with base classes.
    /// The syntax is <code>fixture.CustomizeProperties&lt;T&gt;(c => c.With(d => d.Property, Value));</code>
    /// </summary>
    public class PropertiesCustomizer<T> : ICustomization
    {
        private List<ISpecimenBuilder> _builders = new();

        /// <inheritdoc/>
        public void Customize(IFixture fixture)
        {
            fixture.Customizations.Insert(0, new CompositeSpecimenBuilder(_builders));
        }

        /// <summary>
        /// Configures how the property will be assigned during object creation.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyPicker">The property picker.</param>
        /// <param name="builder">The builder.</param>
        /// <returns></returns>
        public PropertiesCustomizer<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, ISpecimenBuilder builder)
        {
            var equalRequestSpecification = new EqualRequestSpecification(
                propertyPicker.GetWritableMember().Member,
                new MemberInfoEqualityComparer());

            _builders.Add(
                new FilteringSpecimenBuilder(
                    builder,
                    equalRequestSpecification));

            return this;
        }

        /// <summary>
        /// Configures how the property will be assigned during object creation.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyPicker">The property picker.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        public PropertiesCustomizer<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, Func<object, ISpecimenContext, TProperty> valueFactory)
            => With(propertyPicker, new FactoryBuilder<TProperty>(valueFactory));

        /// <summary>
        /// Configures how the property will be assigned during object creation.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyPicker">The property picker.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        public PropertiesCustomizer<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, Func<TProperty> valueFactory)
            => With(propertyPicker, new FactoryBuilder<TProperty>(valueFactory));

        /// <summary>
        /// Configures how the property will be assigned during object creation.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyPicker">The property picker.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        public PropertiesCustomizer<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, Func<object, TProperty> valueFactory)
            => With(propertyPicker, new FactoryBuilder<TProperty>(valueFactory));

        /// <summary>
        /// Configures how the property will be assigned during object creation.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyPicker">The property picker.</param>
        /// <param name="valueFactory">The value factory.</param>
        /// <returns></returns>
        public PropertiesCustomizer<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, Func<ISpecimenContext, TProperty> valueFactory)
            => With(propertyPicker, new FactoryBuilder<TProperty>(valueFactory));

        /// <summary>
        /// Configures how the property will be assigned during object creation.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyPicker">The property picker.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public PropertiesCustomizer<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, TProperty value)
            => With(propertyPicker, new FixedBuilder(value));
    }
}