import { Component, Injector, computed, effect, forwardRef, inject, input, model, runInInjectionContext, signal, untracked, viewChild } from '@angular/core';
import type { FieldTree } from '@angular/forms/signals';
import { Property, PropertyType, SignalFormService, SimpleValue, Template, type SignalForm } from '@wertzui/ngx-hal-client';
import { FilterMetadata, MenuItem, SelectItem, TranslationKeys, FilterService, type SortMeta } from 'primeng/api';
import { ODataParameters } from '../../models/o-data';
import { ODataService } from '../../services/odata.service';
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { ColumnFilter, Table, TableLazyLoadEvent, TableModule } from 'primeng/table';
import { DomHandler } from 'primeng/dom';
import { PrimeNG } from 'primeng/config';
import { RestWorldMenuButtonComponent } from "../restworld-menu-button/restworld-menu-button.component";
import { RestWorldSignalInputComponent } from "../restworld-signal-inputs/restworld-signal-input/restworld-signal-input.component";
import { RestWorldDisplayComponent } from "../restworld-displays/restworld-displays";
import { RestWorldSignalTableColumnFilterElementComponent } from "../restworld-signal-table-column-filter-element/restworld-signal-table-column-filter-element.component";
import { Router, ActivatedRoute } from "@angular/router";
import { getChildFieldTree, getFieldTreeItemAt } from '../../util/field-tree';

/**
 * Displays a table based on a search-, an edit-template and a list of items, using Signal Forms.
 * This is the Signal Forms equivalent of {@link RestWorldTableComponent} `<rw-table>`.
 * The search-template is required and used to display the table columns and to filter and sort the items.
 * The edit-template is optional and used to edit the items.
 * The items are displayed as table rows.
 * The table supports lazy loading, row selection, row menus, and context menus.
 * @remarks
 * Unlike `<rw-table>`, which relies on `ControlContainer`/`FormArrayName` to find its ancestor `FormArray`,
 * this component supports two modes (Signal Forms has no `ControlContainer` equivalent):
 * - Default/unbound mode: the component owns its own array of `SignalForm`s internally, rebuilt whenever
 *   `rows()` or `editTemplate()` change (only while `lazy()` is `true`). Use {@link editedRows} to read the
 *   current (possibly edited, unsaved) row values.
 * - Bound mode: set the {@link field} input to an externally-owned `FieldTree<ReadonlyArray<TListItem>>`
 *   (e.g. a `Collection` property's field from a parent Signal Form) to edit that array's items in place
 *   instead, mirroring `<rw-table>`'s `[formArrayName]` binding (used by e.g. `<rw-input-collection>`).
 *
 * @example
 * <rw-signal-table
 *   [apiName]="apiName"
 *   [searchTemplate]="searchTemplate"
 *   [editTemplate]="editTemplate"
 *   [rows]="rows"
 *   [rowsPerPageOptions]="[10, 25, 50]"
 *   [headerMenu]="headerMenu"
 *   [rowMenu]="rowMenu"
 *   [rowTrackBy]="rowTrackBy"
 *   [rowStyleClass]="rowStyleClass"
 *   [cellStyleClass]="cellStyleClass"
 *   [totalRecords]="totalRecords"
 *   [multiSortMeta]="multiSortMeta"
 *   [styleClass]="styleClass"
 *   [tableStyle]="tableStyle"
 *   [scrollable]="scrollable"
 *   [scrollHeight]="scrollHeight"
 *   [selectionMode]="selectionMode"
 *   [rowHover]="rowHover"
 *   [selection]="selection"
 *   [contextMenuItems]="contextMenuItems"
 *   [isLoading]="isLoading"
 *   [(selectedRows)]="selectedRows"
 *   [(oDataParameters)]="oDataParameters"
 * </rw-signal-table>
 *
 */
