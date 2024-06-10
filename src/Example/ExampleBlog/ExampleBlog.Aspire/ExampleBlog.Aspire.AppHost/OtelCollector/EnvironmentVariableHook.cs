using Aspire.Hosting.Lifecycle;
using Microsoft.Extensions.Logging;

namespace Aspire.Hosting.OtelCollector;

public class EnvironmentVariableHook : IDistributedApplicationLifecycleHook
{
    private readonly ILogger<EnvironmentVariableHook> _logger;

    public EnvironmentVariableHook(ILogger<EnvironmentVariableHook> logger)
    {
        _logger = logger;
    }
    public Task AfterEndpointsAllocatedAsync(DistributedApplicationModel appModel, CancellationToken cancellationToken)
    {
        var resources = appModel.GetProjectResources();
        var collectorResource = appModel.Resources.OfType<CollectorResource>().FirstOrDefault();

        if (collectorResource == null)
        {
            _logger.LogWarning("No collector resource found");
            return Task.CompletedTask;
        }

        var grpcEndpoint = collectorResource!.GetEndpoint(collectorResource!.GRPCEndpoint.EndpointName);
        var httpEndpoint = collectorResource!.GetEndpoint(collectorResource!.HTTPEndpoint.EndpointName);
        if (grpcEndpoint is null && httpEndpoint is null)
        {
            _logger.LogWarning("No endpoint for the collector");
            return Task.CompletedTask;
        }

        if (resources.Count() == 0)
            _logger.LogInformation("No resources to add Environment Variables to");

        foreach (var resourceItem in resources)
        {
            _logger.LogDebug($"Forwarding Telemetry for {resourceItem.Name} to the collector");
            if (resourceItem == null) continue;

            resourceItem.Annotations.Add(new EnvironmentCallbackAnnotation((context) =>
            {
                if (context.EnvironmentVariables.ContainsKey("OTEL_EXPORTER_OTLP_ENDPOINT"))
                    context.EnvironmentVariables.Remove("OTEL_EXPORTER_OTLP_ENDPOINT");
                context.EnvironmentVariables.Add("OTEL_EXPORTER_OTLP_ENDPOINT", grpcEndpoint.Url);
                context.EnvironmentVariables.Add("OTEL_EXPORTER_OTLP_ENDPOINT_HTTP", httpEndpoint.Url);
            }));
        }

        return Task.CompletedTask;
    }
}