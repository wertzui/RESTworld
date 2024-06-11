using HAL.Client.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RESTworld.Common.Client;

namespace RESTworld.Client.Net;

/// <summary>
/// A collection of all available <see cref="IRestWorldClient"/>s.
/// </summary>
public class RestWorldClientCollection : IRestWorldClientCollection
{
    private readonly IDictionary<string, IRestWorldClient> _clients = new Dictionary<string, IRestWorldClient>();

    /// <summary>
    /// Creates a <see cref="RestWorldClientCollection"/> from the given <see cref="RestWorldClientOptions"/>.
    /// </summary>
    /// <param name="options">The options which contain information on how to construct clients.</param>
    /// <param name="halClientFactory">The factory which will generate the underlying <see cref="IHalClient"/>s.</param>
    /// <param name="logger">The logger.</param>
    /// <param name="cancellationToken">The cancellation token.</param>
    /// <returns>A fully initialized <see cref="RestWorldClientCollection"/>.</returns>
    /// <exception cref="Exception"></exception>
    public static async Task<RestWorldClientCollection> CreateAsync(IOptions<RestWorldClientOptions> options, IHalClientFactory halClientFactory, ILogger<RestWorldClient> logger, CancellationToken cancellationToken = default)
    {
        if (options is null || options.Value is null)
        {
            logger.LogWarning("Cannot create any RestWorldClients because no RestWorldClientOptions are present.");
            return new RestWorldClientCollection(new Dictionary<string, IRestWorldClient>());
        }

        if (options.Value.ClientSettings?.ApiUrls is null || options.Value.ClientSettings.ApiUrls.Length == 0)
        {
            logger.LogWarning("Cannot create any RestWorldClients because the RestWorldClientOptions do not contain any ApiUrls.");
            return new RestWorldClientCollection(new Dictionary<string, IRestWorldClient>());
        }

        var duplicateApiUrls = options.Value.ClientSettings.ApiUrls
            .Where(a => a.Name is not null)
            .GroupBy(a => a.Name)
            .Where(g => g.Count() > 1)
            .ToList();

        if (duplicateApiUrls.Any())
        {
            var message = $"Cannot create any RestWorldClients, because the following ApiUrls are defined more than once: {string.Join(", ", duplicateApiUrls.Select(g => $"{g.Key}: [{string.Join(", ", g.Select(a => $"{a.Url} (Version: {a.Version})"))}]"))}";
            logger.LogCritical(message);
            throw new Exception(message);
        }

        try
        {
            var tasks = options.Value.ClientSettings.ApiUrls
                .Where(a => a.Name is not null)
                .ToDictionary(a => a.Name!, a => RestWorldClient.CreateAsync(halClientFactory.GetClient(a.Name!), a, logger, cancellationToken));

            await Task.WhenAll(tasks.Values);

            var clients = tasks.ToDictionary(p => p.Key, p => (IRestWorldClient)p.Value.Result);

            return new RestWorldClientCollection(clients);
        }
        catch (Exception e)
        {
            var message = "Cannot create a RestWorldClientCollection because there was an exception while calling the home endpoints defined in the ApiUrls of the RestWorldClientOptions.";
            logger.LogCritical(e, message);
            throw new Exception(message, e);
        }
    }

    /// <summary>
    /// Creates a new instance of the <see cref="RestWorldClientCollection"/> class.
    /// </summary>
    /// <param name="clients">The ready-to-use clients.</param>
    /// <exception cref="ArgumentNullException"></exception>
    public RestWorldClientCollection(IDictionary<string, IRestWorldClient> clients)
    {
        _clients = clients ?? throw new ArgumentNullException(nameof(clients));
    }

    /// <inheritdoc/>
    public IRestWorldClient GetClient(string name)
    {
        if (!TryGetClient(name, out var client))
            throw new ArgumentException($"The client with the given name cannot be found: {name}", nameof(name));

        return client;
    }

    /// <inheritdoc/>
    public bool TryGetClient(string name, out IRestWorldClient client)
    {
        if (!_clients.TryGetValue(name, out client!))
            return false;

        return true;
    }
}
