import { EnvironmentProviders, inject, provideAppInitializer, type Provider, type Type } from "@angular/core";
import { OpenTelemetryService } from "./services/opentelemetry.service";
import { SettingsService } from "./services/settings.service";
import { AvatarGenerator } from "./services/avatar-generator";
import { NgHttpCachingStrategy, provideNgHttpCaching, withNgHttpCachingLocalStorage, type NgHttpCachingConfig } from "ng-http-caching";

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
 */
export function provideRestWorld(ngHttpCachingConfig?: NgHttpCachingConfig): EnvironmentProviders[] {
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

    const allProviders = [restWorldInitializer, ...cachingProviders];

    return allProviders;
}

/**
 * Provide a custom avatar generator to the application.
 * You can use this functionality to provide your own implementation of the AvatarGenerator.
 */
export function provideCustomAvatarGenerator(generator: Type<any>): Provider {
    return { provide: AvatarGenerator, useClass: generator };
}
