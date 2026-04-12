# Angular Client Setup

Use these guidelines to scaffold and configure an Angular SPA that talks to a RESTworld backend.

## Project structure (Angular 22+)

Starting with Angular 22, the dotnet and Angular parts of the client are split into two separate projects:

- **`ExampleBlog.Client`** — the ASP.NET Core host project (previously called `ExampleBlog.Client.Angular`). It serves the Angular output as static files from its `wwwroot/` folder after publishing and references the `.esproj`. It also provides the `/configuration` endpoint from where the Angular SPA gets the API endpoints.
- **`ExampleBlog.Client.Angular`** — the standalone Angular project (previously the `ClientApp/` subfolder inside the dotnet project). It now lives as its own sibling folder with its own `angular.json`, `package.json`, and `.esproj`.

Use the [`Migrate-AngularSpa.ps1`](../src/Example/ExampleBlog/Migrate-AngularSpa.ps1) script to apply this split to an existing project. The script moves the old `ClientApp/` contents to a new sibling folder, generates the `.esproj`, wires up the `proxyConfig`, and updates the `.csproj` accordingly.

## Project setup tips

- The ASP.NET Core host project (`ExampleBlog.Client`) references the Angular `.esproj` with `<ReferenceOutputAssembly>false</ReferenceOutputAssembly>`.
- Configure API URLs in `appsettings.json` as shown in [Configuration](configuration.md).
- RESTworld rewrites API endpoints automatically when Aspire injects environment variables.
- On publish, the Angular build output (`dist/browser/`) is copied into the `wwwroot/` folder of the dotnet host project and served as static files.

## ExampleBlog quick start

The **ExampleBlog** application demonstrates this split-project setup. The Angular project lives under [`src/Example/ExampleBlog/ExampleBlog.Client.Angular`](../src/Example/ExampleBlog/Exampleblog.Client.Angular) and the ASP.NET Core host under [`src/Example/ExampleBlog/ExampleBlog.Client`](../src/Example/ExampleBlog/ExampleBlog.Client). Use them as a blueprint when wiring your own SPA.

### Bootstrap the application

Register the RESTworld client providers, Angular Router, and PrimeNG configuration in `main.ts`:

```ts
import { bootstrapApplication } from '@angular/platform-browser';
import { AppComponent } from './app/app.component';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter, withRouterConfig, withComponentInputBinding } from '@angular/router';
import { definePreset } from '@primeng/themes';
import { providePrimeNG } from 'primeng/config';
import Aura from '@primeng/themes/aura';
import * as base from '@primeng/themes/aura/base';
import { MessageService, ConfirmationService } from 'primeng/api';
import { provideRestWorld, provideCustomAvatarGenerator } from './app/ngx-restworld-client/provide-restworld';
import { ExampleAvatarGenerator } from './app/ExampleAvatarGenerator';
import { AppRoutes } from './app/app.routes';

bootstrapApplication(AppComponent, {
  providers: [
    provideRestWorld(),
    provideCustomAvatarGenerator(ExampleAvatarGenerator),
    provideHttpClient(withInterceptorsFromDi()),
    provideRouter(AppRoutes, withRouterConfig({ onSameUrlNavigation: 'reload' }), withComponentInputBinding()),
    provideAnimations(),
    providePrimeNG({
      ripple: true,
      theme: { preset: definePreset(Aura, { semantic: { primary: base.default.primitive?.blue } }) }
    }),
    MessageService,
    ConfirmationService
  ]
});
```

- `provideRestWorld()` registers the RESTworld Angular client, including HTTP interceptors and HAL helpers.
- `provideCustomAvatarGenerator` overrides avatar rendering inside RESTworld UI templates.
- `provideHttpClient(withInterceptorsFromDi())` ensures RESTworld interceptors hook into the Angular `HttpClient` pipeline.
- `providePrimeNG` configures the PrimeNG theme (Aura by default) and enables ripple effects.
- `MessageService` and `ConfirmationService` power toast notifications and confirmation dialogs surfaced by RESTworld UI components.

### Define routes and menus

The example app centralizes routes in `app.routes.ts`, mixing RESTworld-provided views with custom components:

