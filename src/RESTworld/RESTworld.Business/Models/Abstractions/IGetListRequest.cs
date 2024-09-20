using System;
using System.Linq;

namespace RESTworld.Business.Models.Abstractions;

/// <summary>
/// A request for a list of records.
/// </summary>
/// <typeparam name="TEntity">The type of the entity in the database.</typeparam>
/// <typeparam name="TGetListDto">The type of the DTO used to query the database.</typeparam>
public interface IGetListRequest<TGetListDto, TEntity>
{
    /// <summary>
    /// Gets or sets a value indicating whether the total count should be calculated for list endpoints.
    /// Calculating the total count means a second round trip to the database.
    /// </summary>
    bool CalculateTotalCount { get; }

    /// <summary>
    /// A filter which is applied to the query and executed on the database.
    /// </summary>
    Func<IQueryable<TEntity>, IQueryable<TGetListDto>> Filter { get; }

    /// <summary>
    /// A filter which is applied to the query and executed on the database during the calculation of the total count.
    /// It must not have any calls to Top or Skip as this would result in a wrong total count.
    /// </summary>
    Func<IQueryable<TEntity>, IQueryable<TGetListDto>>? FilterForTotalCount { get; }
}
