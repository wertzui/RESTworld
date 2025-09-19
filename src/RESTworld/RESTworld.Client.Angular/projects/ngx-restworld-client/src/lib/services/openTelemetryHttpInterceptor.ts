import { type HttpInterceptor, type HttpRequest, type HttpHandler, type HttpEvent, HttpEventType, type HttpResponse, HTTP_INTERCEPTORS } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { context, type Attributes, SpanKind, trace, propagation, SpanStatusCode } from "@opentelemetry/api";
import { ATTR_HTTP_REQUEST_METHOD, ATTR_URL_FULL, ATTR_URL_PATH, ATTR_URL_QUERY, ATTR_URL_SCHEME, ATTR_CLIENT_ADDRESS, ATTR_CLIENT_PORT, ATTR_HTTP_REQUEST_HEADER, ATTR_SERVICE_NAME, ATTR_HTTP_RESPONSE_STATUS_CODE, ATTR_HTTP_RESPONSE_HEADER } from "@opentelemetry/semantic-conventions";
import { type Observable, tap, finalize } from "rxjs";
import { OpenTelemetryService } from "./opentelemetry.service";

/**
 * This is an HttpInterceptor that ensures all HttpClient requests
 * are properly associated with the current navigation span.
 */

@Injectable()
export class OpenTelemetryHttpInterceptor implements HttpInterceptor {
    constructor(private readonly _telemetryService: OpenTelemetryService) { }

    intercept(request: HttpRequest<any>, next: HttpHandler): Observable<HttpEvent<any>> {
        // Get the active context from the OpenTelemetryService
        const activeContext = this._telemetryService.getActiveContext();

        // If we have an active navigation context, create a child span for this HTTP request
        if (activeContext && this._telemetryService.getTracer()) {
            // Execute within the active context to ensure proper parenting
            return context.with(activeContext, () => {
                // Extract span information from the request
                const url = new URL(request.url, window.location.origin);
                const method = request.method;
                const spanName = `${method} ${url.pathname}`;

                // Get tracer from the telemetry service to create a span
                const tracer = this._telemetryService.getTracer();

                // Create attributes for the span, handling null values from headers
                const contentLength = request.headers.get('Content-Length');
                const contentType = request.headers.get('Content-Type');

                const attributes: Attributes = {
                    [ATTR_HTTP_REQUEST_METHOD]: method,
                    [ATTR_URL_FULL]: request.url,
                    [ATTR_URL_PATH]: url.pathname,
                    [ATTR_URL_QUERY]: url.search.replace('?', ''),
                    [ATTR_URL_SCHEME]: url.protocol.replace(':', ''),
                    "url.host": url.host,
                    "http.user_agent": navigator.userAgent,
                    [ATTR_CLIENT_ADDRESS]: window.location.href,
                    [ATTR_CLIENT_PORT]: window.location.port,
                    [ATTR_HTTP_REQUEST_HEADER("accept")]: request.headers.get('Accept') ?? undefined,
                    [ATTR_HTTP_REQUEST_HEADER("accept-language")]: request.headers.get('Accept-Language') ?? undefined,
                    "http.request.body.size": contentLength ?? undefined,
                    [ATTR_HTTP_REQUEST_HEADER("content-type")]: contentType ?? undefined,
                    [ATTR_SERVICE_NAME]: 'ngx-restworld-client'
                };

                // Start a new span for this HTTP request
                const span = tracer.startSpan(spanName, {
                    kind: SpanKind.CLIENT,
                    attributes
                });

                // Create a context with the current span
                const spanContext = trace.setSpan(context.active(), span);

                // Create a carrier object for W3C trace context headers
                const carrier: Record<string, string> = {};

                // Inject trace context headers into the carrier
                propagation.inject(spanContext, carrier);

                // Clone the request with the added W3C trace context headers
                let headers = request.headers;
                Object.entries(carrier).forEach(([key, value]) => {
                    headers = headers.set(key, value);
                });

                // Create the traced request with the W3C headers
                const tracedRequest = request.clone({ headers });

                // Use with() to make this span active during request execution
                return context.with(spanContext, () => {
                    // Handle the request and observe the response
                    return next.handle(tracedRequest).pipe(
                        // Use operators to capture response or error before completing the span
                        tap({
                            next: (event: HttpEvent<any>) => {
                                if (event.type === HttpEventType.Response) {
                                    const response = event as HttpResponse<any>;
                                    const responseAttrs: Record<string, string | number | boolean> = {
                                        [ATTR_HTTP_RESPONSE_STATUS_CODE]: response.status
                                    };

                                    // Only add response header values if they exist
                                    const respContentLength = response.headers.get('Content-Length');
                                    const respContentType = response.headers.get('Content-Type');

                                    if (respContentLength) responseAttrs["http.response.body.size"] = respContentLength;
                                    if (respContentType) responseAttrs[ATTR_HTTP_RESPONSE_HEADER("content-type")] = respContentType;

                                    // Add response attributes to the span
                                    span.setAttributes(responseAttrs);
                                    span.setStatus({
                                        code: response.ok ? SpanStatusCode.OK : SpanStatusCode.ERROR
                                    });

                                    // End the span when we get the full response
                                    span.end();
                                }
                            },
                            error: (error: Error) => {
                                // Record the error and end the span
                                span.setStatus({
                                    code: SpanStatusCode.ERROR,
                                    message: error.message || 'Http request failed'
                                });
                                span.recordException(error);
                                span.end();
                            }
                        }),
                        // Make sure the span ends even if the observable completes without going through our tap handlers
                        finalize(() => {
                            if (span) {
                                // Only end if not already ended in the tap handlers
                                try {
                                    span.end();
                                } catch (e) {
                                    // Ignore errors from ending an already-ended span
                                }
                            }
                        })
                    );
                });
            });
        }

        // No active context, just proceed with the request without tracing
        return next.handle(request);
    }
}

/**
 * Provider for the OpenTelemetry HTTP interceptor.
 * This interceptor ensures that HttpClient requests are properly
 * associated with the current navigation span.
 */
export const OPENTELEMETRY_HTTP_INTERCEPTOR_PROVIDER = {
    provide: HTTP_INTERCEPTORS,
    useClass: OpenTelemetryHttpInterceptor,
    multi: true
};
