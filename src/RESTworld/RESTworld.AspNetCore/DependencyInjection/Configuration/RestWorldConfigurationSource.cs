using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ServiceDiscovery;
using System;

namespace RESTworld.AspNetCore.DependencyInjection.Configuration;

/// <summary>
/// A source for <see cref="RestWorldClientConfigurationProvider"/>.
/// </summary>
public class RestWorldConfigurationSource : IConfigurationSource
{
    private readonly IConfiguration _rootConfiguration;
    private readonly ServiceEndpointResolver _serviceEndpointResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestWorldConfigurationSource"/> class.
    /// </summary>
    /// <param name="rootConfiguration"></param>
    /// <param name="serviceEndpointResolver"></param>
    /// <exception cref="ArgumentNullException"></exception>
    public RestWorldConfigurationSource(IConfiguration rootConfiguration, ServiceEndpointResolver serviceEndpointResolver)
    {
        _rootConfiguration = rootConfiguration ?? throw new ArgumentNullException(nameof(rootConfiguration));
        _serviceEndpointResolver = serviceEndpointResolver ?? throw new ArgumentNullException(nameof(serviceEndpointResolver));
    }
    /// <inheritdoc/>

    public IConfigurationProvider Build(IConfigurationBuilder builder)
    {
        return new RestWorldClientConfigurationProvider(_rootConfiguration, _serviceEndpointResolver);
    }
}
