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
}