@Component({
    selector: 'rw-signal-table',
    templateUrl: './restworld-signal-table.component.html',
    styleUrls: ['./restworld-signal-table.component.css'],
    imports: [TableModule, RestWorldMenuButtonComponent, forwardRef(() => RestWorldSignalInputComponent), RestWorldDisplayComponent, ContextMenuModule, forwardRef(() => RestWorldSignalTableColumnFilterElementComponent)]
})
export class RestWorldSignalTableComponent<TListItem extends Record<string, any>> {
    /**
     * Compares two row-like objects by the given multi-sort meta, the same way `<rw-table>`'s `onSort` does.
     */
    private compareBySortMeta(a: Record<string, unknown>, b: Record<string, unknown>, multisortmeta: SortMeta[]): number {
        for (const sortMeta of multisortmeta) {
            const field = sortMeta.field;
            const valueA = a[field];
            const valueB = b[field];
            const order = sortMeta.order ?? 1;
            if (valueA === valueB)
                continue;
            if (valueA === undefined || valueA === null)
                return -1 * order;
            if (valueB === undefined || valueB === null)
                return 1 * order;
            if ((valueA as any) < (valueB as any))
                return -1 * order;
            if ((valueA as any) > (valueB as any))
                return 1 * order;
        }
        return 0;
    }

    onSort($event: { multisortmeta: SortMeta[] }) {
        if (this.lazy())
            return;

        // When bound to an externally-owned `field()`, sort its value array in place. The Signal Forms
        // FieldTree items for the array re-derive from the (re-ordered) value array, so this keeps
        // `getFieldAtIndex` aligned with the sorted rows the same way `<rw-table>`'s `onSort` keeps its
        // `formArray.controls` order aligned by sorting them directly.
        const boundField = this.field();
        if (boundField) {
            const currentRows = boundField().value();
            const sortedRows = [...currentRows].sort((a, b) => this.compareBySortMeta(a as Record<string, unknown>, b as Record<string, unknown>, $event.multisortmeta));
            // Only write back an actually-reordered array. `rows()` (derived from this field's value) feeds
            // `<p-table>`'s `[value]` input, and `p-table` re-runs `sortMultiple()` (re-emitting `onSort`)
            // whenever `value` changes while `multiSortMeta` is set. Calling `.set()` unconditionally would
            // therefore always produce a brand-new (if identically-ordered) array reference, which `p-table`
            // would immediately re-sort and re-emit `onSort` for again, ad infinitum (NG0103). Since sorting
            // an already-sorted array with the same comparator is idempotent, comparing by reference here is
            // enough to detect the no-op case and break that feedback loop.
            const isAlreadyInSortedOrder = currentRows.length === sortedRows.length && currentRows.every((row, i) => row === sortedRows[i]);
            if (!isAlreadyInSortedOrder)
                boundField().value.set(sortedRows);
            return;
        }

        // sort the signal forms for the indexes to line up with the sorted rows
        const signalForms = this._signalForms();
        if (signalForms.length === 0)
            return;

        signalForms.sort((a, b) => {
            for (const sortMeta of $event.multisortmeta) {
                const field = sortMeta.field;
                const valueA = a.model()[field];
                const valueB = b.model()[field];
                const order = sortMeta.order ?? 1;
                if (valueA === valueB)
                    continue;
                if (valueA === undefined || valueA === null)
                    return -1 * order;
                if (valueB === undefined || valueB === null)
                    return 1 * order;
                if (valueA < valueB)
                    return -1 * order;
                if (valueA > valueB)
                    return 1 * order;
            }
            return 0;
        });

        this._signalForms.set([...signalForms]);
    }
    public readonly PropertyType = PropertyType;
    /**
     * The name of the api.
     * For the editing capability, you must also set the editTemplate.
     */
    public readonly apiName = input.required<string>();
    /**
     * A function that returns the style class for a cell.
     * @param row The row for which to return the style class.
     * @param column The column for which to return the style class.
     * @param rowIndex The index of the row on the currently displayed page.
     * @param columnIndex The index of the column.
     * @returns The style class for the cell.
     */
    public readonly cellStyleClass = input<(row: TListItem, column: Property<SimpleValue, string, string>, rowIndex: number, columnIndex: number) => string>(() => "");
    public readonly cellStyleClasses = computed(() => this.rows().map((r, ri) => Object.fromEntries<string>(this.columns().map((c, ci) => [c.name, this.cellStyleClass()(r, c, ri, ci)]))));
    public readonly columns = computed(() => this.searchTemplate()?.properties.filter(p => p.type !== PropertyType.Hidden) ?? []);
    public readonly contextMenu = viewChild<ContextMenu>("contextMenu");
    public readonly primeNgTable = viewChild.required<Table>("table");
    public readonly contextMenuItems = signal<MenuItem[]>([]);
    public readonly dateFormat = new Date(3333, 10, 22) // months start at 0 in JS
        .toLocaleDateString()
        .replace("22", "dd")
        .replace("11", "MM")
        .replace("3333", "yy")
        .replace("33", "y");
    public readonly editProperties = computed(() => this.editTemplate()?.propertiesRecord ?? {});
    /**
     * The template that is used to edit the items.
     * Bind this to the template that is used to edit the items.
     * Normally this is returned from the backend as part of the hal-forms resource from a list endpoint.
     * For the editing capability, you must also set the apiName.
     */
    public readonly editTemplate = input<Template>();
    /**
     * An optional `FieldTree` to bind the table to, for editing a `Collection` property that is already part
     * of a parent Signal Form (mirrors `<rw-table>`'s `ControlContainer`/`formArrayName` binding). Used by
     * {@link RestWorldSignalInputCollectionComponent} `<rw-signal-input-collection>`, which nests this
     * component bound to its own collection field, exactly like `<rw-input-collection>` nests `<rw-table>`.
     * @remarks When set, the table edits the array's items in place through this `FieldTree` (bidirectionally
     * bound to the parent form) instead of building its own internally-owned array of `SignalForm`s from
     * `rows()`/`editTemplate()`. `apiName()` and `editTemplate()` are still required for column rendering.
     */
    public readonly field = input<FieldTree<ReadonlyArray<TListItem>>>();
    /**
     * The current (possibly edited, unsaved) values of the rows.
     * Reads from the bound {@link field} if set, otherwise from the internally-owned signal forms.
     * Use this to access edited values instead of a `FormArray` (which Signal Forms has no equivalent of).
     */
    public readonly editedRows = computed<TListItem[]>(() => {
        const field = this.field();
        if (field)
            return [...field().value()];

        return this._signalForms().map(f => f.model() as TListItem);
    });
    public readonly filters = computed(() => {
        const filter = this.oDataParameters().$filter;
        const properties = this.searchTemplate()?.propertiesRecord;
        if (filter === null || filter === undefined || typeof filter !== "string" || properties === undefined)
            return {};

        return ODataService.createFilterMetadataFromODataFilter(filter, properties);
    });
    /**
     * An optional menu that is displayed at the top right of the table.
     * @see RestWorldMenuButtonComponent
     */
    public readonly headerMenu = input<MenuItem[]>([]);
    public readonly isEditable = computed(() => this.editTemplate() !== undefined && this.apiName() !== undefined && (this.field() !== undefined || this.lazy()));
    /**
     * Indicates whether the table is currently loading.
     * Set this to true while loading new items from the backend when reacting to the `onFilterOrSortChanged` event.
     */
    public readonly isLoading = input(false);

