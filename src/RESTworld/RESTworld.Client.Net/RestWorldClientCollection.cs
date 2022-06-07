using HAL.Client.Net;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RESTworld.Common.Client;

namespace RESTworld.Client.Net
{
    public class RestWorldClientCollection
    {
        private readonly IDictionary<string, RestWorldClient> _clients = new Dictionary<string, RestWorldClient>();

        public static async Task<RestWorldClientCollection> CreateAsync(IOptions<RestWorldClientOptions> options, IHalClient halClient, ILogger<RestWorldClient> logger, CancellationToken cancellationToken = default)
        {
            if (options is null || options.Value is null)
                throw new ArgumentNullException(nameof(options));

            if (options.Value.ClientSettings?.ApiUrls is null)
                return new RestWorldClientCollection(new Dictionary<string, RestWorldClient>());

            var tasks = options.Value.ClientSettings.ApiUrls.Where(a => a.Name is not null).ToDictionary(a => a.Name!, a => RestWorldClient.CreateAsync(halClient, a, logger, cancellationToken));

            await Task.WhenAll(tasks.Values);

            var clients = tasks.ToDictionary(p => p.Key, p => p.Value.Result);

            return new RestWorldClientCollection(clients);
        }

        public RestWorldClientCollection(IDictionary<string, RestWorldClient> clients)
        {
            _clients = clients ?? throw new ArgumentNullException(nameof(clients));
        }

        public RestWorldClient GetClient(string name) => _clients[name];
    }
}
