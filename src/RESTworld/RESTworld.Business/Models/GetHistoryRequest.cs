using RESTworld.Business.Models.Abstractions;
using System;
using System.Linq;

namespace RESTworld.Business.Models;

/// <summary>
/// A request for a list of historical records.
/// </summary>
/// <typeparam name="TEntity">The type of the entity in the database.</typeparam>
/// <typeparam name="TDto">The type of the DTO used to query the database.</typeparam>
public record GetHistoryRequest<TDto, TEntity> : GetListRequest<TDto, TEntity>, IGetHistoryRequest<TDto, TEntity>
{
    /// <summary>
    /// Creates a new instance of the <see cref="GetHistoryRequest{TDto, TEntity}"/> class with the given filters.
    /// </summary>
    /// <param name="filter">The filter which should be applied to the request.</param>
    /// <param name="validFrom">The valid from date.</param>
    /// <param name="validTo">The valid to date.</param>
    public GetHistoryRequest(Func<IQueryable<TEntity>, IQueryable<TDto>> filter, DateTimeOffset? validFrom, DateTimeOffset? validTo)
        : base(filter)
    {
        ValidFrom = validFrom ?? DateTimeOffset.MinValue;
        ValidTo = validTo ?? DateTimeOffset.MaxValue;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="GetHistoryRequest{TDto, TEntity}"/> class with the given filters.
    /// </summary>
    /// <param name="filter">The filter which should be applied to the request.</param>
    /// <param name="filterForTotalCount">The filter for the total count which should be applied to the request.</param>
    /// <param name="validFrom">The valid from date.</param>
    /// <param name="validTo">The valid to date.</param>
    public GetHistoryRequest(Func<IQueryable<TEntity>, IQueryable<TDto>> filter, Func<IQueryable<TEntity>, IQueryable<TDto>> filterForTotalCount, DateTimeOffset? validFrom, DateTimeOffset? validTo)
        : base(filter, filterForTotalCount)
    {
        ValidFrom = validFrom ?? DateTimeOffset.MinValue;
        ValidTo = validTo ?? DateTimeOffset.MaxValue;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="GetHistoryRequest{TDto, TEntity}"/> class based on the given list request.
    /// </summary>
    /// <param name="original">The original request containing just the filters.</param>
    /// <param name="validFrom">The valid from date.</param>
    /// <param name="validTo">The valid to date.</param>
    protected GetHistoryRequest(GetListRequest<TDto, TEntity> original, DateTimeOffset? validFrom, DateTimeOffset? validTo)
        : base(original)
    {
        ValidFrom = validFrom ?? DateTimeOffset.MinValue;
        ValidTo = validTo ?? DateTimeOffset.MaxValue;
    }

    /// <inheritdoc/>
    public DateTimeOffset ValidFrom { get; }

    /// <inheritdoc/>
    public DateTimeOffset ValidTo { get; }
}