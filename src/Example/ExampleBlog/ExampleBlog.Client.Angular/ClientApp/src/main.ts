import { enableProdMode } from '@angular/core';

import { environment } from './environments/environment';
import { ExampleAvatarGenerator } from './app/ExampleAvatarGenerator';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { bootstrapApplication } from '@angular/platform-browser';
import { withRouterConfig, provideRouter, withComponentInputBinding } from '@angular/router';
import { AppRoutes } from './app/app.routes';
import { provideAnimations } from '@angular/platform-browser/animations';
import { AppComponent } from './app/app.component';
import { provideCustomAvatarGenerator, provideRestWorld } from "./app/ngx-restworld-client/provide-restworld";
import { providePrimeNG } from "primeng/config";
import Aura from '@primeng/themes/aura';
import { ConfirmationService, MessageService } from "primeng/api";
import { ClrFormatPipe } from "./app/ngx-restworld-client/pipes/clr-format.pipe";
import { definePreset } from "@primeng/themes";
import * as base from "@primeng/themes/aura/base";
import { NgHttpCachingStrategy, provideNgHttpCaching, withNgHttpCachingLocalStorage } from "ng-http-caching";

if (environment.production) {
    enableProdMode();
}

async function main() {
    try {

        await bootstrapApplication(AppComponent, {
            providers: [
                provideRestWorld(),
                provideCustomAvatarGenerator(ExampleAvatarGenerator),
                provideHttpClient(withInterceptorsFromDi()),
                provideRouter(
                    AppRoutes,
                    withRouterConfig({ onSameUrlNavigation: 'reload' }),
                    withComponentInputBinding()),
                provideAnimations(),
                providePrimeNG({
                    ripple: true,
                    theme: { preset: definePreset(Aura, { semantic: { primary: base.default.primitive.blue} }) }
                }),
                MessageService,
                ConfirmationService,
                ClrFormatPipe
            ]
        });
    }
    catch (e) {
        console.error(e);
    }
}

main();
