using Microsoft.AspNetCore.OData.Query;
using System;
using System.Runtime.CompilerServices;

namespace RESTworld.AspNetCore.Caching;

/// <summary>
/// Contains helper methods for generating cache keys.
/// </summary>
public static class CacheKeys
{
    /// <summary>
    /// Creates the cache key for a GET operation where single id is passed as parameter.
    /// </summary>
    /// <remarks>
    /// The form is: {action}_{TDtoFullName}_{id}
    /// </remarks>
    /// <param name="id">The ID of the resource to get.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <typeparam name="TDto">The type of the DTO to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <returns>The key for the cache.</returns>
    public static string CreateCacheKeyForGet<TDto>(long id, [CallerMemberName] string? action = null) => CreateCacheKeyForGet<TDto, long>(id, action);

    /// <summary>
    /// Creates the cache key for a GET operation where single key is passed as parameter.
    /// </summary>
    /// <remarks>
    /// The form is: {action}_{TDtoFullName}_{parameter}
    /// </remarks>
    /// <param name="parameter">A parameter to differentiate DTOs. Normally this is some kind of ID.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <typeparam name="TDto">The type of the DTO to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <returns>The key for the cache.</returns>
    public static string CreateCacheKeyForGet<TDto, TParam>(TParam parameter, [CallerMemberName] string? action = null) => string.Concat(CreateChacheKeyPrefix<TDto>(action), "_", parameter);

    /// <summary>
    /// Creates a key combining type and parameter information for pattern-based cache operations.
    /// </summary>
    /// <typeparam name="TDto">The type of the DTO.</typeparam>
    /// <typeparam name="TParam">The type of the parameter.</typeparam>
    /// <param name="parameter">The parameter value.</param>
    /// <returns>A key combining type and parameter information.</returns>
    public static string CreateTypeAndParameterKey<TDto, TParam>(TParam parameter) => string.Concat(typeof(TDto).FullName, "_", parameter);

    /// <summary>
    /// Creates the cache key for a get list operation.
    /// </summary>
    /// <remarks>
    /// The form is: {action}_{TDtoFullName}_Apply:{Apply}_Compute:{Compute}_Count:{Count}_DeltaToken:{DeltaToken}_Expand:{Expand}_Filter:{Filter}_Format:{Format}_OrderBy:{OrderBy}_Search:{Search}_Select:{Select}_Skip:{Skip}_SkipToken:{SkipToken}_Top:{Top}
    /// </remarks>
    /// <param name="oDataQueryOptions">The raw OData query options.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <typeparam name="TDto">The type of the DTO to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <returns>The key for the cache.</returns>
    public static string CreateCacheKeyForGetList<TDto>(
        ODataRawQueryOptions oDataQueryOptions,
        [CallerMemberName] string? action = null)
        => string.Concat(
            CreateChacheKeyPrefix<TDto>(action),
            "_Apply:", oDataQueryOptions.Apply,
            "_Compute:", oDataQueryOptions.Compute,
            "_Count:", oDataQueryOptions.Count,
            "_DeltaToken:", oDataQueryOptions.DeltaToken,
            "_Expand:", oDataQueryOptions.Expand,
            "_Filter:", oDataQueryOptions.Filter,
            "_Format:", oDataQueryOptions.Format,
            "_OrderBy:", oDataQueryOptions.OrderBy,
            "_Search:", oDataQueryOptions.Search,
            "_Select:", oDataQueryOptions.Select,
            "_Skip:", oDataQueryOptions.Skip,
            "_SkipToken:", oDataQueryOptions.SkipToken,
            "_Top:", oDataQueryOptions.Top);

