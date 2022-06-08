using HAL.Client.Net;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using RESTworld.Common.Client;

namespace RESTworld.Client.Net
{
    /// <summary>
    /// Contains extension methods for <see cref="IServiceCollection"/>.
    /// </summary>
    public static class ServiceCollectionExtensions
    {
        /// <summary>
        /// Adds an <see cref="IRestWorldClientCollection"/> to the <see cref="IServiceCollection"/>.
        /// </summary>
        /// <param name="services">The services to add it to.</param>
        /// <param name="configuration">The configuration to read the <see cref="RestWorldClientOptions"/> from.</param>
        /// <param name="clientConfigurations">An optional dictionary with client names and optional configuration actions. You can use these to inject something into the underlying <see cref="HttpClient"/>s like authorization or retry logic.</param>
        /// <returns>The <paramref name="services"/>.</returns>
        public static IServiceCollection AddRestWorldClients(this IServiceCollection services, IConfiguration configuration, IDictionary<string, Action<IServiceProvider, HttpClient>?>? clientConfigurations = null)
        {
            var configSection = configuration.GetSection("RESTworld");
            services.Configure<RestWorldClientOptions>(configSection);

            if (clientConfigurations is null)
            {
                clientConfigurations = new Dictionary<string, Action<IServiceProvider, HttpClient>?>();
                var options = new RestWorldClientOptions();
                configSection.Bind(options);
                if (options.ClientSettings?.ApiUrls is not null)
                {
                    foreach (var api in options.ClientSettings.ApiUrls)
                    {
                        if (api.Name is not null)
                            clientConfigurations[api.Name] = null;
                    }
                }
            }

            services.AddHalClientFactoy(clientConfigurations);

            services.AddSingleton(RestWorldClientCollectionFactory);

            return services;
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
}
