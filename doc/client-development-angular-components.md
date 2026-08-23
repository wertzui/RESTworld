# Angular Client Components

Leverage the UI primitives from `@wertzui/ngx-restworld-client` to render HAL-driven data grids and forms without rewriting common logic.

## PrimeNG and theming

RESTworld ships with table and form templates built on PrimeNG. Extend or replace the theme by passing a custom preset to `providePrimeNG`, or override styles in global stylesheets. The ExampleBlog project uses the Aura theme preset with a custom primary color.

## `<rw-table>`

`<rw-table>` renders a PrimeNG-based data grid driven by a HAL `search` template (for columns, filters, and sorting) and, optionally, an `edit` template for inline editing. Provide row data from any HAL list response and connect the table to your filtering logic.

### Key inputs

- `apiName` *(required)* – RESTworld API alias, needed when the table creates forms for inline editing.
- `searchTemplate` *(required)* – HAL template describing columns, filter widgets, and sort options (typically `resource._templates.search`).
- `rows` *(required)* – array of DTOs displayed in the grid.
- `editTemplate` – enables editable cells and supplies metadata for input controls.
- `oDataParameters` / `rowsPerPageOptions` – reflect filter and paging options in query parameters or custom UX.
- `rowMenu`, `headerMenu`, `contextMenuItems` – provide PrimeNG `MenuItem` arrays for row and header actions.
- `selectionMode`, `selectedRows` – enable single or multi-row selection with two-way binding.

### Example usage

```html
<rw-table
  [apiName]="'ExampleBlog'"
  [searchTemplate]="templates.search"
  [editTemplate]="templates.edit"
  [rows]="posts"
  [(oDataParameters)]="oData"
  [isLoading]="loading"
  [rowMenu]="buildRowMenu"
  selectionMode="single"
  [(selectedRows)]="selected">
</rw-table>
```

React to the `load` event (PrimeNG `TableLazyLoadEvent`) to translate user filters or sort orders into OData queries, then forward those parameters to RESTworld list endpoints.

## `<rw-signal-table>` (Signal Forms)

