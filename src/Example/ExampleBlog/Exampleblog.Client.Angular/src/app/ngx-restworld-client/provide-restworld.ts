import { EnvironmentProviders, inject, provideAppInitializer, type Provider, type Type } from "@angular/core";
import { provideSignalFormsConfig, type SignalFormsConfig } from "@angular/forms/signals";
import { NG_STATUS_CLASSES } from "@angular/forms/signals/compat";
import { OpenTelemetryService } from "./services/opentelemetry.service";
import { SettingsService } from "./services/settings.service";
import { AvatarGenerator } from "./services/avatar-generator";
import { NgHttpCachingStrategy, provideNgHttpCaching, withNgHttpCachingLocalStorage, type NgHttpCachingConfig } from "ng-http-caching";
import { OPENTELEMETRY_HTTP_INTERCEPTOR_PROVIDER } from "./services/openTelemetryHttpInterceptor";

/**
 * Povides the RESTworld functionality to the application.
 * This includes the RestWorldClientCollection, the SettingsService and the OpenTelemetryService.
 * You need to call this function in your main.ts in the providers array of the bootstrapApplication function.
 * @param ngHttpCachingConfig An optional configuration for caching http requests. Will be merged with the default configuration if provided.
 * The default configuration is this:
 * ```typescript
 * {
 *     allowedMethod: ["ALL"],
 *     checkResponseHeaders: true,
 *     cacheStrategy: NgHttpCachingStrategy.DISALLOW_ALL,
 *     store: withNgHttpCachingLocalStorage(),
 * }
 * ```
 * @param signalFormsConfig An optional configuration for Signal Forms (used by the `rw-signal-*` components,
 * e.g. `<rw-signal-form>`). Defaults to `{ classes: NG_STATUS_CLASSES }`, which makes Signal Forms apply the
 * same `.ng-touched`/`.ng-invalid`/`.ng-dirty`/`.ng-valid` classes to fields that Reactive Forms applies
 * automatically. Pass an empty object (`{}`) to opt out, or your own `classes` map to fully customize.
 */
export function provideRestWorld(ngHttpCachingConfig?: NgHttpCachingConfig, signalFormsConfig?: SignalFormsConfig): (EnvironmentProviders | Provider)[] {
    const defaultCachingConfig = {
            allowedMethod: ["ALL"],
            checkResponseHeaders: true,
            cacheStrategy: NgHttpCachingStrategy.DISALLOW_ALL,
            store: withNgHttpCachingLocalStorage(),
        }

    const mergedCachingConfig = { ...defaultCachingConfig, ...ngHttpCachingConfig };

    const restWorldInitializer = provideAppInitializer(async () => {
        const settingsService = inject(SettingsService);
        const opentelemetryService = inject(OpenTelemetryService);
        await settingsService.ensureInitialized();
        await opentelemetryService.initialize();
    });

    const cachingProviders = provideNgHttpCaching(mergedCachingConfig);

    const signalFormsProviders = provideSignalFormsConfig(signalFormsConfig ?? { classes: NG_STATUS_CLASSES });

    const allProviders = [
        restWorldInitializer,
        cachingProviders,
        signalFormsProviders,
        OPENTELEMETRY_HTTP_INTERCEPTOR_PROVIDER
    ];

    return allProviders;
}

/**
 * Provide a custom avatar generator to the application.
 * You can use this functionality to provide your own implementation of the AvatarGenerator.
 */
export function provideCustomAvatarGenerator(generator: Type<any>): Provider {
    return { provide: AvatarGenerator, useClass: generator };
}
