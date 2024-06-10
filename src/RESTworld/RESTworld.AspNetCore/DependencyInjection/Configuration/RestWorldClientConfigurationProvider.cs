using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.ServiceDiscovery;
using RESTworld.Common.Client;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace RESTworld.AspNetCore.DependencyInjection.Configuration;

/// <summary>
/// Provides the configuration for the RESTworld client.
/// Adds values from Service discovery and OpenTelemetry to the client options.
/// </summary>
public class RestWorldClientConfigurationProvider : ConfigurationProvider
{
    private const string clientConfigSection = nameof(ClientSettings);
    private const string configSection = "RESTworld";
    private readonly IConfigurationSection _restWorldConfigSection;
    private readonly IConfiguration _rootConfiguration;
    private readonly ServiceEndpointResolver _serviceEndpointResolver;

    /// <summary>
    /// Initializes a new instance of the <see cref="RestWorldClientConfigurationProvider"/> class.
    /// </summary>
    /// <param name="rootConfiguration"></param>
    /// <param name="serviceEndpointResolver"></param>
    /// <exception cref="System.ArgumentNullException"></exception>
    public RestWorldClientConfigurationProvider(IConfiguration rootConfiguration, ServiceEndpointResolver serviceEndpointResolver)
    {
        _rootConfiguration = rootConfiguration ?? throw new System.ArgumentNullException(nameof(rootConfiguration));
        _serviceEndpointResolver = serviceEndpointResolver ?? throw new System.ArgumentNullException(nameof(serviceEndpointResolver));
        _restWorldConfigSection = _rootConfiguration.GetSection(configSection);
    }

    /// <inheritdoc/>
    public override void Load()
    {
        // Add the open telemetry configuration to the client extensions
        AddConfigToClientExtensions("OTEL_EXPORTER_OTLP_HEADERS");
        AddConfigToClientExtensions("OTEL_SERVICE_NAME");
        AddConfigToClientExtensions("OTEL_TRACES_SAMPLER");
        AddConfigToClientExtensions("OTEL_TRACES_SAMPLER_ARG");
        AddConfigToClientExtensions("OTEL_RESOURCE_ATTRIBUTES");

        // There is a special case for the endpoint, as it can be either HTTP or gRPC.
        // The Backend for the frontend needs the GRPC endpoint, while the frontend needs the HTTP endpoint.
        // Only OTEL_EXPORTER_OTLP_ENDPOINT_HTTP is defined in the spec at https://opentelemetry.io/docs/languages/sdk-configuration/otlp-exporter/.
        // However the angular frontend runs in the browser and cannot connect to gRPC, so we need to provide the HTTP endpoint as well.
        // OTEL_EXPORTER_OTLP_ENDPOINT_HTTP is a custom name that we came up with to distinguish between the two.
        if (_rootConfiguration.GetValue<string?>("OTEL_EXPORTER_OTLP_ENDPOINT_HTTP") is not null)
            AddConfigToClientExtensions("OTEL_EXPORTER_OTLP_ENDPOINT", "OTEL_EXPORTER_OTLP_ENDPOINT_HTTP");
        else
            AddConfigToClientExtensions("OTEL_EXPORTER_OTLP_ENDPOINT");

        // Set the API URLs if they have been overridden by the service discovery (Aspire)
        var options = _restWorldConfigSection.Get<RestWorldClientOptions>();
        if (options?.ClientSettings is not null)
            ApplyServiceDiscovery(options.ClientSettings, CancellationToken.None).GetAwaiter().GetResult();
    }

    private void AddConfigToClientExtensions(string configKey, string? environmentVariableToReadFrom = null)
    {
        environmentVariableToReadFrom ??= configKey;
        _restWorldConfigSection[$"{nameof(RestWorldClientOptions.ClientSettings)}:{nameof(ClientSettings.Extensions)}:{configKey}"] = _rootConfiguration.GetValue<string?>(environmentVariableToReadFrom);
    }

    private void SetApiUrl(int apiIndex, string apiUrl)
    {
        _restWorldConfigSection[$"{nameof(RestWorldClientOptions.ClientSettings)}:{nameof(ClientSettings.ApiUrls)}:{apiIndex}:{nameof(ApiUrl.Url)}"] = apiUrl;
    }

    private async ValueTask ApplyServiceDiscovery(ClientSettings? clientSettings, CancellationToken cancellationToken)
    {
        if (clientSettings is null)
            return;

        var tasks = clientSettings.ApiUrls.Select((api, index) => DiscoverApiUrl(api, index, cancellationToken));

        foreach (var task in tasks)
            await task;
    }

    private async ValueTask DiscoverApiUrl(ApiUrl api, int index, CancellationToken cancellationToken)
    {
        if (api.Name is null)
            return;

        var endpoints = await _serviceEndpointResolver.GetEndpointsAsync("https+http://" + api.Name, cancellationToken);
        // A check to filter only for endpoints which are UrlEndpoint would be better, but
        // that class is internal. See https://github.com/dotnet/aspire/issues/4224
        if (endpoints.Endpoints.Count <= 0)
            return;

        var endpoint = endpoints.Endpoints[0].EndPoint;

        try
        {
            if (endpoint.AddressFamily == AddressFamily.Unspecified)
                return;
        }
        catch
        {
            // We expect a NotImplementedException if the endpoint is an URL.
        }

        var uri = endpoint.ToString();
        if (uri is null || (!uri.StartsWith("http://") && !uri.StartsWith("https://")))
            return;

        SetApiUrl(index, uri);
    }
}