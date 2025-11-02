# Angular Client Setup

Use these guidelines to scaffold and configure an Angular SPA that talks to a RESTworld backend.

## Project setup tips

- Host the Angular client via `RESTworld.Client.AspNetCore` or serve it separately.
- Configure API URLs in `appsettings.json` as shown in [Configuration](configuration.md).
- RESTworld rewrites API endpoints automatically when Aspire injects environment variables.

## ExampleBlog quick start

The **ExampleBlog** application under [`src/Example/ExampleBlog/ExampleBlog.Client.Angular`](../src/Example/ExampleBlog/ExampleBlog.Client.Angular) demonstrates an Angular 17+ standalone setup. Use it as a blueprint when wiring your own SPA.

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

### Running against multiple APIs

When your Angular client targets more than one RESTworld API (or multiple versions), rely on the settings described in [Configuration](configuration.md). The RESTworld Angular client uses relation metadata plus configuration to resolve endpoints, so you rarely need hard-coded URLs.

Next, explore the reusable UI building blocks in [Angular Client Components](client-development-angular-components.md), or review the foundational patterns in [Angular Client Core Concepts](client-development-angular-core.md).
