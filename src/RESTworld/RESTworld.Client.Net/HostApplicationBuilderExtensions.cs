using HAL.Client.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RESTworld.Client.Net;
using RESTworld.Common.Client;

namespace Microsoft.Extensions.DependencyInjection;

/// <summary>
/// Contains extension methods for <see cref="IHostApplicationBuilder"/>.
/// </summary>
public static class HostApplicationBuilderExtensions
{
    /// <summary>
    /// Adds an <see cref="IRestWorldClientCollection"/> to the <see cref="IHostApplicationBuilder"/>.
    /// </summary>
    /// <param name="builder">The builder to add the RESTworld clients to.</param>
    /// <param name="clientConfigurations">An optional dictionary with client names and optional configuration actions. You can use these to inject something into the underlying <see cref="HttpClient"/>s like authorization or retry logic.</param>
    /// <returns>The <paramref name="builder"/>.</returns>
    public static IHostApplicationBuilder AddRestWorldClients(this IHostApplicationBuilder builder, IDictionary<string, Action<IServiceProvider, HttpClient>?>? clientConfigurations = null)
    {
        ArgumentNullException.ThrowIfNull(builder);

        var configSection = builder.Configuration.GetSection("RESTworld");

        if (clientConfigurations is null)
            clientConfigurations = new Dictionary<string, Action<IServiceProvider, HttpClient>?>();

        var options = configSection.Get<RestWorldClientOptions>();
        if (options?.ClientSettings?.ApiUrls is not null)
        {
            foreach (var api in options.ClientSettings.ApiUrls)
            {
                if (api.Name is not null)
                    clientConfigurations.TryAdd(api.Name, null);
            }
        }

        builder.Services.AddHalClientFactoy(clientConfigurations);

        builder.Services.AddSingleton(RestWorldClientCollectionFactory);

        return builder;
    }

    private static IRestWorldClientCollection RestWorldClientCollectionFactory(IServiceProvider provider)
    {
        var options = provider.GetRequiredService<IOptions<RestWorldClientOptions>>();
        var halClientFactory = provider.GetRequiredService<IHalClientFactory>();
        var logger = provider.GetRequiredService<ILogger<RestWorldClient>>();

        try
        {
            var collection = RestWorldClientCollection.CreateAsync(options, halClientFactory, logger).GetAwaiter().GetResult();

            return collection;
        }
        catch (Exception e)
        {
            throw new Exception("Unable to create a RestWorldClientCollection in the DI Container. See the InnerException for details.", e);
        }
    }
}