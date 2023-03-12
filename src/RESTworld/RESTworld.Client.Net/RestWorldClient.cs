using HAL.Client.Net;
using HAL.Common;
using Microsoft.Extensions.Logging;
using RESTworld.Common.Client;
using RESTworld.Common.Dtos;

namespace RESTworld.Client.Net
{
    /// <summary>
    /// A client to call a RESTworld backend.
    /// </summary>
    public class RestWorldClient : IRestWorldClient
    {
        private readonly IHalClient _halClient;
        private readonly ILogger<RestWorldClient> _logger;
        private readonly Resource<HomeDto> _homeResource;
        private readonly ApiUrl _apiUrl;
        private readonly string? _defaultCurie;

        /// <summary>
        /// Creates a new client for the given <paramref name="apiUrl"/>.
        /// </summary>
        /// <param name="halClient">The underlying <see cref="IHalClient"/> to use for all calls.</param>
        /// <param name="apiUrl">The URI which points to the home resource.</param>
        /// <param name="logger">The logger.</param>
        /// <param name="cancellationToken">The cancellation token.</param>
        /// <returns>A new client constructed from the home resource found at the given <paramref name="apiUrl"/>.</returns>
        /// <exception cref="ArgumentNullException"></exception>
        /// <exception cref="ArgumentException"></exception>
        public static async Task<RestWorldClient> CreateAsync(IHalClient halClient, ApiUrl apiUrl, ILogger<RestWorldClient> logger, CancellationToken cancellationToken = default)
        {
            if (halClient is null)
                throw new ArgumentNullException(nameof(halClient));

            if (apiUrl is null)
                throw new ArgumentNullException(nameof(apiUrl));

            if (logger is null)
                throw new ArgumentNullException(nameof(logger));

            if (string.IsNullOrWhiteSpace(apiUrl.Url))
                throw new ArgumentException("The apiUrl.Url must contain a value.", nameof(apiUrl));

            var response = await halClient.GetAsync<HomeDto>(new Uri(apiUrl.Url), version: apiUrl.Version?.ToString(), cancellationToken: cancellationToken);
            if (!response.Succeeded || response.Resource is null)
            {
                var ex = new HttpRequestException($"Unable to get the home resource from {apiUrl.Url} with version {apiUrl.Version}. Response was {response}", null, response.StatusCode);
                ex.Data["Response"] = response;
                throw ex;
            }

            return new RestWorldClient(halClient, response.Resource, apiUrl, logger);
        }

        /// <summary>
        /// Creates a new instance of the <see cref="RestWorldClient"/> class.
        /// </summary>
        /// <param name="halClient">The underlying <see cref="IHalClient"/> to use for all calls.</param>
        /// <param name="homeResource">The home resource to construct this client for.</param>
        /// <param name="apiUrl">The URI which points to the home resource. Used for version information.</param>
        /// <param name="logger">The logger.</param>
        /// <exception cref="ArgumentNullException"></exception>
        public RestWorldClient(IHalClient halClient, Resource<HomeDto> homeResource, ApiUrl apiUrl, ILogger<RestWorldClient> logger)
        {
            _halClient = halClient ?? throw new ArgumentNullException(nameof(halClient));
            _homeResource = homeResource ?? throw new ArgumentNullException(nameof(homeResource));
            _apiUrl = apiUrl ?? throw new ArgumentNullException(nameof(apiUrl));
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));

            var version = apiUrl.Version?.ToString();
            var homeVersions = homeResource.State?.Versions;
            if (version is not null && homeVersions is not null)
            {
                if (homeVersions.Deprecated is not null && homeVersions.Deprecated.Contains(version))
                    logger.LogWarning("The version {Version} used for {ApiName} at {ApiUrl} is deprecated.", version, apiUrl.Name, apiUrl.Url);
                else if (homeVersions.Supported is not null && !homeVersions.Supported.Contains(version))
                    logger.LogError("The version {Version} used for {ApiName} at {ApiUrl} is neither supported nor deprecated. Expect calls to fail!", version, apiUrl.Name, apiUrl.Url);
            }