    /**
     * Indicates whether the table is lazy loaded.
     * If set to true, sorting and filtering needs to be handled by the `load` event.
     * If set to false, sorting and filtering is handled by the table component itself.
     * The default is `true`.
     * @see load
     */
    public readonly lazy = input(true);

    /**
     * Indicates whether the table has a paginator.
     * If set to true, the table will display a paginator at the bottom.
     * If set to false, the table will not display a paginator and all rows will be displayed at once.
     * The default is `true`.
     * In order to customize the number of rows per page, you can set the `rowsPerPageOptions` property.
     * @see rowsPerPageOptions
     */
    public readonly paginator = input(true);
    public readonly multiSortMeta = computed(() => {
        const orderBy = this.oDataParameters().$orderby;
        if (orderBy === null || orderBy === undefined || typeof orderBy !== "string")
            return undefined;

        return orderBy
            .split(",")
            .map(o => o.trim())
            .filter(o => o !== "")
            .map(o => {
                const [field, order] = o.split(" ");
                const orderAsNumber = order?.toLowerCase() === "desc" ? -1 : 1;
                return { field: field, order: orderAsNumber };
            });
    });
    public readonly oDataParameters = model<ODataParameters>({});
    public readonly reflectParametersInUrl = input(true);
    /**
     * Indicates whether the table rows are highlighted when the mouse hovers over them.
     */
    public readonly rowHover = input<boolean>(false);
    /**
     * A function that returns the menu for a row.
     * Based on the openedByRightClick parameter, the function can return different menus.
     * The menu when it has not been opened by a right click is displayed in an extra column to the right of the table if `showRowMenuAsColumn` is `true`.
     * The menu when it has been opened by a right click is displayed as a context menu if `showRowMenuOnRightClick` is `true`.
     * @param row The row for which to return the menu.
     * @param openedByRightClick Indicates whether the menu was opened by a right click.
     * @returns The menu for the row.
     * @see showRowMenuAsColumn
     * @see showRowMenuOnRightClick
    */
    public readonly rowMenu = input<(row: TListItem, openedByRightClick: boolean) => MenuItem[]>(() => []);
    public readonly rowMenus = computed(() => {
        return this.showRowMenuAsColumn() ? this.rows().map(r => this.rowMenu()(r, false)) : [];
    });
    /**
     * A function used to track each row, passed straight through to `p-table`'s own `rowTrackBy` input.
     * Defaults to tracking by index instead of `p-table`'s own default of comparing row objects by identity.
     * @remarks When bound to a `field()` (e.g. from `<rw-signal-input-collection>`), `rows()` comes straight from
     * a `FieldTree`'s `value` signal. Signal Forms derives that array (and a brand-new object for the edited item)
     * every time any nested field changes, since its value signals are immutable snapshots, not mutated in place.
     * With `p-table`'s default identity-based `rowTrackBy`, this makes it destroy and recreate the *entire* edited
     * row's DOM subtree - including any active PrimeNG components - on every single edit. For a `p-inputNumber`,
     * this destroys its spin button while the button's own "hold to repeat" timer (started on `mousedown`) is still
     * running: the `mouseup` that would normally stop it never reaches the now-detached button, so the timer keeps
     * firing forever, making the value increase/decrease indefinitely. Since edits never change a row's position in
     * the array, tracking by index (the default) keeps the same row view (and therefore the same component
     * instances) alive across edits, avoiding both the expensive re-creation and the resulting runaway spin timer.
     * Override this input if you need a different tracking strategy (e.g. tracking by a stable row ID), but be
     * aware that reverting to identity-based tracking will reintroduce the issue described above for bound fields.
     */
    public readonly rowTrackBy = input<(index: number, item: TListItem) => unknown>((index) => index);
    /**
     * A function that returns the style class for a row.
     * @param row The row for which to return the style class.
     * @param rowIndex The index of the row on the currently displayed page.
     * @returns The style class for the row.
     */
    public readonly rowStyleClass = input<(row: TListItem, rowIndex: number) => string>(() => "");
    public readonly rowStyleClasses = computed(() => this.rows().map((r, i) => this.rowStyleClass()(r, i)));
    /**
     * The items that are displayed as table rows.
     * Bind this to the items that are displayed as table rows.
     * Normally this is returned from the backend as part of the hal-forms resource from a list endpoint.
     */
    public readonly rows = input.required<TListItem[]>();
    public readonly rowsBeforeCurrentPage = computed(() => this.oDataParameters().$skip ?? 0);
    /**
     * The number of rows per page.
     * The default is the first element of rowsPerPageOptions.
     */
    public readonly rowsPerPage = computed(() => this.oDataParameters().$top ?? this.rowsPerPageOptions()[0]);
    /**
     * The possible values for the number of rows per page.
     * The default is [10, 25, 50].
     */
    public readonly rowsPerPageOptions = input([10, 25, 50]);
    /**
     * The height of the scrollable table.
     * The default is "flex".
     */
    public readonly scrollHeight = input<string>("flex");
    /**
     * Indicates whether the table is scrollable.
     * The default is `true`.
     */
    public readonly scrollable = input<boolean>(true);
    /**
     * The template that is used to display the table columns and to filter and sort the items.
     * Bind this to the template that is used to display the table columns and to filter and sort the items.
     * Normally this is returned from the backend as part of the hal-forms resource from a list endpoint.
     */
    public readonly searchTemplate = input.required<Template>();
    /**
     * The currently selected rows.
     */
    public readonly selectedRows = model<TListItem[]>([]);
    /**
     * The mode how rows can be selected.
     * The default is `null` which means rows cannot be selected.
     */
    public readonly selectionMode = input<"single" | "multiple" | null>(null);
    public readonly showMenuColumn = computed(() => this.headerMenu().length > 0 || (this.showRowMenuAsColumn() && this.rowMenus().some(m => m.length > 0)));
    /**
     * Indicates whether the row menu is displayed as a column to the right of the table.
     */
    public readonly showRowMenuAsColumn = input(true);
    /**
     * Indicates whether the row menu is displayed as a context menu when the user right clicks on a row.
     */
    public readonly showRowMenuOnRightClick = input(true);
    /**
     * The style class for the table.
     * The default is "".
     */
    public readonly styleClass = input<string>("");
    /**
     * The inline style for the table.
     */
    public readonly tableStyle = input<Record<string, string>>();
    public readonly totalRecords = input(0);
    public readonly urlParameterPrefix = input("");

