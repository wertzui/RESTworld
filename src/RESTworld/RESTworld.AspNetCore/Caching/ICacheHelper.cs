﻿using RESTworld.AspNetCore.DependencyInjection;
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
    /// Removes all entries associated with the current user that have keys starting with the specified prefix.
    /// </summary>
    /// <remarks>This method only affects entries associated with the current user. The comparison type
    /// determines how the prefix is matched,  allowing for case-sensitive or case-insensitive comparisons depending on
    /// the specified <paramref name="comparisonType"/>.</remarks>
    /// <param name="prefix">The prefix to match against the keys of the entries to be removed.</param>
    /// <param name="comparisonType">The type of string comparison to use when matching the prefix. Defaults to <see cref="StringComparison.Ordinal"/>.</param>
    void RemoveByPrefixWithCurrentUser(string prefix, StringComparison comparisonType = StringComparison.Ordinal);

    /// <summary>
    /// Removes all entries from the collection whose keys start with the specified prefix.
    /// </summary>
    /// <remarks>This method only removes entries that match the specified prefix.
    /// The comparison behavior is determined by the <paramref name="comparisonType"/> parameter.</remarks>
    /// <param name="prefix">The prefix to match against the keys of the entries to be removed.  This value cannot be <see langword="null"/>
    /// or empty.</param>
    /// <param name="comparisonType">The type of string comparison to use when matching the prefix.  The default is <see cref="StringComparison.Ordinal"/>.</param>
    void RemoveByPrefixWithoutUser(string prefix, StringComparison comparisonType = StringComparison.Ordinal);

    /// <summary>
    /// Removes all entries associated with the specified user that have keys starting with the given prefix.
    /// </summary>
    /// <remarks>This method performs a case-sensitive or case-insensitive comparison based on the specified
    /// <paramref name="comparisonType"/>. Only entries that match both the prefix and the username will be
    /// removed.</remarks>
    /// <param name="prefix">The prefix to match against the keys of the entries to be removed.</param>
    /// <param name="username">The username associated with the entries to be removed. Cannot be <see langword="null"/> or empty.</param>
    /// <param name="comparisonType">The type of string comparison to use when matching the prefix. Defaults to <see cref="StringComparison.Ordinal"/>.</param>
    void RemoveByPrefixWithUser(string prefix, string username, StringComparison comparisonType = StringComparison.Ordinal);

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