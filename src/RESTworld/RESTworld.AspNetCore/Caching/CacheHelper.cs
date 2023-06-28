using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RESTworld.AspNetCore.DependencyInjection;
using System;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Caching
{
    /// <inheritdoc/>
    public class CacheHelper : ICacheHelper
    {
        private readonly IMemoryCache _memoryCache;
        private readonly CachingOptions _options;

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheHelper"/> class.
        /// </summary>
        /// <param name="memoryCache">The memory cache.</param>
        /// <param name="options">The options holding the default timeouts.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public CacheHelper(IMemoryCache memoryCache, IOptions<RestWorldOptions> options)
        {
            _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
            _options = options?.Value?.Caching ?? throw new ArgumentNullException(nameof(options));
        }

        /// <inheritdoc/>
        public TItem GetOrCreate<TItem>(string key, string timeoutKey, Func<string, TItem> factory)
        {
            var absoluteExpirationRelativeToNow = GetTimeout(timeoutKey);

            var result = _memoryCache.GetOrCreate(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
                return factory(key);
            });

            if (result is null)
                throw new ArgumentException($"Factory for key {key} returned null", nameof(factory));

            return result;
        }

        /// <inheritdoc/>
        public async Task<TItem> GetOrCreateAsync<TItem>(string key, string timeoutKey, Func<string, Task<TItem>> factory)
        {
            var absoluteExpirationRelativeToNow = GetTimeout(timeoutKey);

            var result = await _memoryCache.GetOrCreateAsync(key, entry =>
            {
                entry.AbsoluteExpirationRelativeToNow = absoluteExpirationRelativeToNow;
                return factory(key);
            });

            if (result is null)
                throw new ArgumentException($"Factory for key {key} returned null", nameof(factory));

            return result;
        }

        /// <inheritdoc/>
        public void Remove(string key)
        {
            _memoryCache.Remove(key);
        }

        /// <inheritdoc/>
        public TItem Set<TItem>(string key, string timeoutKey, TItem item)
        {
            var absoluteExpirationRelativeToNow = GetTimeout(timeoutKey);

            return _memoryCache.Set(key, item, absoluteExpirationRelativeToNow);
        }

        private TimeSpan GetTimeout(string timeoutKey)
        {
            if (!_options.TryGetValue(timeoutKey, out var absoluteExpirationRelativeToNow))
                throw new ArgumentException($"No timeout configured for key {timeoutKey}", nameof(timeoutKey));

            return absoluteExpirationRelativeToNow;
        }
    }
}