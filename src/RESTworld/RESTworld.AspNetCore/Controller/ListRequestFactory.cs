using Microsoft.AspNetCore.OData.Query;
using RESTworld.Business.Mapping;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using System;
using System.Linq;
using System.Reflection;

namespace RESTworld.AspNetCore.Controller;

/// <summary>
/// Creates <see cref="IGetListRequest{TQueryDto, TGetListDto, TEntity}"/>.
/// This is used to convert the ODataQueryOptions into a request which can be used to call the service.
/// </summary>
/// <typeparam name="TEntity">The type of the entity which is queried in the database.</typeparam>
/// <typeparam name="TQueryDto">The type of the DTO which is used for the query.</typeparam>
/// <typeparam name="TGetListDto">The type of the DTO which is returned.</typeparam>
/// <typeparam name="TGetFullDto">The type of the DTO which is returned for history requests.</typeparam>
public class ListRequestFactory<TEntity, TQueryDto, TGetListDto, TGetFullDto> : IListRequestFactory<TEntity, TQueryDto, TGetListDto, TGetFullDto>
    where TEntity : class
    where TQueryDto : class
    where TGetListDto : class
    where TGetFullDto : class
{
    private static readonly PropertyInfo _skipProperty = typeof(ODataQueryOptions).GetProperty(nameof(ODataQueryOptions.Skip))!;
    private static readonly PropertyInfo _skipTokenProperty = typeof(ODataQueryOptions).GetProperty(nameof(ODataQueryOptions.SkipToken))!;
    private static readonly PropertyInfo _topProperty = typeof(ODataQueryOptions).GetProperty(nameof(ODataQueryOptions.Top))!;

    private readonly IReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto> _mapper;

    /// <summary>
    /// Creates a new instance of the <see cref="ListRequestFactory{TQueryDto, TGetListDto, TGetFullDto, TEntity}"/> class.
    /// </summary>
    public ListRequestFactory(IReadMapper<TEntity, TQueryDto, TGetListDto, TGetFullDto> mapper)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
    }

    /// <inheritdoc/>
    public IGetHistoryRequest<TEntity, TQueryDto, TGetFullDto> CreateHistoryRequest(ODataQueryOptions<TQueryDto> oDataQueryOptions, DateTimeOffset? validFrom, DateTimeOffset? validTo, bool calculateTotalCount)
    {
        ArgumentNullException.ThrowIfNull(oDataQueryOptions);

        var filter = GetQuery(oDataQueryOptions, _mapper.MapQueryToFullQueryable);

        if (calculateTotalCount)
        {
            var filterForTotalCount = GetFilterOnlyHistoryQuery(oDataQueryOptions);
            return new GetHistoryRequest<TEntity, TQueryDto, TGetFullDto>(filter, filterForTotalCount, validFrom, validTo);
        }
        else
        {
            return new GetHistoryRequest<TEntity, TQueryDto, TGetFullDto>(filter, validFrom, validTo);
        }
    }

    /// <inheritdoc/>
    public IGetListRequest<TEntity, TQueryDto, TGetListDto> CreateListRequest(ODataQueryOptions<TQueryDto> oDataQueryOptions, bool calculateTotalCount)
    {
        ArgumentNullException.ThrowIfNull(oDataQueryOptions);

        var filter = GetQuery(oDataQueryOptions, _mapper.MapQueryToListQueryable);

        if (calculateTotalCount)
        {
            var filterForTotalCount = GetFilterOnlyListQuery(oDataQueryOptions);
            return new GetListRequest<TEntity, TQueryDto, TGetListDto>(filter, filterForTotalCount);
        }
        else
        {
            return new GetListRequest<TEntity, TQueryDto, TGetListDto>(filter);
        }
    }

    /// <inheritdoc/>
    public Func<IQueryable<TEntity>, IQueryable<TGetListDto>> GetFilterOnlyListQuery(ODataQueryOptions<TQueryDto> oDataQueryOptions)
        => GetFilterOnlyQuery(oDataQueryOptions, _mapper.MapQueryToListQueryable);

    public Func<IQueryable<TEntity>, IQueryable<TGetFullDto>> GetFilterOnlyHistoryQuery(ODataQueryOptions<TQueryDto> oDataQueryOptions)
        => GetFilterOnlyQuery(oDataQueryOptions, _mapper.MapQueryToFullQueryable);

    private Func<IQueryable<TEntity>, IQueryable<TResult>> GetQuery<TResult>(ODataQueryOptions<TQueryDto> oDataQueryOptions, Func<IQueryable<TQueryDto>, IQueryable<TResult>> mapFromQueryToResult)
    {
        IQueryable<TResult> apply(IQueryable<TEntity> source) => ApplyODataQueryOptions(source, oDataQueryOptions, mapFromQueryToResult);

        return apply;
    }

    private Func<IQueryable<TEntity>, IQueryable<TResult>> GetFilterOnlyQuery<TResult>(ODataQueryOptions<TQueryDto> oDataQueryOptions, Func<IQueryable<TQueryDto>, IQueryable<TResult>> mapFromQueryToResult)
    {
        // The total count must be calculated without any given $skip and $top value.
        var totalCountOptions = CreateQueryOptionsWithoutPagination(oDataQueryOptions);
        return source => ApplyODataQueryOptions(source, totalCountOptions, mapFromQueryToResult);
    }

    private static ODataQueryOptions<TQueryDto> CreateQueryOptionsWithoutPagination(ODataQueryOptions<TQueryDto> oDataQueryOptions)
    {
        var totalCountOptions = new ODataQueryOptions<TQueryDto>(oDataQueryOptions.Context, oDataQueryOptions.Request);
        if (totalCountOptions.Skip != null)
            _skipProperty.SetValue(totalCountOptions, null);
        if (totalCountOptions.SkipToken != null)
            _skipTokenProperty.SetValue(totalCountOptions, null);
        if (totalCountOptions.Top != null)
            _topProperty.SetValue(totalCountOptions, null);
        return totalCountOptions;
    }

    private IQueryable<TResult> ApplyODataQueryOptions<TResult>(IQueryable<TEntity> source, ODataQueryOptions<TQueryDto> oDataQueryOptions, Func<IQueryable<TQueryDto>, IQueryable<TResult>> mapFromQueryToResult)
    {
        var mappedQuery = _mapper.MapEntityToQueryQueryable(source);
        var appliedQuery = oDataQueryOptions.ApplyTo(mappedQuery);
        if (appliedQuery is not IQueryable<TQueryDto> query)
            throw new InvalidOperationException($"The applied query must be of type {typeof(IQueryable<TQueryDto>).FullName}, but was {appliedQuery.GetType().FullName}.");
        var resultingQuery = mapFromQueryToResult(query);

        return resultingQuery;
    }
}