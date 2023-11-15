using System;
using System.Collections.Generic;
using System.Linq;

namespace RESTworld.Business.Models.Abstractions;

/// <summary>
/// A request for an update of multiple records.
/// </summary>
/// <typeparam name="TDto">The type of the DTO in the request.</typeparam>
/// <typeparam name="TEntity">The type of the entity in the database.</typeparam>
public interface IUpdateMultipleRequest<TDto, TEntity>
{
    /// <summary>
    /// The DTOs which are used to update the entities in the database.
    /// </summary>
    IReadOnlyCollection<TDto> Dtos { get; }

    /// <summary>
    /// A filter which is applied to the query and executed on the database.
    /// </summary>
    Func<IQueryable<TEntity>, IQueryable<TEntity>> Filter { get; }
}