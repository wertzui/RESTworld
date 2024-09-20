using AutoMapper;
using AutoMapper.AspNet.OData;
using Microsoft.AspNetCore.OData.Query;
using Microsoft.Extensions.Caching.Memory;
using RESTworld.Business.Models;
using RESTworld.Business.Models.Abstractions;
using System;
using System.Linq;
using System.Linq.Expressions;
using System.Reflection;

namespace RESTworld.AspNetCore.Controller;

/// <summary>
/// Creates <see cref="IGetListRequest{TDto, TEntity}"/>.
/// This is used to convert the ODataQueryOptions into a request which can be used to call the service.
/// </summary>
public class ListRequestFactory : IListRequestFactory
{
    private static readonly MethodInfo _getQueryableMethod = GetQueryableMethod();
    //private static readonly Dictionary<(Type, Type), Delegate> _getQueryableMethodCache = new();
    private readonly IMapper _mapper;
    private readonly IMemoryCache _cache;

    /// <summary>
    /// Creates a new instance of the <see cref="ListRequestFactory"/> class.
    /// </summary>
    /// <param name="mapper">The mapper to convert the entities to DTOs and vice versa.</param>
    /// <param name="cache">The cache.</param>
    public ListRequestFactory(
        IMapper mapper,
        IMemoryCache cache)
    {
        _mapper = mapper ?? throw new ArgumentNullException(nameof(mapper));
        _cache = cache ?? throw new ArgumentNullException(nameof(cache));
    }

    /// <inheritdoc/>
    public IGetHistoryRequest<TDto, TEntity> CreateHistoryRequest<TDto, TEntity>(ODataQueryOptions<TDto> oDataQueryOptions, DateTimeOffset? validFrom, DateTimeOffset? validTo, bool calculateTotalCount)
        where TDto : class
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(oDataQueryOptions);

        Func<IQueryable<TEntity>, IQueryable<TDto>> filter = source => source.GetQuery(_mapper, oDataQueryOptions);

        if (calculateTotalCount)
        {
            var filterForTotalCount = GetFilterOnlyQuery<TDto, TEntity>(oDataQueryOptions);
            return new GetHistoryRequest<TDto, TEntity>(filter, filterForTotalCount, validFrom, validTo);
        }
        else
        {
            return new GetHistoryRequest<TDto, TEntity>(filter, validFrom, validTo);
        }
    }

    /// <inheritdoc/>
    public IGetListRequest<TDto, TEntity> CreateListRequest<TDto, TEntity>(ODataQueryOptions<TDto> oDataQueryOptions, bool calculateTotalCount)
        where TDto : class
        where TEntity : class
    {
        ArgumentNullException.ThrowIfNull(oDataQueryOptions);

        Func<IQueryable<TEntity>, IQueryable<TDto>> filter = source => source.GetQuery(_mapper, oDataQueryOptions);

        if (calculateTotalCount)
        {
            var filterForTotalCount = GetFilterOnlyQuery<TDto, TEntity>(oDataQueryOptions);
            return new GetListRequest<TDto, TEntity>(filter, filterForTotalCount);
        }
        else
        {
            return new GetListRequest<TDto, TEntity>(filter);
        }
    }

    /// <inheritdoc/>
    public Func<IQueryable<TEntity>, IQueryable<TDto>> GetFilterOnlyQuery<TDto, TEntity>(ODataQueryOptions<TDto> oDataQueryOptions, QuerySettings? querySettings = null)
        where TDto : class
    {
        Expression<Func<TDto, bool>> filter = oDataQueryOptions.ToFilterExpression((querySettings?.ODataSettings?.HandleNullPropagation).GetValueOrDefault(HandleNullPropagationOption.False), querySettings?.ODataSettings?.TimeZone, (querySettings?.ODataSettings?.EnableConstantParameterization).GetValueOrDefault(true));

        return query => GetQueryable(query, oDataQueryOptions, querySettings, filter);
    }

    private IQueryable<TModel> GetQueryable<TModel, TData>(
        IQueryable<TData> query,
        ODataQueryOptions<TModel> options,
        QuerySettings? querySettings,
        Expression<Func<TModel, bool>> filter)
        where TModel : class
    {
        var func = GetQueryableFunc<TModel, TData>();

        var queryable = func(query, _mapper, options, querySettings, filter);

        return queryable;
    }

    private Func<IQueryable<TData>, IMapper, ODataQueryOptions<TModel>, QuerySettings?, Expression<Func<TModel, bool>>, IQueryable<TModel>> GetQueryableFunc<TModel, TData>() where TModel : class
    {
        var key = (typeof(TModel), typeof(TData));

        if (!_cache.TryGetValue(key, out Func<IQueryable<TData>, IMapper, ODataQueryOptions<TModel>, QuerySettings?, Expression<Func<TModel, bool>>, IQueryable<TModel>>? func) || func is null)
        {
            var genericMethod = _getQueryableMethod.MakeGenericMethod(key.Item1, key.Item2);
            func = genericMethod.CreateDelegate<Func<IQueryable<TData>, IMapper, ODataQueryOptions<TModel>, QuerySettings?, Expression<Func<TModel, bool>>, IQueryable<TModel>>>();
            _cache.Set(key, func);
        }

        return func;
    }

    private static MethodInfo GetQueryableMethod()
    {
        var type = typeof(AutoMapper.AspNet.OData.QueryableExtensions);
        var method = type.GetMethod("GetQueryable", BindingFlags.Static | BindingFlags.NonPublic);
        if (method is null)
            throw new InvalidOperationException($"""Unable to create the Count expression for the query. Method "GetQueryable" not found on {type.Name}. The reason is probably a version incompatibility.""");
        if (!method.IsGenericMethod || method.GetGenericArguments().Length != 2)
            throw new InvalidOperationException($"""Unable to create the Count expression for the query. Method "GetQueryable" is not generic or has the wrong number of generic arguments on {type.Name}. The reason is probably a version incompatibility.""");
        return method;
    }
}