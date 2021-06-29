using RESTworld.Business.Abstractions;
using System;
using System.Linq;

namespace RESTworld.Business
{
    public record GetListRequest<TEntity> : IGetListRequest<TEntity>
    {
        public GetListRequest(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
        {
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
            CalculateTotalCount = false;
            FilterForTotalCount = null;
        }

        public GetListRequest(Func<IQueryable<TEntity>, IQueryable<TEntity>> filter, Func<IQueryable<TEntity>, IQueryable<TEntity>> filterForTotalCount)
        {
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
            CalculateTotalCount = true;
            FilterForTotalCount = filterForTotalCount ?? throw new ArgumentNullException(nameof(filterForTotalCount));
        }

        public Func<IQueryable<TEntity>, IQueryable<TEntity>> Filter { get; }
        public bool CalculateTotalCount { get; }
        public Func<IQueryable<TEntity>, IQueryable<TEntity>> FilterForTotalCount { get; }
    }
}
