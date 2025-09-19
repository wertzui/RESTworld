using RESTworld.AspNetCore.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Caching;

/// <summary>
/// Helper for caching using timeouts from <see cref="RestWorldOptions"/>.
/// 
/// </summary>
public interface ICacheHelper
{
    /// <summary>
    /// Gets or creates a cache entry for the current user with the specified key. The current users
    /// name is automatically added to the key so the result is not leaked to user users.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="key">The key in the cache.</param>
    /// <param name="timeoutKey">The key of the timeout from the options.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <returns>The entry.</returns>
    TItem GetOrCreateWithCurrentUser<TItem>(string key, string timeoutKey, Func<string, TItem> factory);

    /// <summary>
    /// Gets or creates a cache entry for the current user with the specified key asynchronous.
    /// The current users name is automatically added to the key so the result is not leaked to user users.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="key">The key in the cache.</param>
    /// <param name="timeoutKey">The key of the timeout from the options.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <returns>The entry.</returns>
    Task<TItem> GetOrCreateWithCurrentUserAsync<TItem>(string key, string timeoutKey, Func<string, Task<TItem>> factory);

    /// <summary>
    /// Gets or creates a cache entry with the specified key.
    /// No user is not added to the key so the result may be visible to other users if no other precautions are taken.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="key">The key in the cache.</param>
    /// <param name="timeoutKey">The key of the timeout from the options.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <returns>The entry.</returns>
    TItem GetOrCreateWithoutUser<TItem>(string key, string timeoutKey, Func<string, TItem> factory);

    /// <summary>
    /// Gets or creates a cache entry with the specified key asynchronous.
    /// No user is not added to the key so the result may be visible to other users if no other precautions are taken.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="key">The key in the cache.</param>
    /// <param name="timeoutKey">The key of the timeout from the options.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <returns>The entry.</returns>
    Task<TItem> GetOrCreateWithoutUserAsync<TItem>(string key, string timeoutKey, Func<string, Task<TItem>> factory);

    /// <summary>
    /// Gets or creates a cache entry with the specified key.
    /// The given users name is automatically added to the key so the result is not leaked to user users.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="key">The key in the cache.</param>
    /// <param name="username">The name of the user. This must be unique per user.</param>
    /// <param name="timeoutKey">The key of the timeout from the options.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <returns>The entry.</returns>
    TItem GetOrCreateWithUser<TItem>(string key, string username, string timeoutKey, Func<string, TItem> factory);

    /// <summary>
    /// Gets or creates a cache entry with the specified key asynchronous.
    /// The given users name is automatically added to the key so the result is not leaked to user users.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="key">The key in the cache.</param>
    /// <param name="username">The name of the user. This must be unique per user.</param>
    /// <param name="timeoutKey">The key of the timeout from the options.</param>
    /// <param name="factory">The factory method to create the entry if it does not exist.</param>
    /// <returns>The entry.</returns>
    Task<TItem> GetOrCreateWithUserAsync<TItem>(string key, string username, string timeoutKey, Func<string, Task<TItem>> factory);

    /// <summary>
    /// Removes the entry from the cache.
    /// The current users name is automatically added to the key so use this method to remove something that has been added with a user in mind.
    /// </summary>
    /// <param name="key">The key in the cache.</param>
    void RemoveWithCurrentUser(string key);

    /// <summary>
    /// Removes the entry from the cache.
    /// No user is not added to the key so use this method to remove something that has been added without a user in mind.
    /// </summary>
    /// <param name="key">The key in the cache.</param>
    void RemoveWithoutUser(string key);

    /// <summary>
    /// Removes the entry from the cache.
    /// The given users name is automatically added to the key so use this method to remove something that has been added with a user in mind.
    /// </summary>
    /// <param name="key">The key in the cache.</param>
    /// <param name="username">The name of the user. This must be unique per user.</param>
    void RemoveWithUser(string key, string username);

    /// <summary>
    /// Removes the entry from the cache for all users.
    /// Note that due to limitations in IMemoryCache, this method can only remove the non-user specific version of the key
    /// and not all user-specific variants.
    /// </summary>
    /// <param name="key">The key in the cache.</param>
    void RemoveForAllUsers(string key);

    /// <summary>
    /// Removes all list entries from the cache for the current user that match the given type and action.
    /// This will remove all entries regardless of the OData query options that were used.
    /// </summary>
    /// <param name="typeKey">The type key identifying the list entries (typically the full name of the service response type).</param>
    /// <param name="action">The action that was used to create the cache entries.</param>
    void RemoveAllListEntriesWithCurrentUser(string typeKey, string action);

    /// <summary>
    /// Removes all list entries from the cache without a user that match the given type and action.
    /// This will remove all entries regardless of the OData query options that were used.
    /// </summary>
    /// <param name="typeKey">The type key identifying the list entries (typically the full name of the service response type).</param>
    /// <param name="action">The action that was used to create the cache entries.</param>
    void RemoveAllListEntriesWithoutUser(string typeKey, string action);

    /// <summary>
    /// Removes all list entries from the cache for all users that match the given type and action.
    /// This will remove all entries regardless of the OData query options that were used.
    /// </summary>
    /// <param name="typeKey">The type key identifying the list entries (typically the full name of the service response type).</param>
    /// <param name="action">The action that was used to create the cache entries.</param>
    void RemoveAllListEntriesForAllUsers(string typeKey, string action);

    /// <summary>
    /// Sets the item in the cache with the specified key.
    /// The current users name is automatically added to the key so the result is not leaked to user users.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="key">The key in the cache.</param>
    /// <param name="timeoutKey">The key of the timeout from the options.</param>
    /// <param name="item"></param>
    /// <returns>The item.</returns>
    TItem SetWithCurrentUser<TItem>(string key, string timeoutKey, TItem item);

    /// <summary>
    /// Sets the item in the cache with the specified key.
    /// No user is not added to the key so the result may be visible to other users if no other precautions are taken.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="key">The key in the cache.</param>
    /// <param name="timeoutKey">The key of the timeout from the options.</param>
    /// <param name="item"></param>
    /// <returns>The item.</returns>
    TItem SetWithoutUser<TItem>(string key, string timeoutKey, TItem item);

    /// <summary>
    /// Sets the item in the cache with the specified key.
    /// The given users name is automatically added to the key so the result is not leaked to user users.
    /// </summary>
    /// <typeparam name="TItem">The type of the item.</typeparam>
    /// <param name="key">The key in the cache.</param>
    /// <param name="username">The name of the user. This must be unique per user.</param>
    /// <param name="timeoutKey">The key of the timeout from the options.</param>
    /// <param name="item"></param>
    /// <returns>The item.</returns>
    TItem SetWithUser<TItem>(string key, string username, string timeoutKey, TItem item);
}