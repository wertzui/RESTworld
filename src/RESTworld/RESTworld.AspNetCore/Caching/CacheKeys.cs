using Microsoft.AspNetCore.OData.Query;
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
    /// <param name="id">The ID of the resource to get.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <typeparam name="TDto">The type of the DTO to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <returns>The key for the cache.</returns>
    public static string CreateCacheKeyForGet<TDto>(long id, [CallerMemberName] string? action = null) => CreateCacheKeyForGet<TDto, long>(id, action);

    /// <summary>
    /// Creates the cache key for a GET operation where single key is passed as parameter.
    /// </summary>
    /// <param name="parameter">A parameter to differentiate DTOs. Normally this is some kind of ID.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <typeparam name="TDto">The type of the DTO to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <returns>The key for the cache.</returns>
    public static string CreateCacheKeyForGet<TDto, TParam>(TParam parameter, [CallerMemberName] string? action = null) => string.Concat(action, "_", typeof(TDto).FullName, "_", parameter);

    /// <summary>
    /// Creates the cache key for a get list operation.
    /// </summary>
    /// <param name="oDataQueryOptions">The raw OData query options.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <typeparam name="TDto">The type of the DTO to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <returns>The key for the cache.</returns>
    public static string CreateCacheKeyForGetList<TDto>(ODataRawQueryOptions oDataQueryOptions, [CallerMemberName] string? action = null)
        => string.Concat(
        action,
        "_",
        typeof(TDto).FullName,
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
    /// Creates the cache key for a get list operation with one additional parameter.
    /// </summary>
    /// <param name="parameter">An extra parameter to differentiate the key</param>
    /// <param name="oDataQueryOptions">The raw OData query options.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <typeparam name="TDto">The type of the DTO to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <returns>The key for the cache.</returns>
    public static string CreateCacheKeyForGetList<TDto, TParam>(TParam parameter, ODataRawQueryOptions oDataQueryOptions, [CallerMemberName] string? action = null)
        => string.Concat(CreateCacheKeyForGetList<TDto>(oDataQueryOptions, action), "_", parameter);
}
