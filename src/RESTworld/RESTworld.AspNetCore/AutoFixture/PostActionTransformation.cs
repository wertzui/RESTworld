using AutoFixture.Kernel;
using System;
using System.Collections.Generic;
using System.Linq.Expressions;

namespace AutoFixture
{
    /// <summary>
    /// Allows easy customization of how properties are assigned after creation.
    /// In contrast to the inbuild features of Auto Fixture, this works with base classes.
    /// The syntax is <code>fixture.CustomizePostActions&lt;T&gt;(c => c.With(d => d.Property, Value));</code>
    /// </summary>
    public class PostActionTransformation<T> : ISpecimenBuilderTransformation
    {
        private readonly ICollection<ISpecimenCommand> _commands = new List<ISpecimenCommand>();

        /// <inheritdoc/>
        public ISpecimenBuilderNode Transform(ISpecimenBuilder builder)
        {
            return new Postprocessor(
                builder,
                new CompositeSpecimenCommand(_commands),
                new IsAssignableToTypeSpecification(typeof(T)));
        }

        /// <summary>
        /// Configures how the property will be assigned after the object has been created.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyPicker">The property to be assigned.</param>
        /// <param name="value">The value.</param>
        /// <returns></returns>
        public PostActionTransformation<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, TProperty value)
        {
            _commands.Add(new BindingCommand<T, TProperty>(propertyPicker, value));

            return this;
        }

        /// <summary>
        /// Configures how the property will be assigned after the object has been created.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyPicker">The property to be assigned.</param>
        /// <param name="valueCreator">The value creator.</param>
        /// <returns></returns>
        public PostActionTransformation<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, Func<TProperty> valueCreator)
        {
            _commands.Add(new BindingCommand<T, TProperty>(propertyPicker, _ => valueCreator()));

            return this;
        }

        /// <summary>
        /// Configures how the property will be assigned after the object has been created.
        /// </summary>
        /// <typeparam name="TProperty">The type of the property.</typeparam>
        /// <param name="propertyPicker">The property to be assigned.</param>
        /// <param name="valueCreator">The value creator.</param>
        /// <returns></returns>
        public PostActionTransformation<T> With<TProperty>(Expression<Func<T, TProperty>> propertyPicker, Func<ISpecimenContext, TProperty> valueCreator)
        {
            _commands.Add(new BindingCommand<T, TProperty>(propertyPicker, valueCreator));

            return this;
        }
    }
}