import { Injectable } from "@angular/core";
import { registerInstrumentations } from '@opentelemetry/instrumentation';
import { AlwaysOffSampler, AlwaysOnSampler, BatchSpanProcessor, ConsoleSpanExporter, ParentBasedSampler, SimpleSpanProcessor, TraceIdRatioBasedSampler, WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { SettingsService } from "./settings.service";
import { ZoneContextManager } from "@opentelemetry/context-zone";
import { getWebAutoInstrumentations } from "@opentelemetry/auto-instrumentations-web";
import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-http";
import { CompressionAlgorithm } from '@opentelemetry/otlp-exporter-base';

@Injectable({
  providedIn: "root"
})
export class OpenTelemetryService {
  constructor(private readonly _settingsService: SettingsService) {
  }

  async initialize(): Promise<void> {
    await this._settingsService.ensureInitialized();

    const clientSettings = this._settingsService.settings;
    if (!clientSettings)
      throw new Error('Settings are not loaded yet.');

    const exporterEndpoint = clientSettings.extensions["OTEL_EXPORTER_OTLP_ENDPOINT"];
    const exporterHeaders = clientSettings.extensions["OTEL_EXPORTER_OTLP_HEADERS"];
    const serviceName = clientSettings.extensions["OTEL_SERVICE_NAME"];
    const resourceAttributes = clientSettings.extensions["OTEL_RESOURCE_ATTRIBUTES"];
    const tracesSampler = clientSettings.extensions["OTEL_TRACES_SAMPLER"];
    const tracesSamplerArg = clientSettings.extensions["OTEL_TRACES_SAMPLER_ARG"];

    const provider = new WebTracerProvider({ sampler: OpenTelemetryService.GetSampler(tracesSampler, tracesSamplerArg) });

    if (!exporterEndpoint)
      provider.addSpanProcessor(new SimpleSpanProcessor(new ConsoleSpanExporter()));
    else {
      let headersObj = {};
      if (exporterHeaders) {
        headersObj = Object.fromEntries(
          exporterHeaders
            .split(",")
            .map((header: string) => {
              const [key, value] = header.split("=");
              return [key, value];
            }));
      }

      const otlpExporterConfiguration = {
        url: exporterEndpoint,
        headers: headersObj,
        compression: "gzip" as CompressionAlgorithm,
      };

      provider.addSpanProcessor(new BatchSpanProcessor(new OTLPTraceExporter(otlpExporterConfiguration)));
    }

    provider.register({
      // Changing default contextManager to use ZoneContextManager - supports asynchronous operations - optional
      contextManager: new ZoneContextManager(),
    });

    if (resourceAttributes) {
      resourceAttributes
        .split(",")
        .map((attribute: string) => {
          const [key, value] = attribute.split("=");
          provider.resource.attributes[key] = value;
        });
    }

    if (serviceName)
      provider.resource.attributes['service.name'] = serviceName;

    // Registering instrumentations
    registerInstrumentations({
      instrumentations: [
        getWebAutoInstrumentations()
      ],
    });
  }

  private static GetSampler(tracesSampler?: string, tracesSamplerArg?: string) {
    switch (tracesSampler) {
      case "always_on":
        return new AlwaysOnSampler();
      case "always_off":
        return new AlwaysOffSampler();
      case "traceidratio":
        const ratio = Number.parseFloat(tracesSamplerArg ?? "1");
        return new TraceIdRatioBasedSampler(ratio);
      case "parentbased_always_on":
        return new ParentBasedSampler({ root: new AlwaysOnSampler() });
      case "parentbased_always_off":
        return new ParentBasedSampler({ root: new AlwaysOffSampler() });
      case "parentbased_traceidratio":
        const ratio2 = Number.parseFloat(tracesSamplerArg ?? "1");
        return new ParentBasedSampler({ root: new TraceIdRatioBasedSampler(ratio2) });
      default:
        return new AlwaysOnSampler();
    }
  }
}
