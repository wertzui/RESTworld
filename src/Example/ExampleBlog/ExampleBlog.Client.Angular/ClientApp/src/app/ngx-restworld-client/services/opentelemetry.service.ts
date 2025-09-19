import { Injectable } from "@angular/core";
import { NavigationEnd, Router } from "@angular/router";
import { HTTP_INTERCEPTORS } from "@angular/common/http";
import { AlwaysOffSampler, AlwaysOnSampler, BatchSpanProcessor, ConsoleSpanExporter, ParentBasedSampler, SimpleSpanProcessor, SpanProcessor, TraceIdRatioBasedSampler, WebTracerProvider } from '@opentelemetry/sdk-trace-web';
import { SettingsService } from "./settings.service";
import { ZoneContextManager } from "@opentelemetry/context-zone-peer-dep";
import { OTLPTraceExporter } from "@opentelemetry/exporter-trace-otlp-proto";
import { CompressionAlgorithm } from '@opentelemetry/otlp-exporter-base';
import { ClientSettings } from "../models/client-settings";
import { resourceFromAttributes } from "@opentelemetry/resources";
import { context, trace, Span, SpanKind, Tracer } from '@opentelemetry/api';
import { filter } from 'rxjs/operators';
import { ATTR_SERVICE_NAME } from "@opentelemetry/semantic-conventions";

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
    private _tracer: any;
    private _activeNavigationSpan: Span | null = null;
    private _lastNavigationPath: string | null = null;

    constructor(
        private readonly _settingsService: SettingsService,
        private readonly _router: Router
    ) {
        // Listen for navigation events to create parent spans
        this._router.events
            .pipe(filter(event => event instanceof NavigationEnd))
            .subscribe((event: any) => {
                this.createNavigationSpan((event as NavigationEnd).urlAfterRedirects);
            });

        // Listen for beforeunload event to end the span when user leaves the application
        window.addEventListener('beforeunload', () => {
            this.endCurrentNavigationSpan();
        });
    }

    /**
     * Sets the current span as active in the global context
     * This ensures that new spans created after this will automatically
     * become child spans of this active span
     */
    private activateNavigationContext(): void {
        if (this._activeNavigationSpan) {
            // Make this span the active span in the current context
            const activeContext = trace.setSpan(context.active(), this._activeNavigationSpan);

            // Use ZoneContextManager to persist this context across async operations
            // This is important for Angular applications that use zones
            context.with(activeContext, () => {
                // The context remains active for all operations within this callback
                // and for all asynchronous operations started within this zone

                // Set the context as a property in the window for access by other parts of the application
                // This helps ensure the context is available for all operations
                (window as any).__navigationContext = activeContext;
            });
        }
    }

    /**
     * Creates a new span to track a navigation event.
     * This will be the parent span for any HTTP requests made during this navigation.
     * Only creates a new span if the path has changed from the last navigation.
     * @param url The URL being navigated to
     */
    private createNavigationSpan(url: string): void {
        // Parse the URL to extract route information
        let routePath = 'unknown';
        try {
            const urlObj = new URL(url, window.location.origin);
            routePath = urlObj.pathname;
        } catch (e) {
            routePath = url; // Use the raw URL if parsing fails
        }

        // If the path hasn't changed, don't create a new span
        if (this._lastNavigationPath === routePath) {
            return;
        }

        // Update the last navigation path
        this._lastNavigationPath = routePath;

        // End the previous span if it exists
        if (this._activeNavigationSpan) {
            this._activeNavigationSpan.end();
            this._activeNavigationSpan = null;
        }

        if (!this._tracer) {
            return; // Not initialized yet
        }

        // Create a new span for this navigation
        this._activeNavigationSpan = this._tracer.startSpan(
            `Navigation to ${routePath}`,
            {
                kind: SpanKind.INTERNAL,
                attributes: {
                    'component': 'router',
                    'route.path': routePath,
                    'client.address': window.location.href,
                    'client.port': window.location.port,
                    'browser.language': navigator.language
                }
            }
        );

        // Activate this navigation span as the current context
        this.activateNavigationContext();
    }

    async initialize(): Promise<void> {
        await this._settingsService.ensureInitialized();

        const clientSettings = this._settingsService.settings;
        if (!clientSettings)
            throw new Error('Client Settings are not loaded yet.');
        if (!clientSettings.extensions)
            throw new Error('Client Settings do not contain extensions.');

        const sampler = OpenTelemetryService.GetSampler(clientSettings);
        const spanProcessors = OpenTelemetryService.GetSpanProcessors(clientSettings);
        const resource = OpenTelemetryService.GetResource(clientSettings);

        const provider = new WebTracerProvider({
            sampler: sampler,
            spanProcessors: spanProcessors,
            resource: resource,
        });

        provider.register({
            // Changing default contextManager to use ZoneContextManager - supports asynchronous operations - optional
            contextManager: new ZoneContextManager(),
        });

        // Get a tracer from the provider
        this._tracer = provider.getTracer('angular-navigation-tracer');

        // Create the initial navigation span based on the current URL
        if (this._router && this._router.url) {
            this.createNavigationSpan(this._router.url);
        }
    }

    /**
     * Ends the current navigation span if one exists.
     * This can be called when the user is leaving the application or when
     * special navigation events happen that should terminate the current span.
     */
    public endCurrentNavigationSpan(): void {
        if (this._activeNavigationSpan) {
            this._activeNavigationSpan.end();
            this._activeNavigationSpan = null;
            this._lastNavigationPath = null; // Reset the last navigation path when ending the span manually
        }
    }

    private static GetResource(clientSettings: ClientSettings) {
        const serviceName = clientSettings.extensions["OTEL_SERVICE_NAME"];
        const resourceAttributes = clientSettings.extensions["OTEL_RESOURCE_ATTRIBUTES"];

        const resource = resourceFromAttributes({});

        if (resourceAttributes) {
            resourceAttributes
                .split(",")
                .map((attribute: string) => {
                    const [key, value] = attribute.split("=");
                    resource.attributes[key] = value;
                });
        }

        if (serviceName)
            resource.attributes[ATTR_SERVICE_NAME] = serviceName;

        return resource;
    }

    private static GetSpanProcessors(clientSettings: ClientSettings): SpanProcessor[] {
        const exporterEndpoint = clientSettings.extensions["OTEL_EXPORTER_OTLP_ENDPOINT"];
        const exporterHeaders = clientSettings.extensions["OTEL_EXPORTER_OTLP_HEADERS"];

        if (!exporterEndpoint)
            return[new SimpleSpanProcessor(new ConsoleSpanExporter())];
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

            return[new BatchSpanProcessor(new OTLPTraceExporter(otlpExporterConfiguration))];
        }
    }

    private static GetSampler(clientSettings: ClientSettings) {
        const tracesSampler = clientSettings.extensions["OTEL_TRACES_SAMPLER"];
        const tracesSamplerArg = clientSettings.extensions["OTEL_TRACES_SAMPLER_ARG"];

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

    /**
     * Gets the active context that contains the current navigation span.
     * This is used by the HttpInterceptor to ensure HttpClient requests are
     * properly associated with the current navigation span.
     * @returns The active context, or null if no navigation span exists
     */
    public getActiveContext(): any {
        if (this._activeNavigationSpan) {
            // Create a context with the active navigation span
            return trace.setSpan(context.active(), this._activeNavigationSpan);
        }

        return null;
    }

    /**
     * Gets the OpenTelemetry tracer instance for creating spans
     * @returns The tracer instance or undefined if not initialized
     */
    public getTracer(): Tracer {
        return this._tracer;
    }
}
