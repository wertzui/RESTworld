using AutoMapper;
using System;
using System.Diagnostics.CodeAnalysis;
using System.Reflection;

namespace Automapper;

/// <summary>
/// Extensions for Automapper
/// </summary>
public static class AutomapperExtensions
{

    /// <summary>
    /// Adds the given <paramref name="memberOptions"/> to every property of the destination type.
    /// </summary>
    /// <typeparam name="TSource">The source type.</typeparam>
    /// <typeparam name="TDest">The destination type.</typeparam>
    /// <param name="expression">The mapping expression.</param>
    /// <param name="memberOptions">The member configuration options.</param>
    /// <returns>The mapping expression.</returns>
    /// <remarks>
    /// This method maps every member of the source type to the destination type.
    /// </remarks>
    /// <example>
    /// <code>
    /// CreateMap&lt;Source, Destination&gt;()
    ///     .ForEveryMember((dest, opt) =&gt; opt.Condition((src, dest, srcMember) =&gt; srcMember != null));
    /// </code>
    /// </example>
    public static IMappingExpression<TSource, TDest> ForEveryMember<TSource, [DynamicallyAccessedMembers(DynamicallyAccessedMemberTypes.PublicProperties | DynamicallyAccessedMemberTypes.NonPublicProperties)] TDest>(this IMappingExpression<TSource, TDest> expression, Action<IMemberConfigurationExpression<TSource, TDest, object>> memberOptions)
    {
        var type = typeof(TDest);
        var properties = type.GetRuntimeProperties();

        foreach (var property in properties)
        {
            expression.ForMember(property.Name, memberOptions);
        }

        return expression;
    }
}