    /// <summary>
    /// Creates the cache key for a get history operation.
    /// </summary>
    /// <remarks>
    /// The form is: {action}_{TDtoFullName}_Apply:{Apply}_Compute:{Compute}_Count:{Count}_DeltaToken:{DeltaToken}_Expand:{Expand}_Filter:{Filter}_Format:{Format}_OrderBy:{OrderBy}_Search:{Search}_Select:{Select}_Skip:{Skip}_SkipToken:{SkipToken}_Top:{Top}_At:{At}_From:{From}_To:{To}_ToInclusive:{ToInclusive}
    /// </remarks>
    /// <param name="oDataQueryOptions">The raw OData query options.</param>
    /// <param name="at">Specifies a specific point in time.</param>
    /// <param name="from">Specifies the start of a time range.</param>
    /// <param name="to">Specifies the exclusive end of a time range.</param>
    /// <param name="toInclusive">Specifies the inclusive end of a time range.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <typeparam name="TDto">The type of the DTO to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <returns>The key for the cache.</returns>
    public static string CreateCacheKeyForGetHistory<TDto>(
        ODataRawQueryOptions oDataQueryOptions,
        DateTimeOffset? at,
        DateTimeOffset? from,
        DateTimeOffset? to,
        DateTimeOffset? toInclusive,
        [CallerMemberName] string? action = null)
        => string.Concat(
            CreateCacheKeyForGetList<TDto>(oDataQueryOptions, action),
            "_At:", at,
            "_From:", from,
            "_To:", to,
            "_ToInclusive", toInclusive);

    /// <summary>
    /// Creates the cache key for a get list operation with one additional parameter.
    /// </summary>
    /// <remarks>
    /// The form is: {action}_{TDtoFullName}_{parameter}_Apply:{Apply}_Compute:{Compute}_Count:{Count}_DeltaToken:{DeltaToken}_Expand:{Expand}_Filter:{Filter}_Format:{Format}_OrderBy:{OrderBy}_Search:{Search}_Select:{Select}_Skip:{Skip}_SkipToken:{SkipToken}_Top:{Top}_{parameter}
    /// </remarks>
    /// <param name="parameter">An extra parameter to differentiate the key</param>
    /// <param name="oDataQueryOptions">The raw OData query options.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <typeparam name="TDto">The type of the DTO to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <returns>The key for the cache.</returns>
    public static string CreateCacheKeyForGetList<TDto, TParam>(
        TParam parameter,
        ODataRawQueryOptions oDataQueryOptions,
        [CallerMemberName] string? action = null)
        => string.Concat(
            CreateCacheKeyForGetList<TDto>(oDataQueryOptions, action),
            "_", parameter);

    /// <summary>
    /// Creates the cache key for a get history operation with one additional parameter.
    /// </summary>
    /// <remarks>
    /// The form is: {action}_{TDtoFullName}_Apply:{Apply}_Compute:{Compute}_Count:{Count}_DeltaToken:{DeltaToken}_Expand:{Expand}_Filter:{Filter}_Format:{Format}_OrderBy:{OrderBy}_Search:{Search}_Select:{Select}_Skip:{Skip}_SkipToken:{SkipToken}_Top:{Top}_{parameter}_At:{At}_From:{From}_To:{To}_ToInclusive:{ToInclusive}_{parameter}
    /// </remarks>
    /// <param name="parameter">An extra parameter to differentiate the key</param>
    /// <param name="oDataQueryOptions">The raw OData query options.</param>
    /// <param name="at">Specifies a specific point in time.</param>
    /// <param name="from">Specifies the start of a time range.</param>
    /// <param name="to">Specifies the exclusive end of a time range.</param>
    /// <param name="toInclusive">Specifies the inclusive end of a time range.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <typeparam name="TDto">The type of the DTO to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <returns>The key for the cache.</returns>
    public static string CreateCacheKeyForGetHistory<TDto, TParam>(
        TParam parameter,
        ODataRawQueryOptions oDataQueryOptions,
        DateTimeOffset? at,
        DateTimeOffset? from,
        DateTimeOffset? to,
        DateTimeOffset? toInclusive,
        [CallerMemberName] string? action = null)
        => string.Concat(
            CreateCacheKeyForGetHistory<TDto>(oDataQueryOptions, at, from, to, toInclusive, action),
            "_", parameter);

    /// <summary>
    /// Creates the common prefix for a get list operation.
    /// </summary>
    /// <remarks>
    /// The form is: {action}_{TDtoFullName}
    /// </remarks>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <typeparam name="TDto">The type of the DTO to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <returns>The prefix for the cache.</returns>
    public static string CreateChacheKeyPrefix<TDto>([CallerMemberName] string? action = null)
        => string.Concat(action, "_", typeof(TDto).FullName);
}