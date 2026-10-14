---
name: restworld-angular-add-page
description: 'Add a list, edit or custom page for a RESTworld endpoint to an Angular app that uses @wertzui/ngx-restworld-client. Use when asked to add a page, route, menu entry, list view, edit view, CRUD UI or master/detail page for a RESTworld resource; to choose between rw-list/rw-edit (Reactive Forms, RESTworldListViewComponent) and rw-signal-list/rw-signal-edit (Signal Forms); to compose rw-table, rw-signal-table, rw-form or rw-signal-form in a custom component; to wire app.routes.ts, AppMenu, apiName/rel/uri route parameters or withComponentInputBinding; to load data with RestWorldClientCollection / RestWorldClient (HAL home discovery, getLinkFromHome, getList, fillTemplate, OData parameters); or to bootstrap a client with provideRestWorld in main.ts, proxy.conf.js, npm start or Aspire. Consumer-side only.'
argument-hint: 'Name the RESTworld API (apiName, as configured in the client settings) and the resource relation (rel, e.g. MyEx:Post). Say whether you want the default list/edit pages or a custom page, and whether the app uses Reactive Forms (classic) or Signal Forms.'
---

# RESTworld Angular: Add a List / Edit Page

Audience: developers who **use** `@wertzui/ngx-restworld-client` in their own Angular app. Blueprint: `src/Example/ExampleBlog/Exampleblog.Client.Angular`.

## When to Use
- Add a list and/or edit page for a RESTworld resource (route + menu entry)
- Decide between the classic (Reactive Forms) and Signal Forms component variants
- Build a custom page that embeds `rw-table` / `rw-form` (or their `rw-signal-*` equivalents)
- Load HAL resources, templates and links through the library's client services
- Bootstrap a fresh Angular app for RESTworld (`provideRestWorld`, proxy, dev run)

## Import Paths (read first)
Consumers import everything from the npm package entry point:
```ts
import { provideRestWorld, RESTworldListViewComponent, RESTworldEditViewComponent, RestWorldClientCollection } from '@wertzui/ngx-restworld-client';
```
The example app uses deep relative paths such as `./ngx-restworld-client/views/restworld-list-view/restworld-list-view.component` only because `src/app/ngx-restworld-client` is a junction to the library source. In your own app use `@wertzui/ngx-restworld-client`. The public API exports components, views, services (`RestWorldClientCollection`, `RestWorldClient`, `ProblemService`, `AvatarGenerator`, `ODataService`, `SettingsService`), models (`ODataParameters`, `ApiUrl`, ...), pipes and `provideRestWorld` / `provideCustomAvatarGenerator`. The `AfterSubmitOkEvent`/`AfterSubmitRedirectEvent` types are **not** exported; let TypeScript infer the `$event` type.

