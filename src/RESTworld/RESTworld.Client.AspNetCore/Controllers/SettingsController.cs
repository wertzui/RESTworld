using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using Microsoft.Extensions.ServiceDiscovery;
using RESTworld.AspNetCore.Caching;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.Common.Client;
using System.Linq;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.Client.AspNetCore.Controllers;

/// <summary>
/// This controller serves the <see cref="ClientSettings"/> which the Angular application will use to find the correct API endpoints.
/// </summary>
[Route("[controller]")]
public class SettingsController : RestControllerBase
{
    private readonly IOptionsMonitor<RestWorldClientOptions> _optionsMonitor;
    private RestWorldClientOptions _options;
    private readonly ServiceEndpointResolver _serviceEndpointResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsController"/> class.
    /// </summary>
    /// <param name="optionsMonitor">RestWorldOptions from the app config.</param>
    /// <param name="serviceEndpointResolver">The resolver to overwrite API URLs defined in the ClientSetting with the ones defined through Aspire Service Discovery.</param>
    /// <param name="resourceFactory">The resource factory.</param>
    /// <param name="cache">The cache.</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    public SettingsController(
        IOptionsMonitor<RestWorldClientOptions> optionsMonitor,
        ServiceEndpointResolver serviceEndpointResolver,
        IODataResourceFactory resourceFactory,
        ICacheHelper cache)
        : base(resourceFactory, cache)
    {
        _optionsMonitor = optionsMonitor ?? throw new System.ArgumentNullException(nameof(optionsMonitor));
        _options = _optionsMonitor.CurrentValue;
        _optionsMonitor.OnChange(opt => _options = opt);

        _serviceEndpointResolver = serviceEndpointResolver ?? throw new System.ArgumentNullException(nameof(serviceEndpointResolver));
    }
    /// <summary>
    /// Gets the <see cref="ClientSettings"/> which the Angular application will use to find the correct API endpoints.
    /// </summary>
    [HttpGet("")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    [ProducesResponseType(200)]
    public async ValueTask<ActionResult<Resource<ClientSettings?>>> GetAsync(CancellationToken cancellationToken)
    {
        var clientSettings = _options.ClientSettings;

        await ApplyServiceDiscovery(clientSettings, cancellationToken);

        var resource = Cache.GetOrCreateWithoutUser("ClientSettings", nameof(CachingOptions.Get), _ => ResourceFactory.CreateForEndpoint(clientSettings));

        return resource;
    }

    private async ValueTask ApplyServiceDiscovery(ClientSettings? clientSettings, CancellationToken cancellationToken)
    {
        if (clientSettings is null)
            return;

        var tasks = clientSettings.ApiUrls.Select(api => DiscoverApiUrl(api, cancellationToken));

        foreach (var task in tasks)
            await task;
    }

    private async ValueTask DiscoverApiUrl(ApiUrl api, CancellationToken cancellationToken)
    {
        if (api.Name is null)
            return;

        var endpoints = await _serviceEndpointResolver.GetEndpointsAsync("https+http://" + api.Name, cancellationToken);
        if (endpoints.Endpoints.Count > 0)
        {
            // A check to filter only for endpoints which are UrlEndpoint would be better, but that class is internal.
            // See https://github.com/dotnet/aspire/issues/4224
            var endpoint = endpoints.Endpoints[0];
            var uri = endpoint.ToString();
            api.Url = uri;
        }
    }
}
