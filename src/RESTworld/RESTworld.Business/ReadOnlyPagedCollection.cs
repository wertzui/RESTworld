using RESTworld.Business.Abstractions;
using System.Collections.Generic;

namespace RESTworld.Business
{
    /// <inheritdoc/>
    public record ReadOnlyPagedCollection<T>(IReadOnlyCollection<T> Items, long? TotalCount)
        : IReadOnlyPagedCollection<T>
    {
    }
}
