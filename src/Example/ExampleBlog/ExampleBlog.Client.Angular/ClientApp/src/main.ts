
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideHttpClient, withInterceptorsFromDi } from "@angular/common/http";
import { provideAnimations } from "@angular/platform-browser/animations";
import { provideRouter, withRouterConfig, withComponentInputBinding } from "@angular/router";
import { definePreset } from "@primeng/themes";
import { MessageService, ConfirmationService } from "primeng/api";
import { providePrimeNG } from "primeng/config";
import Aura from '@primeng/themes/aura';
import * as base from "@primeng/themes/aura/base";
import { AppRoutes } from "./app/app.routes";
import { ExampleAvatarGenerator } from "./app/ExampleAvatarGenerator";
import { ClrFormatPipe } from "./app/ngx-restworld-client/pipes/clr-format.pipe";
import { provideRestWorld, provideCustomAvatarGenerator } from "./app/ngx-restworld-client/provide-restworld";



bootstrapApplication(AppComponent, {
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
            theme: { preset: definePreset(Aura, { semantic: { primary: base.default.primitive?.blue} }) }
        }),
        MessageService,
        ConfirmationService,
        ClrFormatPipe
    ]
})
.catch(err => console.error(err));
