using HAL.AspNetCore.OData.Abstractions;
using HAL.Common;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Options;
using RESTworld.AspNetCore.Caching;
using RESTworld.AspNetCore.Controller;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.Common.Client;
using System.Threading;

namespace RESTworld.Client.AspNetCore.Controllers;

/// <summary>
/// This controller serves the <see cref="ClientSettings"/> which the Angular application will use to find the correct API endpoints.
/// </summary>
[Route("[controller]")]
public class SettingsController : RestControllerBase
{
    private readonly IOptionsMonitor<RestWorldClientOptions> _optionsMonitor;
    private RestWorldClientOptions _options;

    /// <summary>
    /// Initializes a new instance of the <see cref="SettingsController"/> class.
    /// </summary>
    /// <param name="optionsMonitor">RestWorldOptions from the app config.</param>
    /// <param name="resourceFactory">The resource factory.</param>
    /// <param name="cache">The cache.</param>
    /// <exception cref="System.ArgumentNullException"></exception>
    public SettingsController(
        IOptionsMonitor<RestWorldClientOptions> optionsMonitor,
        IODataResourceFactory resourceFactory,
        ICacheHelper cache)
        : base(resourceFactory, cache)
    {
        _optionsMonitor = optionsMonitor ?? throw new System.ArgumentNullException(nameof(optionsMonitor));
        _options = _optionsMonitor.CurrentValue;
        _optionsMonitor.OnChange(opt => _options = opt);
    }
    /// <summary>
    /// Gets the <see cref="ClientSettings"/> which the Angular application will use to find the correct API endpoints.
    /// </summary>
    [HttpGet("")]
    [ApiConventionMethod(typeof(DefaultApiConventions), nameof(DefaultApiConventions.Get))]
    [ProducesResponseType(200)]
    public ActionResult<Resource<ClientSettings?>> Get(CancellationToken cancellationToken)
    {
        var clientSettings = _options.ClientSettings;

        var resource = Cache.GetOrCreateWithoutUser("ClientSettings", nameof(CachingOptions.Get), _ => ResourceFactory.CreateForEndpoint(clientSettings));

        return resource;
    }
}
