using Microsoft.Extensions.Caching.Memory;
using Microsoft.Extensions.Options;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.Business.Authorization.Abstractions;
using System;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.Caching;

/// <inheritdoc/>
public class CacheHelper : ICacheHelper
{
    private readonly IMemoryCache _memoryCache;
    private readonly IUserAccessor _userAccessor;
    private readonly CachingOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="CacheHelper"/> class.
    /// </summary>
    /// <param name="memoryCache">The memory cache.</param>
    /// <param name="options">The options holding the default timeouts.</param>
    /// <param name="userAccessor">The user accessor.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public CacheHelper(
        IMemoryCache memoryCache,
        IOptions<RestWorldOptions> options,
        IUserAccessor userAccessor)
    {
        _memoryCache = memoryCache ?? throw new ArgumentNullException(nameof(memoryCache));
        _userAccessor = userAccessor ?? throw new ArgumentNullException(nameof(userAccessor));
        _options = options?.Value?.Caching ?? throw new ArgumentNullException(nameof(options));
    }

    /// <inheritdoc/>
    public TItem GetOrCreateWithUser<TItem>(string key, string username, string timeoutKey, Func<string, TItem> factory)
        => GetOrCreateWithoutUser(AddUserToKey(key, username), timeoutKey, factory);

    /// <inheritdoc/>
    public TItem GetOrCreateWithCurrentUser<TItem>(string key, string timeoutKey, Func<string, TItem> factory)
        => GetOrCreateWithoutUser(AddCurrentUserToKey(key), timeoutKey, factory);

    /// <inheritdoc/>
    public TItem GetOrCreateWithoutUser<TItem>(string key, string timeoutKey, Func<string, TItem> factory)
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
    public Task<TItem> GetOrCreateWithUserAsync<TItem>(string key, string username, string timeoutKey, Func<string, Task<TItem>> factory)
        => GetOrCreateWithoutUserAsync(AddUserToKey(key, username), timeoutKey, factory);

    /// <inheritdoc/>
    public Task<TItem> GetOrCreateWithCurrentUserAsync<TItem>(string key, string timeoutKey, Func<string, Task<TItem>> factory)
        => GetOrCreateWithoutUserAsync(AddCurrentUserToKey(key), timeoutKey, factory);

    /// <inheritdoc/>
    public async Task<TItem> GetOrCreateWithoutUserAsync<TItem>(string key, string timeoutKey, Func<string, Task<TItem>> factory)
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
    public void RemoveWithUser(string key, string username)
        => RemoveWithoutUser(AddUserToKey(key, username));

    /// <inheritdoc/>
    public void RemoveWithCurrentUser(string key)
        => RemoveWithoutUser(AddCurrentUserToKey(key));

    /// <inheritdoc/>
    public void RemoveWithoutUser(string key)
    {
        _memoryCache.Remove(key);
    }

    /// <inheritdoc/>
    public TItem SetWithUser<TItem>(string key, string username, string timeoutKey, TItem item)
        => SetWithoutUser(AddUserToKey(key, username), timeoutKey, item);

    /// <inheritdoc/>
    public TItem SetWithCurrentUser<TItem>(string key, string timeoutKey, TItem item)
        => SetWithoutUser(AddCurrentUserToKey(key), timeoutKey, item);

    /// <inheritdoc/>
    public TItem SetWithoutUser<TItem>(string key, string timeoutKey, TItem item)
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

    private string GetCurrentUserName()
    {
        return _userAccessor.User?.Identity?.Name ?? "$$ANONYMOUS$$";
    }

    private static string AddUserToKey(string key, string username) => string.Concat("User_", username, "_", key);

    private string AddCurrentUserToKey(string key) => AddUserToKey(key, GetCurrentUserName());
}