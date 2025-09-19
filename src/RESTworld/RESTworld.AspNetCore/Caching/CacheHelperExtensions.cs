using Microsoft.AspNetCore.OData.Query;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.Common.Dtos;
using RESTworld.EntityFrameworkCore.Models;
using System;
using System.Runtime.CompilerServices;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Caching;

/// <summary>
/// Extensions for <see cref="ICacheHelper"/>.
/// </summary>
public static class CacheHelperExtensions
{
    /// <summary>
    /// Caches the result in a GET operation with the default lifetime.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="id">The ID of the resource to get.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <returns>The entry.</returns>
    public static Task<TServiceResponse> CacheGetWithCurrentUserAsync<TServiceResponse>(this ICacheHelper cache, long id, Func<string, Task<TServiceResponse>> factory, [CallerMemberName] string? action = null)
        => cache.GetOrCreateWithCurrentUserAsync(CacheKeys.CreateCacheKeyForGet<TServiceResponse>(id, action), nameof(CachingOptions.Get), factory);

    /// <summary>
    /// Caches the result in a GET operation with the default lifetime.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="parameter">A parameter to differentiate DTOs. Normally this is some kind of ID.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <returns>The entry.</returns>
    public static Task<TServiceResponse> CacheGetWithCurrentUserAsync<TServiceResponse, TParam>(this ICacheHelper cache, TParam parameter, Func<string, Task<TServiceResponse>> factory, [CallerMemberName] string? action = null)
        => cache.GetOrCreateWithCurrentUserAsync(CacheKeys.CreateCacheKeyForGet<TServiceResponse, TParam>(parameter, action), nameof(CachingOptions.Get), factory);

    /// <summary>
    /// Caches the result in a GET History operation with the default lifetime.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="oDataQueryOptions">The raw OData query options.</param>
    /// <param name="at">Specifies a specific point in time.</param>
    /// <param name="from">Specifies the start of a time range.</param>
    /// <param name="to">Specifies the exclusive end of a time range.</param>
    /// <param name="toInclusive">Specifies the inclusive end of a time range.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <returns>The entry.</returns>
    public static Task<TServiceResponse> CacheGetHistoryWithCurrentUserAsync<TServiceResponse>(
        this ICacheHelper cache,
        ODataRawQueryOptions oDataQueryOptions,
        DateTimeOffset? at,
        DateTimeOffset? from,
        DateTimeOffset? to,
        DateTimeOffset? toInclusive,
        Func<string, Task<TServiceResponse>> factory,
        [CallerMemberName] string? action = null)
        => cache.GetOrCreateWithCurrentUserAsync(
            CacheKeys.CreateCacheKeyForGetHistory<TServiceResponse>(oDataQueryOptions, at, from, to, toInclusive, action),
            nameof(CachingOptions.GetHistory),
            factory);

    /// <summary>
    /// Caches the result in a GET History operation with the default lifetime.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="parameter">A parameter to differentiate DTOs. Normally this is some kind of ID.</param>
    /// <param name="oDataQueryOptions">The raw OData query options.</param>
    /// <param name="at">Specifies a specific point in time.</param>
    /// <param name="from">Specifies the start of a time range.</param>
    /// <param name="to">Specifies the exclusive end of a time range.</param>
    /// <param name="toInclusive">Specifies the inclusive end of a time range.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <returns>The entry.</returns>
    public static Task<TServiceResponse> CacheGetHistoryWithCurrentUserAsync<TServiceResponse, TParam>(this ICacheHelper cache,
        TParam parameter,
        ODataRawQueryOptions oDataQueryOptions,
        DateTimeOffset? at,
        DateTimeOffset? from,
        DateTimeOffset? to,
        DateTimeOffset? toInclusive,
        Func<string, Task<TServiceResponse>> factory,
        [CallerMemberName] string? action = null)
        => cache.GetOrCreateWithCurrentUserAsync(
            CacheKeys.CreateCacheKeyForGetHistory<TServiceResponse, TParam>(parameter, oDataQueryOptions, at, from, to, toInclusive, action),
            nameof(CachingOptions.GetHistory),
            factory);

    /// <summary>
    /// Caches the result in a GET List operation with the default lifetime.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="oDataQueryOptions">The raw OData query options.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <returns>The entry.</returns>
    public static Task<TServiceResponse> CacheGetListWithCurrentUserAsync<TServiceResponse>(this ICacheHelper cache, ODataRawQueryOptions oDataQueryOptions, Func<string, Task<TServiceResponse>> factory, [CallerMemberName] string? action = null)
        => cache.GetOrCreateWithCurrentUserAsync(CacheKeys.CreateCacheKeyForGetList<TServiceResponse>(oDataQueryOptions, action), nameof(CachingOptions.GetList), factory);

