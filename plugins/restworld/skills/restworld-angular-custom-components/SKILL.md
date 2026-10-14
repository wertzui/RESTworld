---
name: restworld-angular-custom-components
description: 'Customize the RESTworld Angular client (@wertzui/ngx-restworld-client) in a consuming app. Use when asked to customize the avatar (provideCustomAvatarGenerator, AvatarGenerator, rw-avatar); change the layout or buttons of rw-form / rw-signal-form with the #content, #buttons, #beforeButtons, #afterButtons templates and rw-form-element; customize dropdown, object or extra-tab templates (#inputOptionsSingle, #inputObject, #extraTabs); use rw-image, rw-file, image cropping or file upload; style rows or menus of rw-table; understand column filters and OData filtering; change the PrimeNG theme (providePrimeNG, definePreset, Aura); or find out what is NOT customizable (rw-display / rw-input type mapping has no registry, so build your own component). Consumer-side only.'
argument-hint: 'Describe what to customize (avatar, form layout/buttons, dropdown/object rendering, image/file input, table styling/menus, column filters, theme) and whether the app uses classic (rw-form/rw-table) or Signal Forms (rw-signal-form/rw-signal-table) components.'
---

# RESTworld Angular: Customize Components, Avatar, Theme

Audience: developers who **use** `@wertzui/ngx-restworld-client`. Blueprint: `src/Example/ExampleBlog/Exampleblog.Client.Angular`. For routes/pages see [restworld-angular-add-page](../restworld-angular-add-page/SKILL.md). For server-side control of what the form contains (labels, types, images, dropdown data) see `restworld-halforms-customization`.

## When to Use
- Provide your own avatar images for `createdBy` / `lastChangedBy` users
- Change layout/buttons of a RESTworld form, or render a few properties manually
- Customize dropdown/object rendering inside forms, add extra tabs to the edit view
- Use image (cropper) or file inputs, also outside of generated forms
- Style table rows/cells, add row/header/context menu items, understand column filters
- Re-theme the PrimeNG based UI

## Import Paths
Import from the package: `import { AvatarGenerator, provideCustomAvatarGenerator, RestWorldClientCollection } from '@wertzui/ngx-restworld-client';`. The example app imports `./ngx-restworld-client/services/avatar-generator` etc. only because `src/app/ngx-restworld-client` is a junction to the library source; do not copy those paths into your app. The `AfterSubmit*Event` and `DropdownChangeEvent` types are not in the public API (`PropertyTemplateContext`/`DropdownTemplateContext` are).

## What Is (Not) Customizable
- **Supported hooks:** `provideCustomAvatarGenerator`, content templates on forms/views/dropdown/object inputs (below), `providePrimeNG` theme, CSS, table style/menu inputs, `rw-form-element`/`rw-signal-form-element`/`rw-input`/`rw-display` as building blocks in your own components.
- **Not available:** there is **no** registry or token to swap the component used for a property type (the `restworld-inputs`/`restworld-displays` files are just component declarations, `@switch (property.type)` is hard-coded), and `rw-table` has no cell/column template slot. To render a type differently, build your own page/component and use your own control for that property inside the `#content` slot, mixing in `rw-form-element` for the rest.
- Which properties/types/labels/dropdown options exist comes from the server's HAL-FORMS templates (DTO attributes), not from the client.

## Custom Avatar Generator
`rw-avatar` (`[user]`) is used by `rw-display-simple` for properties named `createdBy` and `lastChangedBy`. It asks `AvatarGenerator` for an image; if none, it shows two initials on a generated color. By default no image is returned. Override `getImageAsyncOverride` in a subclass and register it:

```ts
// ExampleAvatarGenerator.ts (example app, simplified; import from the npm package in your app)
@Injectable({ providedIn: 'root' })
export class ExampleAvatarGenerator extends AvatarGenerator {
  private readonly _http: HttpClient;
  private readonly _photoLink: Link;                       // from '@wertzui/ngx-hal-client'

  constructor(clients: RestWorldClientCollection, private readonly _sanitizer: DomSanitizer) {
    super();
    this.getImageAsyncOverride = email => this.getPhotoAsync(email);
    const client = clients.getClient('ExampleBlog');       // available: provideRestWorld() loads clients before bootstrap completes
    this._http = client.halClient.httpClient;
    this._photoLink = client.getLinkFromHome('Photo');     // link from the API home resource (URI template)
  }

  private async getPhotoAsync(email: string): Promise<SafeUrl> {
    const uri = this._photoLink.fillTemplate({ email });   // expand RFC 6570 template, no hardcoded URL
    try {
      const response = await lastValueFrom(this._http.get(uri, { responseType: 'blob', observe: 'response' }));
      if (!response.ok || response.body === null) return '';
      return this._sanitizer.bypassSecurityTrustUrl(window.URL.createObjectURL(response.body));
    } catch { return ''; }                                  // '' => initials are shown
  }
}
```
```ts
// main.ts
providers: [provideRestWorld(), provideCustomAvatarGenerator(ExampleAvatarGenerator), ...]
```
Notes: return `''` for "no image". Results are cached per name/email in a static map. `getLabelAsync` / `getStyleAsync` can be overridden as well (labels are 2 uppercase letters by default). Use `<rw-avatar [user]="email">` in your own templates; it injects the same `AvatarGenerator`.

## Custom Form Layout and Buttons
`rw-form` and `rw-signal-form` accept these content templates: `#content` (replaces all fields), `#buttons` (replaces all buttons), `#beforeButtons`, `#afterButtons` (add buttons next to the defaults). Template context (`let-...`): `form`, `template`, `apiName`. **`form` and `template` are signals/computed** (the form passes the signal itself), so call them: `form()`, `template()`; `apiName` is a plain string.

Classic (needs a `[formGroup]` on the elements because the template is declared in your component):
```html
<rw-form [apiName]="apiName" rel="MyEx:Post" [(template)]="editTemplate">
  <ng-template #content let-form="form" let-template="template" let-apiName="apiName">
    <rw-form-element [apiName]="apiName" [property]="template().propertiesRecord['title']!" [formGroup]="form()"></rw-form-element>
    <rw-form-element [apiName]="apiName" [property]="template().propertiesRecord['text']!" [formGroup]="form()"></rw-form-element>
  </ng-template>
  <ng-template #afterButtons let-form="form">
    <button pButton type="button" label="Preview" (click)="preview(form().value)"></button>
  </ng-template>
</rw-form>
```
Signal Forms (use `getChildFieldTree`, exported by the package, to get the field of a property by name):
```html
<rw-signal-form [apiName]="apiName" rel="MyEx:Post" [(template)]="editTemplate">
  <ng-template #content let-form="form" let-template="template" let-apiName="apiName">
    <rw-signal-form-element [apiName]="apiName" [property]="template().propertiesRecord['title']!"
      [field]="getField(form(), 'title')"></rw-signal-form-element>
  </ng-template>
</rw-signal-form>
```
```ts
protected getField(form: FieldTree<Record<string, unknown>>, name: string) { return getChildFieldTree(form, name)!; }
```
Elements: `rw-form-element` = label + input (`property`, `apiName`); `rw-input` = input only; `rw-label` (`property`). Signal versions add a `field` input. Submit/Reload/Delete are the form's own methods (`submit()`, `reload()`, `delete()`), reachable via a `viewChild(RestWorldFormComponent)` / `viewChild(RestWorldSignalFormComponent)` reference.

Other slots:
- `rw-edit` / `rw-signal-edit`: `#extraTabs` adds an "Extra" tab (see `post-with-author*.component.html`; that example also uses `#visualTabAdditional`, which this library version does not read, so verify before copying it).
- Dropdowns (`rw-input-dropdown` / `rw-signal-input-dropdown`): `#inputOptionsSingle`, `#inputOptionsMultiple`, inputs `getLabel`, `getTooltip`, `caseSensitive`, output `onChange`.
- `rw-input-object` / `rw-signal-input-object`: `#inputObject`.
These components are only reachable if you render them yourself (inside your own `#content`), since the generated forms do not expose their slots.