    /**
     * The `SignalForm`s (model signal + field tree) for each row, rebuilt whenever `rows()` or `editTemplate()`
     * change. This is the Signal Forms replacement for `<rw-table>`'s `formArray`.
     */
    private readonly _signalForms = signal<SignalForm<TListItem>[]>([]);

    private readonly _filterMatchModeOptions: { [K in keyof PrimeNG["filterMatchModeOptions"]]: SelectItem[] } & { "boolean": SelectItem[] } & { "enum": SelectItem[] };
    private readonly timeFormat = new Date(1, 1, 1, 22, 33, 44)
        .toLocaleTimeString()
        .replace("22", "hh")
        .replace("33", "mm")
        .replace("44", "ss");

    private readonly _injector = inject(Injector);
    private _initialQueryParamsSet = false;
    private _lastUsedFilters: Partial<Record<string, FilterMetadata | FilterMetadata[]>> = {};

    public constructor(
        private readonly _signalFormService: SignalFormService,
        router: Router,
        activatedRoute: ActivatedRoute,
        primeNGConfig: PrimeNG,
        filterService: FilterService) {
        this._filterMatchModeOptions = {
            text: [TranslationKeys.NO_FILTER, ...primeNGConfig.filterMatchModeOptions.text].map(o => ({ label: primeNGConfig.getTranslation(o), value: o })),
            numeric: [TranslationKeys.NO_FILTER, ...primeNGConfig.filterMatchModeOptions.numeric].map(o => ({ label: primeNGConfig.getTranslation(o), value: o })),
            date: [TranslationKeys.NO_FILTER, ...primeNGConfig.filterMatchModeOptions.date].map(o => ({ label: primeNGConfig.getTranslation(o), value: o })),
            boolean: [TranslationKeys.NO_FILTER, TranslationKeys.EQUALS, TranslationKeys.NOT_EQUALS].map(o => ({ label: primeNGConfig.getTranslation(o), value: o })),
            enum: [TranslationKeys.NO_FILTER, TranslationKeys.EQUALS, TranslationKeys.NOT_EQUALS].map(o => ({ label: primeNGConfig.getTranslation(o), value: o })),
        };

        filterService.register(TranslationKeys.NO_FILTER, () => true);

        // Rebuild the signal forms for the rows whenever the rows or edit template change. Not needed (and
        // skipped) when bound to an externally-owned `field()`, since that FieldTree already provides one
        // FieldTree per item directly.
        // @remarks `form()` (used internally by `SignalFormService.createSignalFormFromTemplate`) internally
        // calls both `inject()` and `effect()`, so the creation must run inside `untracked()` (to escape this
        // effect's own reactive context) wrapped around `runInInjectionContext()` (for the `inject()` call) -
        // see `RestWorldSignalFormComponent`'s `signalForm` for the same pattern with more detail.
        effect(() => {
            const editTemplate = this.editTemplate();
            const rows = this.rows();
            const lazy = this.lazy();
            const boundField = this.field();

            if (boundField !== undefined || !this.isEditable() || !editTemplate || !lazy) {
                this._signalForms.set([]);
                return;
            }

            const newSignalForms = untracked(() => rows.map(row =>
                runInInjectionContext(this._injector, () => this._signalFormService.createSignalFormFromTemplate(editTemplate, row)) as SignalForm<TListItem>));

            this._signalForms.set(newSignalForms);
        });

        // update the url when the oDataParameters change
        effect(async () => {
            if (!this.reflectParametersInUrl())
                return;

            const urlParameterPrefix = this.urlParameterPrefix();
            const oDataParameters = this.oDataParameters();

            // Set the initial query parameters on the first change
            if (!this._initialQueryParamsSet) {
                this._initialQueryParamsSet = true;
                const oDataParametersFromUrl = ODataService.createParametersFromRoute(activatedRoute, urlParameterPrefix);
                const mergedParameters = { ...oDataParameters, ...oDataParametersFromUrl };
                this.oDataParameters.set(mergedParameters);
                return;
            }

            // Update the query parameters in the url after the first change
            const parameters = this.prefixObjectProperties(oDataParameters, urlParameterPrefix);
            await router.navigate([], { queryParams: parameters, queryParamsHandling: 'merge' });
        });
    }

