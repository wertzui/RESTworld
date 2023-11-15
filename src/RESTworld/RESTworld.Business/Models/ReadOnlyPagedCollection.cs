using RESTworld.Business.Models.Abstractions;
using System.Collections.Generic;

namespace RESTworld.Business.Models;

/// <inheritdoc/>
public record ReadOnlyPagedCollection<T>(IReadOnlyCollection<T> Items, long? TotalCount)
    : IReadOnlyPagedCollection<T>
{
}