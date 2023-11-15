using AutoFixture.Kernel;
using System;

namespace AutoFixture;

/// <summary>
/// Specification that checks that request type is assignable to the specified type.
/// </summary>
public class IsAssignableToTypeSpecification : IRequestSpecification
{
    private readonly Type _expectedType;

    /// <inheritdoc/>
    public IsAssignableToTypeSpecification(Type expectedType)
    {
        _expectedType = expectedType ?? throw new ArgumentNullException(nameof(expectedType));
    }

    /// <inheritdoc/>
    public bool IsSatisfiedBy(object request) => request is Type typeRequest && _expectedType.IsAssignableFrom(typeRequest);
}