## Image and File Components
- `rw-image` and `rw-file` are `ControlValueAccessor`s whose value is a data URI string, used automatically by the generated forms for properties of type `Image` / `File`.
- `rw-image` needs `[property]` (a property with the server-provided `restWorldImage` settings: `aspectRatio`, `format`, `resizeToWidth`, `cropper...`, `accept`, ...; configured with `[RestWorldImage]` on the DTO property, see `restworld-halforms-customization`). It shows download, upload, thumbnail and a crop dialog.
- `rw-file` has inputs `accept` (default `""`) and `fileName` (default `"download"`):
```html
<rw-file [(ngModel)]="fileDataUri" accept="application/pdf" fileName="contract.pdf"></rw-file>
```
(import `FormsModule`, `RestWorldFileComponent`). Types: `RestWorldImage`, `PropertyWithImage`, `PropertyWithOptions` are exported.

## Tables: Styling, Menus, Column Filters
Inputs on `rw-table` / `rw-signal-table` (example: `posts-for-blog*.component`):
```ts
generateRowClasses(row, rowIndex) { return rowIndex % 2 === 0 ? 'row-even' : 'row-odd'; }
generateCellClasses(row, property, rowIndex, columnIndex) { return columnIndex % 2 === 0 ? 'cell-even' : 'cell-odd'; }
```
```html
<rw-table [rowStyleClass]="generateRowClasses" [cellStyleClass]="generateCellClasses" [rowHover]="true"
          [styleClass]="'my-table'" [tableStyle]="{ 'min-width': '50rem' }" [rowMenu]="rowMenu" [headerMenu]="headerMenu" ...>
```
Pass plain functions (they are not bound to `this`). `rowMenu` is `(row, openedByRightClick) => MenuItem[]` (also used for the right-click context menu), `headerMenu` is `MenuItem[]` (PrimeNG). `showRowMenuAsColumn`, `showRowMenuOnRightClick`, `scrollable`, `scrollHeight` adjust layout. `rw-list`/`rw-signal-list` expose only `createButtonMenu`.

Column filters: shown only when `lazy` is `true` (default) and the column is not read-only. The filter type derives from the search template property type (`Number`/`Percent`/`Currency`/`Month` numeric, `Bool` boolean, `Date`/`DatetimeLocal`/`DatetimeOffset` date, with `options` enum or numeric, otherwise text) and is rendered with `rw-input`. The table translates filters/sorting into OData `$filter`/`$orderby`/`$skip`/`$top` (`ODataService`, exported) and emits them via the two-way `oDataParameters`; with `reflectParametersInUrl` (default `true`) they are also written to URL query params. The filter widgets themselves are not replaceable; to change which columns are filterable/sortable change the server-side search template, or set `[lazy]="false"`.

## PrimeNG Theme and CSS
```ts
import { definePreset } from '@primeuix/themes';
import Aura from '@primeuix/themes/aura';
import * as base from '@primeuix/themes/aura/base';
providePrimeNG({ ripple: true, theme: { preset: definePreset(Aura, { semantic: { primary: base.default.primitive?.blue } }) } })
```
Swap the primary palette (e.g. `.green`) or the preset to re-theme everything. Keep `primeflex.css`, `primeicons.css` and Font Awesome in `angular.json` styles (library templates use their classes). Global overrides go in `src/styles.css` (the example sets the layout grid on `app-root` and `.p-toast-message-text`). Signal Forms status classes (`.ng-invalid`, `.ng-touched`, ...) are enabled by `provideRestWorld()`; pass a second argument (`SignalFormsConfig`) to change that.

## Verification Checklist
- [ ] Imports from `@wertzui/ngx-restworld-client`, not junction-relative paths
- [ ] Custom avatar generator is `providedIn: 'root'`, extends `AvatarGenerator`, registered with `provideCustomAvatarGenerator(...)` in `main.ts`, returns `''` when no image
- [ ] Custom `#content` uses `form()` / `template()` (signals), and classic `rw-form-element` gets `[formGroup]="form()"`; signal elements get `[field]`
- [ ] Classic and Signal Forms components are not mixed inside one tree
- [ ] Custom type rendering done in own components (no non-existing registry/token)
- [ ] Theme via `providePrimeNG`; required CSS files still in `angular.json`
- [ ] App builds (`npm run build`) and the page works against the running API

## Related
[Components](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-angular-components.md) · [Setup](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-angular-setup.md) · [Core concepts](https://github.com/wertzui/RESTworld/blob/main/doc/client-development-angular-core.md) · Skills: `restworld-angular-add-page`, `restworld-halforms-customization`, `restworld-api-extras`, `restworld-dotnet-client`