    /**
     * Gets the `FieldTree` for the given column of the row at the given index on the current page, or
     * `undefined` if the table is not editable or the column has no corresponding field.
     * Reads from the bound {@link field} if set, otherwise from the internally-owned signal forms.
     * @param indexOnCurrentPage The index of the row on the currently displayed page.
     * @param columnName The name of the column (property) to get the field for.
     */
    public getFieldAtIndex(indexOnCurrentPage: number, columnName: string): FieldTree<unknown> | undefined {
        const finalIndex = this.getAbsoluteIndex(indexOnCurrentPage);

        const boundField = this.field();
        if (boundField) {
            const itemField = getFieldTreeItemAt<Record<string, unknown>>(boundField as unknown as FieldTree<ReadonlyArray<unknown>>, finalIndex);
            if (!itemField)
                return undefined;

            return getChildFieldTree(itemField, columnName);
        }

        const signalForm = this._signalForms()[finalIndex];
        if (!signalForm)
            return undefined;

        return getChildFieldTree(signalForm.form as unknown as FieldTree<Record<string, unknown>>, columnName);
    }

    public getAbsoluteIndex(indexOnCurrentPage: number): number {
        const lazy = this.lazy();
        if (!lazy)
            return indexOnCurrentPage;
        const primeNgTable = this.primeNgTable();
        if (!primeNgTable)
            return indexOnCurrentPage;
        return indexOnCurrentPage - (primeNgTable.first ?? 0);
    }

