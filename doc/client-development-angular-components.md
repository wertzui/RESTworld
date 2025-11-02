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

Continue with [Angular Client Core Concepts](client-development-angular-core.md) for REST patterns or [Angular Client Setup](client-development-angular-setup.md) for end-to-end bootstrapping.