## Key Rules
1. Use `rw-*` components for RESTworld resources; do not hand-write tables/forms against the REST API.
2. Never hardcode URLs. Start from `apiName` and the relation `rel` (e.g. `MyEx:Post`); the library resolves the HAL home resource and its links.
3. `apiName` must match a name in the client settings (`ApiUrls` served by the host at `/settings`, see [configuration](https://github.com/wertzui/RESTworld/blob/main/doc/configuration.md)).
4. Classic (`rw-list`, `rw-edit`, `rw-table`, `rw-form`) and Signal Forms (`rw-signal-*`) trees are independent implementations and may be mixed in one app, but not nested inside each other.
5. Route params are bound to component inputs, so `provideRouter(..., withComponentInputBinding())` is **required** and input names must equal route param names (`apiName`, `rel`, `uri`).
6. List routes need `runGuardsAndResolvers: 'always'` so changed query parameters (`$filter`, `$orderby`, `$skip`, `$top`) reload the data.
7. Put specific routes **before** generic ones (`edit/postsForBlog/:apiName/:rel` would otherwise collide with `edit/:apiName/:rel/:uri` patterns).

## Classic vs Signal Forms

| | Classic | Signal Forms |
|---|---|---|
| List view | `RESTworldListViewComponent` (`rw-list`), default `editLink` `/edit` | `RESTworldSignalListViewComponent` (`rw-signal-list`), default `editLink` `/edit/signal` |
| Edit view | `RESTworldEditViewComponent` (`rw-edit`) | `RESTworldSignalEditViewComponent` (`rw-signal-edit`) |
| Table / Form | `rw-table` / `rw-form` (inline edit needs a surrounding `[formGroup]` + `formArrayName`) | `rw-signal-table` / `rw-signal-form` (standalone; read edits via `editedRows()`) |
| Extra output | `rw-form` has `valueChanges` | none, read the model signal |

Choose **Signal Forms** when the app has adopted `@angular/forms/signals`; otherwise use the classic variants. Inputs/outputs are otherwise identical. See [Angular Client Components](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-angular-components.md).

## Step 1: Bootstrap (`main.ts`)
Taken from the example app (Angular 22, PrimeNG 21):
```ts
import { bootstrapApplication } from '@angular/platform-browser';
import { provideHttpClient, withInterceptorsFromDi } from '@angular/common/http';
import { provideAnimations } from '@angular/platform-browser/animations';
import { provideRouter, withRouterConfig, withComponentInputBinding } from '@angular/router';
import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';
import * as base from '@primeuix/themes/aura/base';
import { MessageService, ConfirmationService } from 'primeng/api';
import { providePrimeNG } from 'primeng/config';
import { provideRestWorld, ClrFormatPipe } from '@wertzui/ngx-restworld-client';
import { AppComponent } from './app/app.component';
import { AppRoutes } from './app/app.routes';

bootstrapApplication(AppComponent, {
  providers: [
    provideRestWorld(),
    provideHttpClient(withInterceptorsFromDi()),
    provideRouter(AppRoutes, withRouterConfig({ onSameUrlNavigation: 'reload' }), withComponentInputBinding()),
    provideAnimations(),
    providePrimeNG({ ripple: true, theme: { preset: definePreset(Aura, { semantic: { primary: base.default.primitive?.blue } }) } }),
    MessageService,
    ConfirmationService,
    ClrFormatPipe
  ]
}).catch(err => console.error(err));
```
- `provideRestWorld()` fetches `/settings` at startup (app initializer), creates one `RestWorldClient` per API URL in `RestWorldClientCollection` (home resource included), and adds HTTP caching, the OpenTelemetry interceptor and Signal Forms config (`NG_STATUS_CLASSES`).
- `MessageService`/`ConfirmationService` are needed by the views (toasts, delete confirmation). `ClrFormatPipe` is provided in the example because the library's `PropertyTypeFormatPipe` injects it (the setup doc's snippet omits it).
- The example imports `@primeuix/themes`; the setup doc snippet still shows `@primeng/themes`.
- Shell (`app.component.html`): `<app-navigation>` (a `p-menubar [model]="AppMenu"`), `<router-outlet>`, `<p-toast>`.
- `angular.json` `styles` in the example: `@fortawesome/fontawesome-free/css/all.css`, `primeflex/primeflex.css`, `primeicons/primeicons.css`, `src/styles.css`. Library templates use Font Awesome icons and PrimeFlex classes, so keep them.
- Install `@wertzui/ngx-restworld-client` plus its `peerDependencies`; the example's `package.json` is a known-good set (PrimeNG, `@primeuix/themes`, `primeflex`, `primeicons`, `@fortawesome/fontawesome-free`, `@wertzui/ngx-hal-client`, `ngx-valdemort`, `ng-http-caching`, `ngx-image-cropper`, `clr-format`, `uri-templates-es`, `@odata/parser`, `cldr-*`, `@opentelemetry/*`).

## Step 2: Routes and Menu (`app.routes.ts`)
```ts
export const AppRoutes: Route[] = [
  { path: '', component: HomeComponent, pathMatch: 'full' },
  // custom pages first
  { path: 'list/postWithAuthor', runGuardsAndResolvers: 'always', component: PostWithAuthorListComponent },
  { path: 'edit/postWithAuthor/:apiName/:rel/:uri', component: PostWithAuthorComponent },
  // Signal Forms defaults
  { path: 'list/signal/:apiName/:rel', runGuardsAndResolvers: 'always', component: RESTworldSignalListViewComponent, data: { editLink: '/edit/signal' } },
  { path: 'edit/signal/:apiName/:rel/:uri', component: RESTworldSignalEditViewComponent },
  // classic defaults
  { path: 'list/:apiName/:rel', runGuardsAndResolvers: 'always', component: RESTworldListViewComponent },
  { path: 'edit/:apiName/:rel/:uri', component: RESTworldEditViewComponent }
];

export const AppMenu: MenuItem[] = [{
  label: 'Examples with defaults',
  items: [
    { label: 'Blogs', routerLink: ['list', 'ExampleBlog', 'MyEx:Blog'], queryParams: { $orderby: 'id desc' } },
    { label: 'Authors', routerLink: ['list', 'ExampleBlog', 'MyEx:Author'] }
  ]
}];
```
- `:apiName` + `:rel` identify the list; `:uri` is the resource's `self` href. The list view builds row links and the create button (using the list's `new` link) for you.
- Query params `$filter`, `$orderby`, `$skip`, `$top` are bound to inputs with these aliases on `rw-list`/`rw-signal-list`, hence `queryParams: { $orderby: ... }` in menu items works.
- A list links to `editLink` + `/:apiName/:rel/:uri`. Override it (route `data` or the `editLink` input) when your edit route differs.
- For a plain CRUD page the routes alone are enough; no new component is needed.