    /// <summary>
    /// Caches the result in a GET List operation with the default lifetime.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="parameter">A parameter to differentiate DTOs. Normally this is some kind of ID.</param>
    /// <param name="oDataQueryOptions">The raw OData query options.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <returns>The entry.</returns>
    public static Task<TServiceResponse> CacheGetListWithCurrentUserAsync<TServiceResponse, TParam>(this ICacheHelper cache, TParam parameter, ODataRawQueryOptions oDataQueryOptions, Func<string, Task<TServiceResponse>> factory, [CallerMemberName] string? action = null)
        => cache.GetOrCreateWithCurrentUserAsync(CacheKeys.CreateCacheKeyForGetList<TServiceResponse, TParam>(parameter, oDataQueryOptions, action), nameof(CachingOptions.GetList), factory);

    /// <summary>
    /// Removes an entry from the cache that was previously cached.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="id">The ID of the resource to get.</param>
    /// <param name="action">The name of the GET method.</param>
    public static void RemoveGetWithCurrentUser<TServiceResponse>(this ICacheHelper cache, long id, string action = nameof(ReadController<EntityBase, DtoBase, DtoBase>.GetAsync))
        => cache.RemoveWithCurrentUser(CacheKeys.CreateCacheKeyForGet<TServiceResponse>(id, action));

    /// <summary>
    /// Removes an entry from the cache that was previously cached.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="parameter">A parameter to differentiate DTOs. Normally this is some kind of ID.</param>
    /// <param name="action">The name of the GET method.</param>
    public static void RemoveGetWithCurrentUser<TServiceResponse, TParam>(this ICacheHelper cache, TParam parameter, string action = nameof(ReadController<EntityBase, DtoBase, DtoBase>.GetAsync))
        => cache.RemoveWithCurrentUser(CacheKeys.CreateCacheKeyForGet<TServiceResponse, TParam>(parameter, action));

    /// <summary>
    /// Removes an entry from the cache for all users that was previously cached.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="id">The ID of the resource to get.</param>
    /// <param name="action">The name of the GET method.</param>
    public static void RemoveGetForAllUsers<TServiceResponse>(this ICacheHelper cache, long id, string action = nameof(ReadController<EntityBase, DtoBase, DtoBase>.GetAsync))
        => cache.RemoveForAllUsers(CacheKeys.CreateCacheKeyForGet<TServiceResponse>(id, action));

    /// <summary>
    /// Removes an entry from the cache for all users that was previously cached.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="parameter">A parameter to differentiate DTOs. Normally this is some kind of ID.</param>
    /// <param name="action">The name of the GET method.</param>
    public static void RemoveGetForAllUsers<TServiceResponse, TParam>(this ICacheHelper cache, TParam parameter, string action = nameof(ReadController<EntityBase, DtoBase, DtoBase>.GetAsync))
        => cache.RemoveForAllUsers(CacheKeys.CreateCacheKeyForGet<TServiceResponse, TParam>(parameter, action));

    /// <summary>
    /// Removes all list entries from the cache that were previously cached for the current user,
    /// regardless of the OData query options used.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="action">The name of the GET List method.</param>
    public static void RemoveGetListWithCurrentUser<TServiceResponse>(this ICacheHelper cache, string action = nameof(ReadController<EntityBase, DtoBase, DtoBase>.GetListAsync))
        => cache.RemoveAllListEntriesWithCurrentUser(typeof(TServiceResponse).FullName ?? typeof(TServiceResponse).Name, action);

    /// <summary>
    /// Removes all list entries from the cache that were previously cached for the current user,
    /// regardless of the OData query options used.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="parameter">A parameter to differentiate DTOs. Normally this is some kind of ID.</param>
    /// <param name="action">The name of the GET List method.</param>
    public static void RemoveGetListWithCurrentUser<TServiceResponse, TParam>(this ICacheHelper cache, TParam parameter, string action = nameof(ReadController<EntityBase, DtoBase, DtoBase>.GetListAsync))
        => cache.RemoveAllListEntriesWithCurrentUser(CacheKeys.CreateTypeAndParameterKey<TServiceResponse, TParam>(parameter), action);

    /// <summary>
    /// Removes all list entries from the cache for all users that were previously cached,
    /// regardless of the OData query options used.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="action">The name of the GET List method.</param>
    public static void RemoveGetListForAllUsers<TServiceResponse>(this ICacheHelper cache, string action = nameof(ReadController<EntityBase, DtoBase, DtoBase>.GetListAsync))
        => cache.RemoveAllListEntriesForAllUsers(typeof(TServiceResponse).FullName ?? typeof(TServiceResponse).Name, action);

    /// <summary>
    /// Removes all list entries from the cache for all users that were previously cached,
    /// regardless of the OData query options used.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="parameter">A parameter to differentiate DTOs. Normally this is some kind of ID.</param>
    /// <param name="action">The name of the GET List method.</param>
    public static void RemoveGetListForAllUsers<TServiceResponse, TParam>(this ICacheHelper cache, TParam parameter, string action = nameof(ReadController<EntityBase, DtoBase, DtoBase>.GetListAsync))
        => cache.RemoveAllListEntriesForAllUsers(CacheKeys.CreateTypeAndParameterKey<TServiceResponse, TParam>(parameter), action);

