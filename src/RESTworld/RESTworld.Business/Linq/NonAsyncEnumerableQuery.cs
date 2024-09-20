using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Linq.Expressions;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Business.Linq;

/// <summary>
/// Wraps an <see cref="IAsyncQueryable{T}"/> to support both synchronous and asynchronous query capabilities.
/// </summary>
/// <remarks>Use <see cref="AsyncQueryableExtensions.AsQueryable{T}(IAsyncQueryable{T})"/> instead of instantiating this directly.</remarks>
/// <typeparam name="T">The type of the data in the data source.</typeparam>
public class NonAsyncEnumerableQuery<T> : IQueryable<T>, IAsyncQueryable<T>
{
    private readonly IAsyncQueryable<T> _source;

    /// <summary>
    /// Initializes a new instance of the <see cref="NonAsyncEnumerableQuery{T}"/> class.
    /// </summary>
    /// <remarks>Use <see cref="AsyncQueryableExtensions.AsQueryable{T}(IAsyncQueryable{T})"/> instead of instantiating this directly.</remarks>
    /// <param name="source">The source of the data.</param>
    public NonAsyncEnumerableQuery(IAsyncQueryable<T> source)
    {
        _source = source;
    }

    /// <inheritdoc/>
    public Type ElementType => _source.ElementType;

    /// <inheritdoc/>
    public Expression Expression => _source.Expression;

    /// <inheritdoc/>
    public IQueryProvider Provider => (IQueryProvider)_source.Provider;

    IAsyncQueryProvider IAsyncQueryable.Provider => _source.Provider;

    /// <inheritdoc/>
    public IAsyncEnumerator<T> GetAsyncEnumerator(CancellationToken cancellationToken = default) => _source.GetAsyncEnumerator(cancellationToken);

    /// <inheritdoc/>
    public IEnumerator<T> GetEnumerator() => _source.ToBlockingEnumerable().GetEnumerator();

    /// <inheritdoc/>
    IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
}