using System;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;
using System.Runtime.Serialization;
using System.Text.Json.Serialization;

namespace RESTworld.Business.Mapping;

/// <summary>
/// Builds a select projection expression that maps every property
/// onto itself, except properties decorated with the specified attribute types.
/// </summary>
public static class SelectExpressionBuilder
{
    /// <summary>
    /// Creates an <see cref="Expression{TDelegate}"/> of type <c>Func&lt;T, T&gt;</c>
    /// that projects every public instance property of <typeparamref name="T"/> onto itself,
    /// skipping any property decorated with <see cref="JsonIgnoreAttribute"/> or <see cref="IgnoreDataMemberAttribute"/>.
    /// </summary>
    /// <typeparam name="T">The type to build the projection for.</typeparam>
    /// <returns>An expression tree representing the projection.</returns>
    public static Expression<Func<T, T>> ExcludingSerializationIgnored<T>()
        => Excluding<T>(typeof(JsonIgnoreAttribute), typeof(IgnoreDataMemberAttribute));

    /// <summary>
    /// Creates an <see cref="Expression{TDelegate}"/> of type <c>Func&lt;T, T&gt;</c>
    /// that projects every public instance property of <typeparamref name="T"/> onto itself,
    /// skipping any property that has one of the specified <paramref name="attributeTypesToIgnore"/>.
    /// </summary>
    /// <typeparam name="T">The type to build the projection for.</typeparam>
    /// <param name="attributeTypesToIgnore">Attribute types whose decorated properties should be excluded from the projection.</param>
    /// <returns>An expression tree representing the projection.</returns>
    public static Expression<Func<T, T>> Excluding<T>(params IEnumerable<Type> attributeTypesToIgnore)
    {
        var type = typeof(T);
        var parameter = Expression.Parameter(type, "x");

        var properties = type
            .GetProperties(BindingFlags.Public | BindingFlags.Instance)
            .Where(p => p.CanRead && p.CanWrite)
            .Where(p => !attributeTypesToIgnore.Any(a => p.IsDefined(a, inherit: true)));

        var bindings = properties
            .Select(p => Expression.Bind(p, Expression.Property(parameter, p)));

        var body = Expression.MemberInit(Expression.New(type), bindings);

        return Expression.Lambda<Func<T, T>>(body, parameter);
    }
}