            _defaultCurie = GetDefaultCurie();
        }

        private string? GetDefaultCurie()
        {
            if (_homeResource is not null && _homeResource.TryFindLink("curies", out var link))
                return link?.Name;

            _logger.LogWarning("Unable to determine the default curie for the home resource at {Url} with version {Version}, because the home resource does not have a curies link.", _apiUrl.Url, _apiUrl.Version);
            return null;
        }

        /// <inheritdoc/>
        public IEnumerable<Link> GetLinksFromHome(string rel, string? curie = default)
        {
            var fullRel = GetFullRel(rel, curie);
            var links = _homeResource.FindLinks(fullRel);

            return links;
        }

        /// <inheritdoc/>
        public IDictionary<string, ICollection<Link>> GetAllLinksFromHome()
        {
            return _homeResource.Links ?? new Dictionary<string, ICollection<Link>>();
        }

        /// <inheritdoc/>
        public Link? GetLinkFromHome(string rel, string? name = default, string? curie = default)
        {
            var fullRel = GetFullRel(rel, curie);
            var link = _homeResource.FindLink(fullRel, name);

            return link;
        }

        /// <inheritdoc/>
        public async Task<HalResponse<Page>> GetListAsync(
            string rel,
            string? curie = default,
            IDictionary<string, object>? uriParameters = default,
            IDictionary<string, IEnumerable<string>>? headers = default,
            string? version = default,
            CancellationToken cancellationToken = default)
        {
            var link = GetLinkFromHome(rel, Common.Constants.GetListLinkName, curie)
                ?? throw new ArgumentException($"Unable to find a link with the rel {rel} and the curie {curie ?? "null"}", nameof(rel));
            var response = await link.FollowGetAsync<Page>(_halClient, uriParameters, headers, version, cancellationToken);

            return response;
        }

        /// <inheritdoc/>
        public async Task<HalResponse<Page>> GetAllPagesListAsync(
            string rel,
            string? curie = default,
            IDictionary<string, object>? uriParameters = default,
            IDictionary<string, IEnumerable<string>>? headers = default,
            string? version = default,
            CancellationToken cancellationToken = default)
        {
            var response = await GetListAsync(rel, curie, uriParameters, headers, version, cancellationToken);

            if (!response.Succeeded || response.Resource is null)
                return response;

            if (!response.Succeeded || response.Resource is null || response.Resource.Embedded is null || !response.Resource.Embedded.TryGetValue(Common.Constants.ListItems, out var items))
                return response;

            items ??= new List<Resource>();

            var lastResponse = response;

            while (lastResponse.Resource!.TryFindLink("next", out var nextLink))
            {
                // Get the next response
                lastResponse = await nextLink!.FollowGetAsync<Page>(_halClient, null, headers, version, cancellationToken);

                if (!lastResponse.Succeeded)
                    return lastResponse;

                // Combine the embedded items
                if (lastResponse.Resource?.Embedded is not null && lastResponse.Resource.Embedded.TryGetValue(Common.Constants.ListItems, out var lastItems) && lastItems is not null)
                {
                    foreach (var embeddedItemResource in lastItems)
                    {
                        items.Add(embeddedItemResource);
                    }
                }
            }

            // We combined everything, so there is just one big page
            if (response.Resource.State is not null)
                response.Resource.State.TotalPages = 1;

            response.Resource.Links!.Remove("next");

            return response;
        }

        private string GetFullRel(string rel, string? curie)
        {
            // rel already includes a curie => just return it
            if (rel.Contains(':'))
                return rel;

            // No curie given => use default curie.
            curie ??= _defaultCurie;

            // Combine curie and rel
            var fullRel = string.Concat(curie, ":", rel);

            return fullRel;
        }

        /// <inheritdoc/>
        public Task<HalResponse<TResponse>> DeleteAsync<TResponse>(
            Uri requestUri, string? eTag = null, IDictionary<string, object>? uriParameters = null,
            IDictionary<string, IEnumerable<string>>? headers = null, string? version = null,
            CancellationToken cancellationToken = default)
            => _halClient.DeleteAsync<TResponse>(requestUri, eTag, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse<TResponse>> GetAsync<TResponse>(
            Uri requestUri, IDictionary<string, object>? uriParameters = null,
            IDictionary<string, IEnumerable<string>>? headers = null, string? version = null,
            CancellationToken cancellationToken = default)
            => _halClient.GetAsync<TResponse>(requestUri, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse<TResponse>> PostAsync<TRequest, TResponse>(
            Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null,
            IDictionary<string, IEnumerable<string>>? headers = null, string? version = null,
            CancellationToken cancellationToken = default)
            => _halClient.PostAsync<TRequest, TResponse>(requestUri, content, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse<TResponse>> PutAsync<TRequest, TResponse>(
            Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null,
            IDictionary<string, IEnumerable<string>>? headers = null, string? version = null,
            CancellationToken cancellationToken = default)
            => _halClient.PutAsync<TRequest, TResponse>(requestUri, content, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse<TResponse>> SendAsync<TRequest, TResponse>(
            HttpMethod method, Uri requestUri, TRequest? content = default,
            IDictionary<string, object>? uriParameters = null, IDictionary<string, IEnumerable<string>>? headers = null,
            string? version = null, CancellationToken cancellationToken = default)
            => _halClient.SendAsync<TRequest, TResponse>(method, requestUri, content, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse> DeleteAsync(
            Uri requestUri, string? eTag = null, IDictionary<string, object>? uriParameters = null,
            IDictionary<string, IEnumerable<string>>? headers = null, string? version = null,
            CancellationToken cancellationToken = default)
            => _halClient.DeleteAsync(requestUri, eTag, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse> GetAsync(
            Uri requestUri, IDictionary<string, object>? uriParameters = null,
            IDictionary<string, IEnumerable<string>>? headers = null, string? version = null,
            CancellationToken cancellationToken = default)
            => _halClient.GetAsync(requestUri, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse> PostAsync<TRequest>(
            Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null,
            IDictionary<string, IEnumerable<string>>? headers = null, string? version = null,
            CancellationToken cancellationToken = default)
            => _halClient.PostAsync(requestUri, content, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse> PutAsync<TRequest>(
            Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null,
            IDictionary<string, IEnumerable<string>>? headers = null, string? version = null,
            CancellationToken cancellationToken = default)
            => _halClient.PutAsync(requestUri, content, uriParameters, headers, version, cancellationToken);

        /// <inheritdoc/>
        public Task<HalResponse> SendAsync<TRequest>(
            HttpMethod method,
            Uri requestUri,
            TRequest? content = default,
            IDictionary<string, object>? uriParameters = null,
            IDictionary<string, IEnumerable<string>>? headers = null,
            string? version = null,
            CancellationToken cancellationToken = default)
            => _halClient.SendAsync(method, requestUri, content, uriParameters, headers, version, cancellationToken);
    }
}