## Step 3: Custom Pages That Wrap a View
From the example (`post-with-author`):
```html
<!-- list page: PostWithAuthorListComponent, imports: [RESTworldListViewComponent, DialogModule] -->
<rw-list apiName="ExampleBlog" rel="MyEx:Post" editLink="/edit/postWithAuthor" [createButtonMenu]="menu"></rw-list>
```
```ts
// edit page: inputs are bound from the route (:apiName/:rel/:uri)
public readonly apiName = input.required<string>();
public readonly rel = input.required<string>();
public readonly uri = input.required<string>();
public readonly modifiedUri = computed(() => this.uri().replace(/\/post\//gmi, '/postwithauthor/'));
```
```html
<rw-edit [uri]="modifiedUri()" [apiName]="apiName()" [rel]="rel()">
  <ng-template #extraTabs><!-- extra tab content --></ng-template>
</rw-edit>
```
The Signal Forms twin uses `<rw-signal-list ... editLink="/edit/postWithAuthor/signal">` and `<rw-signal-edit>`. `rw-edit`/`rw-signal-edit` require `apiName`, `rel`, `uri`, and accept an `#extraTabs` template. `rw-list`/`rw-signal-list` take `apiName`, `rel`, `editLink`, `createButtonMenu` (PrimeNG `MenuItem[]`) and the OData aliases.

## Step 4: Fully Custom Page with `rw-table` / `rw-form`
Load data with `RestWorldClientCollection` (as `posts-for-blog*.component.ts` does):
```ts
const client = this._clients.getClient(apiName);                           // throws if the name is unknown
const response = await client.getList<PostListDto>(rel, oDataParameters);  // follows the home GetList link for rel
if (this._problemService.checkResponseAndDisplayErrors(response, undefined, 'Error while loading', 'Error')) {
  const list = response.body;                                              // PagedListResource: _embedded.items, totalPages
  const templates = await client.getAllTemplates(list);                    // keyed by title: templates['Search'], templates['Edit']
}
```
Classic table (inline editing needs `ReactiveFormsModule`, a `FormArray` and `formArrayName`):
```html
<form [formGroup]="formGroup">
  <rw-table formArrayName="posts" [apiName]="apiName" [searchTemplate]="searchTemplate()" [editTemplate]="editTemplate()"
    [rows]="items()" [totalRecords]="totalRecords()" [isLoading]="isLoading()"
    [(oDataParameters)]="oDataParameters" selectionMode="multiple" [(selectedRows)]="selection"></rw-table>
</form>
```
Signal table (no form wrapper):
```html
<rw-signal-table #table [apiName]="apiName" [searchTemplate]="searchTemplate()" [editTemplate]="editTemplate()"
  [rows]="items()" [totalRecords]="totalRecords()" [isLoading]="isLoading()"
  [(oDataParameters)]="oDataParameters" selectionMode="multiple" [(selectedRows)]="selection"></rw-signal-table>
<!-- edited values: table.editedRows() -->
```
Reload the list when `oDataParameters` changes (the example uses a `resource()` with `params: () => ({ oDataParameters: this.oDataParameters(), rel: this.rel() })`).
Other table inputs: `rowMenu`, `headerMenu`, `rowStyleClass`, `cellStyleClass`, `rowHover`, `lazy` (default `true`; `false` = client-side only), `paginator`, `rowsPerPageOptions`, `reflectParametersInUrl` (default `true`, writes OData parameters to the URL) and `urlParameterPrefix` (set one, or disable URL reflection, when a page hosts several tables).

