using HAL.Client.Net;
using HAL.Common;
using HAL.Common.Forms;
using Microsoft.Extensions.Logging;
using RESTworld.Common.Client;
using RESTworld.Common.Dtos;
using System.Text.Json;

namespace RESTworld.Client.Net;

/// <summary>
/// A client to call a RESTworld backend.
/// </summary>
public class RestWorldClient : IRestWorldClient
{
    private readonly ApiUrl _apiUrl;
    private readonly string? _defaultCurie;
    private readonly IHalClient _halClient;
    private readonly Resource<HomeDto> _homeResource;
    private readonly ILogger<RestWorldClient> _logger;

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

        logger.LogInformation("Created a new RestWorldClient for {ApiName} at {ApiUrl} with version {Version}.", apiUrl.Name, apiUrl.Url, version);
    }

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
        ArgumentNullException.ThrowIfNull(halClient);
        ArgumentNullException.ThrowIfNull(apiUrl);
        ArgumentNullException.ThrowIfNull(logger);

        if (string.IsNullOrWhiteSpace(apiUrl.Url))
            throw new ArgumentException("The apiUrl.Url must contain a value.", nameof(apiUrl));

        var response = await halClient.GetAsync<HomeDto>(new Uri(apiUrl.Url), version: apiUrl.Version?.ToString(), cancellationToken: cancellationToken);
        if (!response.Succeeded || response.Resource is null)
        {
            var jsonOptions = new JsonSerializerOptions(JsonSerializerDefaults.Web)
            {
                WriteIndented = true
            };
            var ex = new HttpRequestException($"Unable to get the home resource from {apiUrl.Url} with version {apiUrl.Version}. Response was:{Environment.NewLine}{JsonSerializer.Serialize(response, jsonOptions)}", null, response.StatusCode);
            ex.Data["Response"] = response;
            throw ex;
        }

        return new RestWorldClient(halClient, response.Resource, apiUrl, logger);
    }

    /// <inheritdoc/>
    public Task<HalResponse<Resource<TState>>> DeleteAsync<TState>(
        Uri requestUri, string? eTag = null, IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null, string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.DeleteAsync<TState>(requestUri, eTag, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<Resource>> DeleteAsync(
        Uri requestUri, string? eTag = null, IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null, string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.DeleteAsync(requestUri, eTag, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource<TState>>> DeleteFormAsync<TState>(
        Uri requestUri,
        string? eTag = null,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.DeleteFormAsync<TState>(requestUri, eTag, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource>> DeleteFormAsync(
        Uri requestUri,
        string? eTag = null,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.DeleteFormAsync(requestUri, eTag, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public IDictionary<string, ICollection<Link>> GetAllLinksFromHome() => _homeResource.Links ?? new Dictionary<string, ICollection<Link>>();

    /// <inheritdoc/>
    public async Task<HalResponse<FormsResource<Page>>> GetAllPagesFormListAsync(
        string rel,
        string? curie = null,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        var response = await GetFormListAsync(rel, curie, uriParameters, headers, version, cancellationToken);

        if (!response.Succeeded || response.Resource is null)
            return response;

        if (!response.Succeeded || response.Resource is null || response.Resource.Embedded is null || !response.Resource.Embedded.TryGetValue(Common.Constants.ListItems, out var items))
            return response;

        items ??= [];

        var lastResponse = response;

        while (lastResponse.Resource!.TryFindLink("next", out var nextLink))
        {
            // Get the next response
            lastResponse = await nextLink!.FollowGetFormAsync<Page>(_halClient, null, headers, version, cancellationToken);

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
        response.Resource.State?.TotalPages = 1;

        response.Resource.Links!.Remove("next");

        return response;
    }

    /// <inheritdoc/>
    public async Task<HalResponse<Resource<Page>>> GetAllPagesListAsync(
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

        items ??= [];

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
        response.Resource.State?.TotalPages = 1;

        response.Resource.Links!.Remove("next");

        return response;
    }

    /// <inheritdoc/>
    public Task<HalResponse<Resource<TState>>> GetAsync<TState>(
        Uri requestUri,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.GetAsync<TState>(requestUri, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<Resource>> GetAsync(
        Uri requestUri,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.GetAsync(requestUri, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource<TState>>> GetFormAsync<TState>(
        Uri requestUri,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.GetFormAsync<TState>(requestUri, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource>> GetFormAsync(
        Uri requestUri,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.GetFormAsync(requestUri, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public async Task<HalResponse<FormsResource<Page>>> GetFormListAsync(
        string rel,
        string? curie = null,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
    {
        var link = GetLinkFromHome(rel, Common.Constants.GetListLinkName, curie)
            ?? throw new ArgumentException($"Unable to find a link with the rel {rel} and the curie {curie ?? "null"}", nameof(rel));
        var response = await link.FollowGetFormAsync<Page>(_halClient, uriParameters, headers, version, cancellationToken);

        return response;
    }

    /// <inheritdoc/>
    public Link? GetLinkFromHome(string rel, string? name = default, string? curie = default)
    {
        var fullRel = GetFullRel(rel, curie);
        var link = _homeResource.FindLink(fullRel, name);

        return link;
    }

    /// <inheritdoc/>
    public IEnumerable<Link> GetLinksFromHome(string rel, string? curie = default)
    {
        var fullRel = GetFullRel(rel, curie);
        var links = _homeResource.FindLinks(fullRel);

        return links;
    }

    /// <inheritdoc/>
    public async Task<HalResponse<Resource<Page>>> GetListAsync(
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
    public Task<HalResponse<Resource<TState>>> PostAsync<TRequest, TState>(
        Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null, string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.PostAsync<TRequest, TState>(requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<Resource>> PostAsync<TRequest>(
        Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null, string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.PostAsync(requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource<TState>>> PostFormAsync<TRequest, TState>(
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
     => _halClient.PostFormAsync<TRequest, TState>(requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource>> PostFormAsync<TRequest>(
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.PostFormAsync(requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<Resource<TState>>> PutAsync<TRequest, TState>(
        Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null, string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.PutAsync<TRequest, TState>(requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<Resource>> PutAsync<TRequest>(
        Uri requestUri, TRequest? content = default, IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null, string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.PutAsync(requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource<TState>>> PutFormAsync<TRequest, TState>(
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.PutFormAsync<TRequest, TState>(requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource>> PutFormAsync<TRequest>(
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.PutFormAsync(requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<Resource<TState>>> SendAsync<TRequest, TState>(
        HttpMethod method,
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.SendAsync<TRequest, TState>(method, requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<Resource>> SendAsync<TRequest>(
        HttpMethod method,
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.SendAsync(method, requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource<TState>>> SendFormAsync<TRequest, TState>(
        HttpMethod method,
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.SendFormAsync<TRequest, TState>(method, requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HalResponse<FormsResource>> SendFormAsync<TRequest>(
        HttpMethod method,
        Uri requestUri,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        CancellationToken cancellationToken = default)
        => _halClient.SendFormAsync(method, requestUri, content, uriParameters, headers, version, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> SendHttpRequestAsync<TRequest>(
        HttpMethod method,
        Uri requestUri,
        Accepts accepts,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = default,
        IDictionary<string, IEnumerable<string>>? headers = default,
        string? version = default,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default)
        => _halClient.SendHttpRequestAsync(method, requestUri, accepts, content, uriParameters, headers, version, completionOption, cancellationToken);

    /// <inheritdoc/>
    public Task<HttpResponseMessage> SendHttpRequestAsync<TRequest>(
        HttpRequestMessage request,
        Accepts accepts,
        TRequest? content = default,
        IDictionary<string, object>? uriParameters = null,
        IDictionary<string, IEnumerable<string>>? headers = null,
        string? version = null,
        HttpCompletionOption completionOption = HttpCompletionOption.ResponseContentRead,
        CancellationToken cancellationToken = default)
        => _halClient.SendHttpRequestAsync(request, accepts, content, uriParameters, headers, version, completionOption, cancellationToken);

    private string? GetDefaultCurie()
    {
        if (_homeResource is not null && _homeResource.TryFindLink("curies", out var link))
            return link?.Name;

        _logger.LogWarning("Unable to determine the default curie for the home resource at {Url} with version {Version}, because the home resource does not have a curies link.", _apiUrl.Url, _apiUrl.Version);
        return null;
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
}