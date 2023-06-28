using RESTworld.AspNetCore.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Caching
{
    /// <summary>
    /// Helper for caching using timeouts from <see cref="RestWorldOptions"/>.
    /// </summary>
    public interface ICacheHelper
    {
        /// <summary>
        /// Gets or creates a cache entry with the specified key.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="key">The key in the cache.</param>
        /// <param name="timeoutKey">The key of the timeout from the options.</param>
        /// <param name="factory">The factory method to create the entry if it does not exist.</param>
        /// <returns>The entry.</returns>
        TItem GetOrCreate<TItem>(string key, string timeoutKey, Func<string, TItem> factory);

        /// <summary>
        /// Gets or creates a cache entry with the specified key asynchronous.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="key">The key in the cache.</param>
        /// <param name="timeoutKey">The key of the timeout from the options.</param>
        /// <param name="factory">The factory method to create the entry if it does not exist.</param>
        /// <returns>The entry.</returns>
        Task<TItem> GetOrCreateAsync<TItem>(string key, string timeoutKey, Func<string, Task<TItem>> factory);

        /// <summary>
        /// Removes the entry from the cache.
        /// </summary>
        /// <param name="key">The key in the cache.</param>
        void Remove(string key);

        /// <summary>
        /// Sets the item in the cache with the specified key.
        /// </summary>
        /// <typeparam name="TItem">The type of the item.</typeparam>
        /// <param name="key">The key in the cache.</param>
        /// <param name="timeoutKey">The key of the timeout from the options.</param>
        /// <param name="item"></param>
        /// <returns>The item.</returns>
        TItem Set<TItem>(string key, string timeoutKey, TItem item);
    }
}