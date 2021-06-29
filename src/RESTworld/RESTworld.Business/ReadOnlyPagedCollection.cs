using RESTworld.Business.Abstractions;
using System.Collections.Generic;

namespace RESTworld.Business
{
    public record ReadOnlyPagedCollection<T>(IReadOnlyCollection<T> Items, long? TotalCount)
        : IReadOnlyPagedCollection<T>
    {
    }
}