```ts
export const AppRoutes: Route[] = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  { path: 'edit/postsForBlog/:apiName/:rel', component: PostsForBlogComponent },
  { path: 'list/postWithAuthor', runGuardsAndResolvers: 'always', component: PostWithAuthorListComponent },
  { path: 'edit/postWithAuthor/:apiName/:rel/:uri', component: PostWithAuthorComponent },
  { path: 'list/:apiName/:rel', runGuardsAndResolvers: 'always', component: RESTworldListViewComponent },
  { path: 'edit/:apiName/:rel/:uri', component: RESTworldEditViewComponent }
];

export const AppMenu: MenuItem[] = [
  {
    label: 'Examples with defaults',
    items: [
      { label: 'Blogs', routerLink: ['list', 'ExampleBlog', 'MyEx:Blog'], queryParams: { $orderby: 'id desc' } },
      { label: 'Authors', routerLink: ['list', 'ExampleBlog', 'MyEx:Author'] }
    ]
  }
  // ...additional menu items
];
```

- `RESTworldListViewComponent` and `RESTworldEditViewComponent` render HAL-driven CRUD pages based on relation values (e.g., `MyEx:Blog`).
- Custom routes such as `postWithAuthor` demonstrate how to mix RESTworld views with bespoke components when you need additional layout or aggregation logic.
- `runGuardsAndResolvers: 'always'` ensures RESTworld refreshes data when the same route is revisited with different query parameters.
- Use the `AppMenu` model alongside PrimeNG menus to surface frequently used relations.

### Compose the shell

Keep the root component simple—leverage Angular's standalone components and PrimeNG widgets for layout and notifications:

```html
<nav>
  <app-navigation></app-navigation>
</nav>
<main>
  <router-outlet></router-outlet>
</main>
<aside>
  <p-toast></p-toast>
</aside>
```

- `<router-outlet>` hosts RESTworld list/edit views and your custom pages.
- `<p-toast>` integrates PrimeNG's toast service, which RESTworld uses for feedback.
- Provide navigation (e.g., `<app-navigation>`) that reads from `AppMenu` or a similar model.

### Proxy configuration

During development the Angular dev server proxies certain requests back to the ASP.NET Core host so the browser can fetch settings without CORS issues. The proxy is configured in `src/proxy.conf.js`:

```js
const PROXY_CONFIG = [
    {
        context: [
            '/settings',
        ],
        target: process.env["services__ExampleBlog-Client__https__0"],
        secure: process.env["NODE_ENV"] !== "development"
    }
];

console.log(`Proxy configuration:\n${JSON.stringify(PROXY_CONFIG, null, 2)}`)

module.exports = PROXY_CONFIG;
```

- The `target` is read from an Aspire-injected environment variable (`services__<ServiceName>__https__0`), so no hard-coded URLs are required.
- The `/settings` path is forwarded to the ASP.NET Core host, which serves the client-side configuration (API URLs, OTEL settings, etc.) from `appsettings.json`.
- `proxyConfig` is referenced from the `serve` target in `angular.json`.

### Testing with Vitest

The Angular project uses **Vitest** instead of Karma/Jasmine. The test runner is configured in `vitest.config.ts`:

```ts
import { defineConfig } from 'vitest/config'
export default defineConfig({
    test: {
        globals: true,
        environment: 'jsdom',
        includeTaskLocation: true,
        reporters: ["default", "junit"],
        outputFile: "test-results.xml",
        coverage: {
            provider: "istanbul",
            reporter: ["cobertura"],
        }
    }
})
```

In `angular.json` the `test` architect target uses `@angular/build:unit-test` with `runnerConfig` pointing to `vitest.config.ts`:

```json
"test": {
    "builder": "@angular/build:unit-test",
    "options": {
        "buildTarget": "exampleblog.client.angular:build:unit-tests",
        "runnerConfig": "vitest.config.ts"
    }
}
```

### Publishing

When publishing the ASP.NET Core host project, the Angular build is run automatically (via the `.esproj` reference) and the output from `dist/browser/` is copied into the `wwwroot/` folder of `ExampleBlog.Client`. The dotnet host then serves the SPA as static files.

### Running against multiple APIs

When your Angular client targets more than one RESTworld API (or multiple versions), rely on the settings described in [Configuration](configuration.md). The RESTworld Angular client uses relation metadata plus configuration to resolve endpoints, so you rarely need hard-coded URLs.

Next, explore the reusable UI building blocks in [Angular Client Components](client-development-angular-components.md), or review the foundational patterns in [Angular Client Core Concepts](client-development-angular-core.md).