    public load(event: TableLazyLoadEvent) {
        this.fixUserFilterErrors(event.filters);
        const currentParameters = this.oDataParameters();
        const searchTemplate = this.searchTemplate();
        if (!searchTemplate || searchTemplate.properties.length === 0)
            return;

        const parameters = ODataService.createParametersFromTableLoadEvent(event, searchTemplate);
        ODataService.createFilterMetadataFromODataFilter(parameters.$filter, searchTemplate.propertiesRecord);
        if (currentParameters.$filter !== parameters.$filter || currentParameters.$orderby !== parameters.$orderby || currentParameters.$top !== parameters.$top || currentParameters.$skip !== parameters.$skip)
            this.oDataParameters.set(parameters);
    }

    public openContextMenu(event: MouseEvent, row: TListItem): void {
        const contextMenu = this.contextMenu();
        if (!this.showRowMenuOnRightClick() || contextMenu === undefined)
            return;

        this.contextMenuItems.set(this.rowMenu()(row, true));
        contextMenu.show(event);

        event.stopPropagation();
    }

    /**
     * Focuses the filter value input inside the column filter's overlay instead of the first focusable
     * element (which is normally the match-mode/operator dropdown), so the user can directly type a value.
     * @remarks Runs in a `setTimeout` macrotask so it executes after `ColumnFilter.focusOnFirstElement()`,
     * which PrimeNG calls synchronously right before emitting the `onShow` event this is bound to.
     * @param columnFilter The `#f` template reference of the `<p-columnFilter>` that was just opened.
     */
    public focusColumnFilterValue(columnFilter: ColumnFilter): void {
        setTimeout(() => {
            const overlay = columnFilter.overlay;
            const valueContainer = overlay?.querySelector<HTMLElement>('.rw-column-filter-value');
            const target = valueContainer ? DomHandler.getFirstFocusableElement(valueContainer) : null;
            target?.focus();
        });
    }