`<rw-signal-table>` is the [Signal Forms](https://angular.dev/guide/forms/signals/overview) equivalent of `<rw-table>`. It renders the same PrimeNG-based data grid (columns, OData filters/sorting, pagination, row/header/context menus, inline editing), but builds row edit state using `@angular/forms/signals` (`form()`, `FieldTree`) instead of Reactive Forms. All inputs are identical to `<rw-table>` (`apiName`, `searchTemplate`, `rows`, `editTemplate`, `oDataParameters`, `rowsPerPageOptions`, `rowMenu`, `headerMenu`, `contextMenuItems`, `selectionMode`, `selectedRows`, etc.), so the same example usage shown above for `<rw-table>` applies, just with the `rw-signal-` prefix.

### Standalone vs. bound mode

`<rw-table>` finds its ancestor `FormArray` via `ControlContainer`/`[formArrayName]` when nested inside a Reactive Form (this is how `<rw-input-collection>` reuses `<rw-table>` to edit a `Collection` property in place). Signal Forms has no `ControlContainer` equivalent, so `<rw-signal-table>` exposes this as an explicit `field` input instead – `<rw-signal-input-collection>` uses exactly this bound mode internally, the same way `<rw-input-collection>` uses `[formArrayName]`:

- **Standalone (default) mode** – leave `field` unset. The component owns its own internal array of `SignalForm`s, rebuilt from `rows()`/`editTemplate()` whenever they change (mirrors `<rw-table>`'s own `formArray`-rebuilding behavior). Read edited-but-unsaved values via `editedRows()`.
- **Bound mode** – set `field` to a `FieldTree<ReadonlyArray<TItem>>` obtained from a parent Signal Form (for example, a `Collection` property's field). The table then edits that array's items in place instead of an internally-owned copy, mirroring `<rw-table>`'s `[formArrayName]` binding.

```html
<rw-signal-table
  [apiName]="'ExampleBlog'"
  [searchTemplate]="defaultTemplate()"
  [editTemplate]="defaultTemplate()"
  [field]="parentForm.tags"
  [lazy]="false">
</rw-signal-table>
```

## `<rw-form>`

`<rw-form>` renders a HAL form with validation messages and default Save/Delete/Reload buttons. It interacts with the RESTworld Angular client service to submit changes and handles optimistic concurrency via timestamps automatically.

### Key inputs and outputs

- `apiName` *(required)* – RESTworld API alias used to resolve links.
- `rel` *(required)* – relation for the current form (for example `MyEx:Post`).
- `template` *(required, two-way `model`)* – HAL template obtained from a `new` or `edit` response.
- `allowSubmit`, `allowReload`, `allowDelete` – enable or disable actions without hiding them.
- `showSubmit`, `showReload`, `showDelete` – show or hide the default buttons.
- `afterSubmit`, `afterDelete` – emitted when operations succeed.
- `valueChanges` – emits whenever the form data changes, mirroring Angular `FormGroup.valueChanges`.

### Custom content

Override the default layout by providing templates for the `content` or `buttons` slots:

```html
<rw-form
  [apiName]="'ExampleBlog'"
  rel="MyEx:Post"
  [(template)]="editTemplate">
  <ng-template #content let-form="form" let-template="template">
    <rw-form-element
      [property]="template.propertiesRecord.title"
      [formGroup]="form">
    </rw-form-element>
  </ng-template>
  <ng-template #buttons let-form="form">
    <button pButton type="button" label="Save" (click)="form.submit()"></button>
  </ng-template>
</rw-form>
```

If you simply want RESTworld to render all fields according to the template, omit the `content` slot and the component will output labeled inputs using the built-in `<rw-form-element>` components.

## `<rw-signal-form>` (Signal Forms)

`<rw-signal-form>` is the [Signal Forms](https://angular.dev/guide/forms/signals/overview) equivalent of `<rw-form>`. It renders the same HAL form, with the same Save/Delete/Reload buttons and optimistic-concurrency handling, but builds the form using Angular's `@angular/forms/signals` API (`form()`, `FieldTree`, `[formField]`) instead of Reactive Forms. Use it if your application has otherwise adopted Signal Forms and you want form state expressed as signals throughout.

Both `<rw-form>` and `<rw-signal-form>` (and their respective `rw-input-*` / `rw-signal-input-*` component trees) are fully supported and can be mixed within the same application – for example, adopting `<rw-signal-form>` only for new views while existing views keep using `<rw-form>`. They are independent, parallel implementations; neither wraps the other.

### Key inputs and outputs (Signal Forms)

The inputs and outputs are identical to `<rw-form>` (`apiName`, `rel`, `template`, `allowSubmit`/`allowReload`/`allowDelete`, `showSubmit`/`showReload`/`showDelete`, `afterSubmit`, `afterDelete`), with one difference: there is no `valueChanges` output, since with Signal Forms you can simply read the model signal directly (see below).

### Example usage (Signal Forms)

```html
<rw-signal-form
  [apiName]="'ExampleBlog'"
  rel="MyEx:Post"
  [(template)]="editTemplate">
</rw-signal-form>
```

If you simply want RESTworld to render all fields according to the template, omit the `content` slot (same as `<rw-form>`) and the component will output labeled inputs using the built-in `<rw-signal-form-element>` components.

### Supported property types

The `<rw-signal-input>` dispatcher (used internally by `<rw-signal-form>` / `<rw-signal-input-template>`) supports the same property types as its Reactive Forms counterpart `<rw-input>`, including nested `Object` properties (via `<rw-signal-input-object>`) and `Collection` properties (via `<rw-signal-input-collection>`).

> [!NOTE]
> `<rw-signal-input-collection>` nests `<rw-signal-table>` bound to the collection's own `field` in "bound mode" – the same full table experience (columns from the default template, inline editing, header/row menus for add/delete) that `<rw-input-collection>` gets by nesting `<rw-table>` via `[formArrayName]`. If you want the full table experience (OData column filters/sorting, context menus) for a standalone editable list outside of a nested collection property, use `<rw-signal-table>` directly instead.

### Enabling CSS status classes

Signal Forms does not automatically add `.ng-touched` / `.ng-invalid` / `.ng-dirty` / `.ng-valid` classes to fields the way Reactive Forms does. `provideRestWorld()` (see [Angular Client Setup](client-development-angular-setup.md)) already provides [`provideSignalFormsConfig({ classes: NG_STATUS_CLASSES })`](https://angular.dev/guide/forms/signals/migration#automatic-status-classes) by default, so these classes are applied automatically and any custom CSS you write against them (for example, styling invalid inputs) works the same as with `<rw-form>`. Pass a custom `SignalFormsConfig` as the second argument to `provideRestWorld()` if you want to opt out or customize which classes are applied.

## `<rw-list>` and `<rw-edit>` (views)

`<rw-list>` (`RESTworldListViewComponent`) and `<rw-edit>` (`RESTworldEditViewComponent`) are full page-level views that compose the components above into ready-to-route CRUD pages, typically wired up directly in `app.routes.ts` (see [Angular Client Setup](client-development-angular-setup.md#define-routes-and-menus)).

- `<rw-list apiName="ExampleBlog" rel="MyEx:Post"></rw-list>` loads the list resource and its `search` template, then nests `<rw-table>` with header/row menus for creating and editing/deleting rows (linking to `<rw-edit>` by default via `editLink`).
- `<rw-edit apiName="ExampleBlog" rel="MyEx:Post" [uri]="uri"></rw-edit>` loads the resource's forms and renders one `<rw-form>` per template (e.g. "Edit", "Create") in a tab view, plus an ID-navigation control (`<rw-id-navigation>`) and a "Create new" button. Add extra tabs via the `#extraTabs` content template.

## `<rw-signal-list>` and `<rw-signal-edit>` (Signal Forms views)

`<rw-signal-list>` (`RESTworldSignalListViewComponent`) and `<rw-signal-edit>` (`RESTworldSignalEditViewComponent`) are the [Signal Forms](https://angular.dev/guide/forms/signals/overview) equivalents of `<rw-list>`/`<rw-edit>` – identical inputs, outputs, and routing shape, but nesting `<rw-signal-table>`/`<rw-signal-form>` instead of `<rw-table>`/`<rw-form>`:

```html
<rw-signal-list apiName="ExampleBlog" rel="MyEx:Post"></rw-signal-list>
<rw-signal-edit apiName="ExampleBlog" rel="MyEx:Post" [uri]="uri"></rw-signal-edit>
```

`<rw-signal-list>` itself has no forms coupling (it only nests `<rw-signal-table>` for a read-only/OData-filtered grid), so its inputs (`apiName`, `rel`, `editLink`, `createButtonMenu`, `$filter`/`$orderby`/`$skip`/`$top`) have the same shape as `<rw-list>`. One difference: `editLink` defaults to `/edit/signal` (instead of `<rw-list>`'s `/edit`), so its row/create links point at `<rw-signal-edit>` out of the box - pass your own `editLink` to override this either way. `<rw-signal-edit>` reuses `<rw-id-navigation>` unchanged (it owns its own self-contained `FormGroup` for the ID field only, unrelated to the resource's own form, so there was nothing Signal-Forms-specific to port there) and swaps `<rw-form>` for `<rw-signal-form>` per tab.

The ExampleBlog sample app ([`src/Example/ExampleBlog/Exampleblog.Client.Angular`](../src/Example/ExampleBlog/Exampleblog.Client.Angular)) wires up both variants side by side: the "Examples with defaults" menu routes to `list/:apiName/:rel` / `edit/:apiName/:rel/:uri` (`<rw-list>`/`<rw-edit>`), and the "Examples with Signal Forms" menu routes to `list/signal/:apiName/:rel` / `edit/signal/:apiName/:rel/:uri` (`<rw-signal-list>`/`<rw-signal-edit>`), using the same `apiName`/`rel` combinations so you can compare both stacks against the same data.

Continue with [Angular Client Core Concepts](client-development-angular-core.md) for REST patterns or [Angular Client Setup](client-development-angular-setup.md) for end-to-end bootstrapping.
