using System;

namespace RESTworld.Business.Models.Abstractions;

/// <summary>
/// A request for a list of historical records.
/// </summary>
/// <typeparam name="TEntity">The type of the entity in the database.</typeparam>
/// <typeparam name="TGetFullDto">The type of the DTO used to query the database.</typeparam>
public interface IGetHistoryRequest<TGetFullDto, TEntity> : IGetListRequest<TGetFullDto, TEntity>
{
    /// <summary>
    /// The inclusive lower bound of the time range.
    /// </summary>
    DateTimeOffset ValidFrom { get; }

    /// <summary>
    /// The exclusive upper bound of the time range.
    /// </summary>
    DateTimeOffset ValidTo { get; }
}