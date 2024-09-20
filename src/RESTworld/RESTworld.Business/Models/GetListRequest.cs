using RESTworld.Business.Models.Abstractions;
using System;
using System.Linq;

namespace RESTworld.Business.Models;


/// <summary>
/// A request for a list of records.
/// </summary>
/// <typeparam name="TEntity">The type of the entity in the database.</typeparam>
/// <typeparam name="TDto">The type of the DTO used to query the database.</typeparam>
public record GetListRequest<TDto, TEntity> : IGetListRequest<TDto, TEntity>
{
    /// <summary>
    /// Creates a new instance of the <see cref="GetListRequest{TDto, TEntity}"/> class with the given filter.
    /// </summary>
    /// <param name="filter">The filter which should be applied to the request.</param>
    public GetListRequest(Func<IQueryable<TEntity>, IQueryable<TDto>> filter)
    {
        Filter = filter ?? throw new ArgumentNullException(nameof(filter));
        CalculateTotalCount = false;
        FilterForTotalCount = null;
    }

    /// <summary>
    /// Creates a new instance of the <see cref="GetListRequest{TDto, TEntity}"/> class with the given filters.
    /// </summary>
    /// <param name="filter">The filter which should be applied to the request.</param>
    /// <param name="filterForTotalCount">The filter for the total count which should be applied to the request.</param>
    public GetListRequest(Func<IQueryable<TEntity>, IQueryable<TDto>> filter, Func<IQueryable<TEntity>, IQueryable<TDto>> filterForTotalCount)
    {
        Filter = filter ?? throw new ArgumentNullException(nameof(filter));
        CalculateTotalCount = true;
        FilterForTotalCount = filterForTotalCount ?? throw new ArgumentNullException(nameof(filterForTotalCount));
    }

    /// <inheritdoc/>
    public Func<IQueryable<TEntity>, IQueryable<TDto>> Filter { get; }
    /// <inheritdoc/>
    public bool CalculateTotalCount { get; }
    /// <inheritdoc/>
    public Func<IQueryable<TEntity>, IQueryable<TDto>>? FilterForTotalCount { get; }
}
