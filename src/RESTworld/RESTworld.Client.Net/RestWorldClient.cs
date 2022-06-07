using HAL.Client.Net;
using HAL.Common;
using Microsoft.Extensions.Logging;
using RESTworld.Common.Client;
using RESTworld.Common.Dtos;
using System.Text.Json;

namespace RESTworld.Client.Net
{
    public class RestWorldClient
    {
        private readonly IHalClient _halClient;
        private readonly ILogger<RestWorldClient> _logger;
        private Resource<HomeDto> _homeResource;
        private readonly ApiUrl _apiUrl;
        private string? _defaultCurie;
        private static readonly JsonSerializerOptions _jsonSerializerOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web);

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

        public IEnumerable<Link> GetLinksFromHome(string rel, string? curie = default)
        {
            var fullRel = GetFullRel(rel, curie);
            var links = _homeResource.FindLinks(fullRel);

            return links;
        }

        public IDictionary<string, ICollection<Link>> GetAllLinksFromHome()
        {
            return _homeResource.Links ?? new Dictionary<string, ICollection<Link>>();
        }

        public Link? GetLinkFromHome(string rel, string? name = default, string? curie = default)
        {
            var fullRel = GetFullRel(rel, curie);
            var link = _homeResource.FindLink(fullRel, name);

            return link;
        }

        public async Task<HalResponse<Page>> GetListAsync(
            string rel,
            string? curie = default,
            IDictionary<string, object>? uriParameters = default,
            IDictionary<string, IEnumerable<string>>? headers = default,
            string? version = default,
            CancellationToken cancellationToken = default)
        {
            var link = GetLinkFromHome(rel, Common.Constants.GetListLinkName, curie);

            if (link is null)
                throw new ArgumentException($"Unable to find a link with the rel {rel} and the curie {curie ?? "null"}", nameof(rel));

            var response = await link.FollowGetAsync<Page>(_halClient, uriParameters, headers, version, cancellationToken);

            return response;
        }

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

            if (items is null)
                items = new List<Resource>();

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
            if (curie is null)
                curie = _defaultCurie;

            // Combine curie and rel
            var fullRel = string.Concat(curie, ":", rel);

            return fullRel;
        }
    }
}
