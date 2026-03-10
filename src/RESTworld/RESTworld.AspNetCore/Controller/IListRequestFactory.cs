using Microsoft.AspNetCore.OData.Query;
using RESTworld.Business.Models.Abstractions;
using System;
using System.Linq;

namespace RESTworld.AspNetCore.Controller;

/// <summary>
/// Creates <see cref="IGetListRequest{TTQueryDto, TGetListDto, TEntity}"/> and <see cref="IGetHistoryRequest{TQueryDto, TGetListDto, TEntity}"/>. This is used to convert the
/// ODataQueryOptions into a request which can be used to call the service.
/// </summary>
/// <typeparam name="TEntity">The type of the entity which is queried in the database.</typeparam>
/// <typeparam name="TQueryDto">The type of the DTO which is used for the query.</typeparam>
/// <typeparam name="TGetListDto">The type of the DTO which is returned for list requests.</typeparam>
/// <typeparam name="TGetFullDto">The type of the DTO which is returned for history requests.</typeparam>
public interface IListRequestFactory<TEntity, TQueryDto, TGetListDto, TGetFullDto>
    where TEntity : class
    where TQueryDto : class
    where TGetListDto : class
    where TGetFullDto : class
{
    /// <summary>
    /// Converts the <paramref name="oDataQueryOptions"/> into a <see cref="IGetHistoryRequest{TQueryDto, TGetListDto, TEntity}"/>.
    /// </summary>
    /// <param name="oDataQueryOptions">The query options to convert from.</param>
    /// <param name="validFrom">The valid from date.</param>
    /// <param name="validTo">The valid to date.</param>
    /// <param name="calculateTotalCount">
    /// A value indicating if the total count shall be retrieved from the database.
    /// </param>
    /// <returns>A request which can be used to call the service.</returns>
    IGetHistoryRequest<TEntity, TQueryDto, TGetFullDto> CreateHistoryRequest(ODataQueryOptions<TQueryDto> oDataQueryOptions, DateTimeOffset? validFrom, DateTimeOffset? validTo, bool calculateTotalCount);

    /// <summary>
    /// Converts the <paramref name="oDataQueryOptions"/> into a <see cref="IGetListRequest{TQueryDto, TGetListDto, TEntity}"/>.
    /// </summary>
    /// <param name="oDataQueryOptions">The query options to convert from.</param>
    /// <param name="calculateTotalCount">
    /// A value indicating if the total count shall be retrieved from the database.
    /// </param>
    /// <returns>A request which can be used to call the service.</returns>
    IGetListRequest<TEntity, TQueryDto, TGetListDto> CreateListRequest(ODataQueryOptions<TQueryDto> oDataQueryOptions, bool calculateTotalCount);

    /// <summary>
    /// Gets a query which only contains the filter and not $skip and $top.
    /// </summary>
    /// <param name="oDataQueryOptions">The query options to convert from.</param>
    /// <returns>
    /// A function which applies the $filter to the entity collection and returns a Dto collection.
    /// </returns>
    Func<IQueryable<TEntity>, IQueryable<TGetFullDto>> GetFilterOnlyHistoryQuery(ODataQueryOptions<TQueryDto> oDataQueryOptions);

    /// <summary>
    /// Gets a query which only contains the filter and not $skip and $top.
    /// </summary>
    /// <param name="oDataQueryOptions">The query options to convert from.</param>
    /// <returns>
    /// A function which applies the $filter to the entity collection and returns a Dto collection.
    /// </returns>
    Func<IQueryable<TEntity>, IQueryable<TGetListDto>> GetFilterOnlyListQuery(ODataQueryOptions<TQueryDto> oDataQueryOptions);
}