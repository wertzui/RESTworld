using AutoFixture.Kernel;
using System;

namespace AutoFixture;

/// <summary>
/// This builder creates objects based on a factory method.
/// </summary>
/// <typeparam name="T"></typeparam>
/// <seealso cref="AutoFixture.Kernel.ISpecimenBuilder" />
public class FactoryBuilder<T> : ISpecimenBuilder
{
    private readonly Func<object, ISpecimenContext, T> _factory;

    /// <summary>
    /// Initializes a new instance of the <see cref="FactoryBuilder{T}"/> class.
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <exception cref="System.ArgumentNullException">factory</exception>
    public FactoryBuilder(Func<object, ISpecimenContext, T> factory)
    {
        _factory = factory ?? throw new ArgumentNullException(nameof(factory));
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FactoryBuilder{T}"/> class.
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <exception cref="System.ArgumentNullException">factory</exception>
    public FactoryBuilder(Func<T> factory)
        : this((_, __) => factory())
    {
        ArgumentNullException.ThrowIfNull(factory);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FactoryBuilder{T}"/> class.
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <exception cref="System.ArgumentNullException">factory</exception>
    public FactoryBuilder(Func<ISpecimenContext, T> factory)
        : this((_, context) => factory(context))
    {
        ArgumentNullException.ThrowIfNull(factory);
    }

    /// <summary>
    /// Initializes a new instance of the <see cref="FactoryBuilder{T}"/> class.
    /// </summary>
    /// <param name="factory">The factory.</param>
    /// <exception cref="System.ArgumentNullException">factory</exception>
    public FactoryBuilder(Func<object, T> factory)
        : this((request, _) => factory(request))
    {
        ArgumentNullException.ThrowIfNull(factory);
    }

    /// <inheritdoc/>
    public object? Create(object request, ISpecimenContext context) => _factory(request, context);
}