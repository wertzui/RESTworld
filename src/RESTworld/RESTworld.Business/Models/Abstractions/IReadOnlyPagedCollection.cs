using System.Collections.Generic;

namespace RESTworld.Business.Models.Abstractions
{
    /// <summary>
    /// Defines a total count besides an <see cref="IReadOnlyCollection{T}"/>.
    /// </summary>
    /// <typeparam name="T"></typeparam>
    public interface IReadOnlyPagedCollection<T>
    {
        /// <summary>
        /// One page of the collection.
        /// </summary>
        IReadOnlyCollection<T> Items { get; }

        /// <summary>
        /// The total number of entries from the database, if the service hhas retrieved them.
        /// </summary>
        long? TotalCount { get; }
    }
}