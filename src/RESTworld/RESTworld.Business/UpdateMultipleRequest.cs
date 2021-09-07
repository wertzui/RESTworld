using RESTworld.Business.Abstractions;
using System;
using System.Collections.Generic;
using System.Linq;

namespace RESTworld.Business
{
    /// <inheritdoc/>
    public record UpdateMultipleRequest<TDto, TEntity> : IUpdateMultipleRequest<TDto, TEntity>
    {
        /// <summary>
        /// Creates a new instance of the <see cref="UpdateMultipleRequest{TDto, TEntity}"/> class.
        /// </summary>
        /// <param name="dtos">The DTOs which are used to update the entities in the database.</param>
        /// <param name="filter">A filter which is applied to the query and executed on the database.</param>
        public UpdateMultipleRequest(IReadOnlyCollection<TDto> dtos, Func<IQueryable<TEntity>, IQueryable<TEntity>> filter)
        {
            Dtos = dtos ?? throw new ArgumentNullException(nameof(dtos));
            Filter = filter ?? throw new ArgumentNullException(nameof(filter));
        }

        /// <inheritdoc/>
        public Func<IQueryable<TEntity>, IQueryable<TEntity>> Filter { get; }

        /// <inheritdoc/>
        public IReadOnlyCollection<TDto> Dtos { get; }
    }
}