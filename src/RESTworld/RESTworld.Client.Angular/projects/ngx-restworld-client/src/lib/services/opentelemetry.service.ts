import { Injectable } from "@angular/core";
import { registerInstrumentations } from '@opentelemetry/instrumentation';
import { AlwaysOffSampler, AlwaysOnSampler, BatchSpanProcessor, ConsoleSpanExporter, ParentBasedSampler, SimpleSpanProcessor, TraceIdRatioBasedSampler, WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { SettingsService } from "./settings.service";
import { ZoneContextManager } from "@opentelemetry/context-zone";
import { InstrumentationConfigMap, getWebAutoInstrumentations } from "@opentelemetry/auto-instrumentations-web";
import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-http";
import { CompressionAlgorithm } from '@opentelemetry/otlp-exporter-base';

/**
 * This service is responsible for setting up OpenTelemetry.
 * It will initialize the OpenTelemetry SDK and register the instrumentations.
 * It will also set up the exporters and samplers.
 *
 * The settings for the OpenTelemetry SDK are loaded from the {@link SettingsService}.
 *
 * @remarks
 * This service is automatically initialized by the `APP_INITIALIZER` in the {@link RestworldClientModule}.
 */
@Injectable({
    providedIn: "root"
})
export class OpenTelemetryService {

    private readonly _defaultInputConfigs: InstrumentationConfigMap = {
        "@opentelemetry/instrumentation-xml-http-request": {
            propagateTraceHeaderCorsUrls: /.*/,
            applyCustomAttributesOnSpan: (span, xhr) => {
                span.setAttribute("client.address", window.location.href);
                span.setAttribute("client.port", window.location.port);
                span.setAttribute("browser.language", navigator.language);
                if (URL.canParse(xhr.responseURL)) {
                    const url = new URL(xhr.responseURL);
                    span.setAttribute("url.domain", url.hostname);
                    span.setAttribute("url.full", url.href);
                    span.setAttribute("url.path", url.pathname);
                    span.setAttribute("url.port", url.port);
                    span.setAttribute("url.scheme", url.protocol.replace(":", ""));
                    if (url.search)
                        span.setAttribute("url.query", url.search.replace("?", ""));

                    if ("name" in span && typeof span.name === "string") {
                        span.updateName(span.name + " " + (url.pathname.startsWith("/") ? url.pathname.substring(1) : url.pathname));
                    }
                }
            }
        },
        "@opentelemetry/instrumentation-fetch": {
            propagateTraceHeaderCorsUrls: /.*/,
            applyCustomAttributesOnSpan: (span, request) => {
                span.setAttribute("client.address", window.location.href);
                span.setAttribute("client.port", window.location.port);
                span.setAttribute("browser.language", navigator.language);
                if ("url" in request && typeof request.url === "string" && URL.canParse(request.url)) {
                    const url = new URL(request.url);
                    span.setAttribute("url.domain", url.hostname);
                    span.setAttribute("url.full", url.href);
                    span.setAttribute("url.path", url.pathname);
                    span.setAttribute("url.port", url.port);
                    span.setAttribute("url.scheme", url.protocol.replace(":", ""));
                    if (url.search)
                        span.setAttribute("url.query", url.search.replace("?", ""));

                    if ("name" in span && typeof span.name === "string") {
                        span.updateName(span.name + " " + (url.pathname.startsWith("/") ? url.pathname.substring(1) : url.pathname));
                    }
                }
            }
        },
        "@opentelemetry/instrumentation-user-interaction": {
            enabled: false,
            shouldPreventSpanCreation: (eventType, element, span) => {
                const shouldCreate =
                    this._lastLocation !== window.location.href ||
                    Date.now() - this._lastTime > 1000 ||
                    this._lastElement !== element ||
                    //(eventType === "click" && (element.tagName === "A" || element.tagName === "Button")) ||
                    (eventType === "submit");

                if (shouldCreate) {
                    this._lastLocation = window.location.href;
                    this._lastTime = Date.now();
                    this._lastElement = element;
                }

                return !shouldCreate;
            }
        }
    };

    private _lastElement: HTMLElement | null = null;
    private _lastTime: number = 0;
    private _lastLocation: string | null = null;

    constructor(private readonly _settingsService: SettingsService) {
    }

    async initialize(configureOptions?: InstrumentationConfigMap | ((inputConfigs: InstrumentationConfigMap) => InstrumentationConfigMap)): Promise<void> {
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

            const traceExporterEndpoint = exporterEndpoint.endsWith("/") ? exporterEndpoint + "v1/traces" : exporterEndpoint + "/v1/traces";

            const otlpExporterConfiguration = {
                url: traceExporterEndpoint,
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

        // apply given options
        let config;
        if (typeof configureOptions === 'function') {
            config = configureOptions(this._defaultInputConfigs);
        }
        else if (typeof configureOptions === 'object') {
            config = configureOptions;
        }
        else {
            config = this._defaultInputConfigs;
        }

        // Registering instrumentations
        registerInstrumentations({
            instrumentations: [
                getWebAutoInstrumentations(config),
            ]
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
