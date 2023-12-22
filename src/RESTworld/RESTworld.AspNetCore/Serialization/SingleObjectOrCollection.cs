using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Text.Json.Serialization;

namespace RESTworld.AspNetCore.Serialization;

/// <summary>
/// Holds either a single object or a collection.
/// </summary>
/// <typeparam name="T"></typeparam>
[JsonConverter(typeof(SingleObjectOrCollectionJsonConverterFactory))]
public record SingleObjectOrCollection<T>
{
    /// <summary>
    /// Creates a new instance of the <see cref="SingleObjectOrCollection{T}"/> class which contains a single object.
    /// </summary>
    /// <param name="singleObject">The object to hold.</param>
    public SingleObjectOrCollection(T singleObject)
    {
        SingleObject = singleObject;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="SingleObjectOrCollection{T}"/> class which contains a collection.
    /// </summary>
    /// <param name="collection">The collection to hold.</param>
    public SingleObjectOrCollection(IReadOnlyCollection<T> collection)
    {
        Collection = collection;
    }

    /// <summary>
    /// The object.
    /// </summary>
    public T? SingleObject { get; }

    /// <summary>
    /// The collection.
    /// </summary>
    public IReadOnlyCollection<T>? Collection { get; }

    /// <summary>
    /// Returns a value telling you if this instance holds a collection.
    /// </summary>
    [MemberNotNullWhen(true, nameof(Collection))]
    [MemberNotNullWhen(false, nameof(SingleObject))]
    public bool ContainsCollection => Collection != default;

    /// <summary>
    /// Returns a value telling you if this instance holds a single object.
    /// </summary>
    [MemberNotNullWhen(true, nameof(SingleObject))]
    [MemberNotNullWhen(false, nameof(Collection))]
    public bool ContainsSingleObject => !ContainsCollection;
}