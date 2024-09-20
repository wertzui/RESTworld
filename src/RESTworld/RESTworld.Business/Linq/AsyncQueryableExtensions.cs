using RESTworld.Business.Linq;

namespace System.Linq;

/// <summary>
/// Contains extension methods for <see cref="IAsyncQueryable{T}"/>.
/// </summary>
public static class AsyncQueryableExtensions
{
    /// <summary>
    /// Converts an <see cref="IAsyncQueryable{T}"/> to an <see cref="IQueryable{T}"/>.
    /// </summary>
    /// <typeparam name="T">The type of the elements in the sequence.</typeparam>
    /// <param name="source">The source of the data to be wrapped.</param>
    /// <returns>
    /// The given <paramref name="source"/> as an <see cref="IQueryable{T}"/>.
    /// </returns>
    public static IQueryable<T> AsQueryable<T>(this IAsyncQueryable<T> source) => new NonAsyncEnumerableQuery<T>(source);
}