    /**
     * Applies the column filter (same as clicking the "Apply" button) when the user presses Enter while
     * typing in the filter value input.
     * @param event The keydown event.
     * @param columnFilter The `#f` template reference of the `<p-columnFilter>` the value belongs to.
     */
    public applyColumnFilterOnEnter(event: KeyboardEvent, columnFilter: ColumnFilter): void {
        event.preventDefault();
        columnFilter.applyFilter();
    }

    public showInputField(column: Property): boolean {
        if (!this.isEditable())
            return false;

        const editProperty = this.editProperties()[column.name];

        return editProperty !== undefined && editProperty.type !== PropertyType.Hidden && !editProperty.readOnly;
    }

    public toColumnFilterType(property: Property<SimpleValue, string, string> | undefined): ColumnFilterType {
        if (!property)
            return ColumnFilterType.text;

        const propertyType = property.type;
        switch (propertyType) {
            case PropertyType.Number:
            case PropertyType.Percent:
            case PropertyType.Currency:
            case PropertyType.Month:
                return ColumnFilterType.numeric;
            case PropertyType.Bool:
                return ColumnFilterType.boolean;
            case PropertyType.Date:
            case PropertyType.DatetimeLocal:
            case PropertyType.DatetimeOffset:
                return ColumnFilterType.date;
            default:
                return property.options ? property.options.link ? ColumnFilterType.numeric : ColumnFilterType.enum : ColumnFilterType.text;
        }
    }

    public toMatchModeOptions(property: Property<SimpleValue, string, string>): SelectItem<any>[] | undefined {
        const columnFilterType = this.toColumnFilterType(property);
        return this._filterMatchModeOptions[columnFilterType];
    }

    public toMaxFractionDigits(property: Property<SimpleValue, string, string>): number | undefined {
        switch (property.type) {
            case PropertyType.Number:
            case PropertyType.Percent:
            case PropertyType.Currency:
                return property.step?.toString().split(".")[1]?.length ?? 2;
            case PropertyType.Month:
                return 0;
            default:
                return undefined;
        }
    }

    private fixUserFilterError(filterEntry: FilterMetadata | undefined, lastFilterEntry: FilterMetadata | undefined, propertyName: string) {
        if (!filterEntry)
            return;

        if (
            lastFilterEntry !== undefined &&
            lastFilterEntry.matchMode !== TranslationKeys.NO_FILTER &&
            filterEntry.matchMode === TranslationKeys.NO_FILTER) {
            // The user changed the mode from something to no filter
            // => We reset the value
            filterEntry.value = null;
        }
        else if (
            filterEntry.matchMode === TranslationKeys.NO_FILTER &&
            (lastFilterEntry === undefined || lastFilterEntry.value === null) &&
            filterEntry.value !== null) {
            // The user entered a value into the filter, but forgot to change the mode
            // => We set the match mode to the default for the type that is not no filter
            filterEntry.matchMode = this._filterMatchModeOptions[this.toColumnFilterType(this.searchTemplate().propertiesRecord[propertyName])][1].value;
        }
    }

    private fixUserFilterErrors(filters?: Partial<Record<string, FilterMetadata | FilterMetadata[]>>) {
        if (!filters)
            return;

        Object.entries(filters).forEach(([propertyName, filter]) => {
            const lastFilter = this._lastUsedFilters[propertyName];
            if (Array.isArray(filter)) {
                filter.forEach((filterEntry, index) => this.fixUserFilterError(filterEntry, Array.isArray(lastFilter) ? lastFilter[index] : lastFilter, propertyName));
            }
            else {
                this.fixUserFilterError(filter, Array.isArray(lastFilter) ? lastFilter[0] : lastFilter, propertyName);
            }
        });
        this._lastUsedFilters = JSON.parse(JSON.stringify(filters));
    }

    private prefixObjectProperties<T extends Record<string, any>, P extends string>(
        obj: T,
        prefix: P
    ): { [K in keyof T as `${P}${string & K}`]: T[K] } {
        return Object.fromEntries(
            Object.entries(obj).map(([key, value]) => [`${prefix}${key}`, value])
        ) as { [K in keyof T as `${P}${string & K}`]: T[K] };
    }
}

enum ColumnFilterType {
    text = 'text',
    numeric = 'numeric',
    boolean = 'boolean',
    date = 'date',
    enum = 'enum',
}
