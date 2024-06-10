using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.ServiceDiscovery;
using RESTworld.AspNetCore.DependencyInjection;
using RESTworld.AspNetCore.DependencyInjection.Configuration;
using RESTworld.Common.Client;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods for <see cref="IHostBuilder"/>.
/// </summary>
public static class HostBuilderConfigurationExtensions
{
    /// <summary>
    /// Adds <see cref="RestWorldClientOptions"/>, <see cref="RestWorldClientOptions"/> as well as Service Discovery to the application.
    /// </summary>
    /// <param name="builder"></param>
    /// <returns></returns>
    public static IHostApplicationBuilder AddRestWorldOptions(this IHostApplicationBuilder builder)
    {
        // This adds the ServiceEndpointResolver to the DI container.
        builder.Services.AddServiceDiscovery();

        // Add Values from service discovery and open telemetry to the client options.
        var provider = builder.Services.BuildServiceProvider();
        try
        {
            var endpointResolver = provider.GetRequiredService<ServiceEndpointResolver>();
            builder.Configuration.Sources.Add(new RestWorldConfigurationSource(builder.Configuration, endpointResolver));

            // Both options are stored under the same section in the appsettings.json file.
            var restWorldSection = builder.Configuration.GetSection("RESTworld");
            builder.Services.Configure<RestWorldOptions>(restWorldSection);
            builder.Services.Configure<RestWorldClientOptions>(restWorldSection);
        }
        finally
        {
            provider?.DisposeAsync().GetAwaiter().GetResult();
        }

        return builder;
    }
}
