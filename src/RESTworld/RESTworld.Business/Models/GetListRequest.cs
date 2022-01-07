using RESTworld.Business.Models.Abstractions;
using System;
using System.Linq;

namespace RESTworld.Business.Models
{
    /// <inheritdoc/>
    public record GetListRequest<TEntity> : IGetListRequest<TEntity>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="GetListRequest{TEntity}"/> class with the given filter.
        /// </summary>
        /// <param name="filter">The filter which should be applied to the request.</param>
        public GetListRequest(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
        {
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
            CalculateTotalCount = false;
            FilterForTotalCount = null;
        }

        /// <summary>
        /// Creates a new instance of the <see cref="GetListRequest{TEntity}"/> class with the given filters.
        /// </summary>
        /// <param name="filter">The filter which should be applied to the request.</param>
        /// <param name="filterForTotalCount">The filter for the total count which should be applied to the request.</param>
        public GetListRequest(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter, Func<IQueryable<TEntity>, IQueryable<TEntity>> filterForTotalCount)
        {
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
            CalculateTotalCount = true;
            FilterForTotalCount = filterForTotalCount ?? throw new ArgumentNullException(nameof(filterForTotalCount));
        }

        /// <inheritdoc/>
        public Func<IQueryable<TEntity>, IQueryable<TEntity>> Filter { get; }
        /// <inheritdoc/>
        public bool CalculateTotalCount { get; }
        /// <inheritdoc/>
        public Func<IQueryable<TEntity>, IQueryable<TEntity>>? FilterForTotalCount { get; }
    }
}