Forms (both variants: `apiName`, `rel`, two-way `[(template)]`; outputs `afterSubmit`, `afterDelete`):
```html
<rw-form [apiName]="apiName" [rel]="rel" [(template)]="editTemplate" (afterSubmit)="onSaved($event)"></rw-form>
<rw-signal-form [apiName]="apiName" [rel]="rel" [(template)]="editTemplate"></rw-signal-form>
```
Content slots (`#content`, `#buttons`, ...) are covered in [restworld-angular-custom-components](../restworld-angular-custom-components/SKILL.md).

## HAL Discovery and Link Templating
- `RestWorldClient` (obtained via `RestWorldClientCollection.getClient(apiName)`) exposes `getLinkFromHome(rel, name?, curie?)`, `getLinksFromHome`, `getAllLinksFromHome()` (cached home resource), plus `getList`, `getSingle(rel, id)`, `getSingleByUri`, `getListByUri`, `getAllTemplates(resource)`, `getTemplateByUri`, `submit`, `save`, `delete*`. `LinkNames` lists the well-known link names (`Get`, `GetList`, `Post`, `Put`, `Delete`, `New`).
- Links can be URI templates: `link.fillTemplate({ email })` (`Link` from `@wertzui/ngx-hal-client`) expands RFC 6570 templates, as `ExampleAvatarGenerator` does for the `Photo` link.
- `ProblemService.checkResponseAndDisplayErrors(response, formGroup?, detail, title)` shows `ProblemDetails` as toasts/validation errors and narrows the response type.
- The API `Version` from the settings is sent as `Accept: application/hal+json; v=<n>`. See [core concepts](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-angular-core.md).

## Dev Run
- `src/proxy.conf.js` proxies `/settings` to the ASP.NET host (`target: process.env["services__ExampleBlog-Client__https__0"]`, injected by Aspire); `angular.json` references it as `proxyConfig`.
- Normally start the Aspire AppHost: it runs `npm start` for the Angular project (`AddJavaScriptApp(...).WithRunScript("start")`; `prestart` creates the dev certificate; `PORT` is injected). The host project calls `AddRestWorldWithSpaFrontend()`/`UseRestWorldWithSpaFrontend()` and serves `ApiUrls` at `/settings` (see `restworld-bootstrap-solution`).
- `npm test` runs Vitest via `ng test`; `npm run build` builds.

## Verification Checklist
- [ ] Imports come from `@wertzui/ngx-restworld-client`, not junction-relative paths
- [ ] `main.ts` has `provideRestWorld()`, `withComponentInputBinding()`, `provideHttpClient(withInterceptorsFromDi())`, `MessageService`, `ConfirmationService`, `ClrFormatPipe`, PrimeNG theme
- [ ] Shell has `<p-toast>` and `<router-outlet>`; Font Awesome, PrimeFlex, PrimeIcons CSS are in `angular.json`
- [ ] Route param names equal component input names; specific routes precede generic ones; list routes have `runGuardsAndResolvers: 'always'`
- [ ] `editLink` points at the edit route of the same variant (classic `/edit`, signal `/edit/signal`)
- [ ] `apiName` exists in `/settings` `ApiUrls`; `rel` exists in the API's home resource
- [ ] No classic/signal nesting; no hardcoded API URLs

## Related
[Setup](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-angular-setup.md) · [Components](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-angular-components.md) · [Core concepts](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-angular-core.md) · [Overview](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-angular.md) · Skills: `restworld-angular-custom-components`, `restworld-add-entity-endpoint`, `restworld-dotnet-client`, `restworld-bootstrap-solution`, `restworld-halforms-customization`, `restworld-api-extras`