    /// <summary>
    /// Caches the result in a GET operation with the default lifetime.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="id">The ID of the resource to get.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <returns>The entry.</returns>
    public static Task<TServiceResponse> CacheGetWithoutUserAsync<TServiceResponse>(this ICacheHelper cache, long id, Func<string, Task<TServiceResponse>> factory, [CallerMemberName] string? action = null)
        => cache.GetOrCreateWithoutUserAsync(CacheKeys.CreateCacheKeyForGet<TServiceResponse>(id, action), nameof(CachingOptions.Get), factory);

    /// <summary>
    /// Caches the result in a GET operation with the default lifetime.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="parameter">A parameter to differentiate DTOs. Normally this is some kind of ID.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <returns>The entry.</returns>
    public static Task<TServiceResponse> CacheGetWithoutUserAsync<TServiceResponse, TParam>(this ICacheHelper cache, TParam parameter, Func<string, Task<TServiceResponse>> factory, [CallerMemberName] string? action = null)
        => cache.GetOrCreateWithoutUserAsync(CacheKeys.CreateCacheKeyForGet<TServiceResponse, TParam>(parameter, action), nameof(CachingOptions.Get), factory);

    /// <summary>
    /// Caches the result in a GET List operation with the default lifetime.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="oDataQueryOptions">The raw OData query options.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <returns>The entry.</returns>
    public static Task<TServiceResponse> CacheGetListWithoutUserAsync<TServiceResponse>(this ICacheHelper cache, ODataRawQueryOptions oDataQueryOptions, Func<string, Task<TServiceResponse>> factory, [CallerMemberName] string? action = null)
        => cache.GetOrCreateWithoutUserAsync(CacheKeys.CreateCacheKeyForGetList<TServiceResponse>(oDataQueryOptions, action), nameof(CachingOptions.GetList), factory);

    /// <summary>
    /// Caches the result in a GET List operation with the default lifetime.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="parameter">A parameter to differentiate DTOs. Normally this is some kind of ID.</param>
    /// <param name="oDataQueryOptions">The raw OData query options.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <param name="action">The calling method. This is automatically filled out.</param>
    /// <returns>The entry.</returns>
    public static Task<TServiceResponse> CacheGetListWithoutUserAsync<TServiceResponse, TParam>(this ICacheHelper cache, TParam parameter, ODataRawQueryOptions oDataQueryOptions, Func<string, Task<TServiceResponse>> factory, [CallerMemberName] string? action = null)
        => cache.GetOrCreateWithoutUserAsync(CacheKeys.CreateCacheKeyForGetList<TServiceResponse, TParam>(parameter, oDataQueryOptions, action), nameof(CachingOptions.GetList), factory);

    /// <summary>
    /// Removes an entry from the cache that was previously cached.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="id">The ID of the resource to get.</param>
    /// <param name="action">The name of the GET method.</param>
    public static void RemoveGetWithoutUser<TServiceResponse>(this ICacheHelper cache, long id, string action = nameof(ReadController<EntityBase, DtoBase, DtoBase>.GetAsync))
        => cache.RemoveWithoutUser(CacheKeys.CreateCacheKeyForGet<TServiceResponse>(id, action));

    /// <summary>
    /// Removes an entry from the cache that was previously cached.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="parameter">A parameter to differentiate DTOs. Normally this is some kind of ID.</param>
    /// <param name="action">The name of the GET method.</param>
    public static void RemoveGet<TServiceResponse, TParam>(this ICacheHelper cache, TParam parameter, string action = nameof(ReadController<EntityBase, DtoBase, DtoBase>.GetAsync))
        => cache.RemoveWithoutUser(CacheKeys.CreateCacheKeyForGet<TServiceResponse, TParam>(parameter, action));

    /// <summary>
    /// Removes all list entries from the cache that were previously cached without a user,
    /// regardless of the OData query options used.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="action">The name of the GET List method.</param>
    public static void RemoveGetListWithoutUser<TServiceResponse>(this ICacheHelper cache, string action = nameof(ReadController<EntityBase, DtoBase, DtoBase>.GetListAsync))
        => cache.RemoveAllListEntriesWithoutUser(typeof(TServiceResponse).FullName ?? typeof(TServiceResponse).Name, action);

    /// <summary>
    /// Removes all list entries from the cache that were previously cached without a user,
    /// regardless of the OData query options used.
    /// </summary>
    /// <typeparam name="TServiceResponse">The type of the service response to return. In case of a custom controller this may also be the controller type.</typeparam>
    /// <typeparam name="TParam">The type of the parameter to differentiate DTOs. Normally this is some kind of ID.</typeparam>
    /// <param name="cache">The cache.</param>
    /// <param name="parameter">A parameter to differentiate DTOs. Normally this is some kind of ID.</param>
    /// <param name="action">The name of the GET List method.</param>
    public static void RemoveGetListWithoutUser<TServiceResponse, TParam>(this ICacheHelper cache, TParam parameter, string action = nameof(ReadController<EntityBase, DtoBase, DtoBase>.GetListAsync))
        => cache.RemoveAllListEntriesWithoutUser(CacheKeys.CreateTypeAndParameterKey<TServiceResponse, TParam>(parameter), action);
}