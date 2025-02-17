import { Component, Optional, computed, effect, input, model, signal, viewChild } from '@angular/core';
import { FormService, Property, PropertyType, SimpleValue, Template } from '@wertzui/ngx-hal-client';
import { FilterMetadata, MenuItem, SelectItem, TranslationKeys } from 'primeng/api';
import { ODataParameters } from '../../models/o-data';
import { ODataService } from '../../services/odata.service';
import { ContextMenu, ContextMenuModule } from 'primeng/contextmenu';
import { AbstractControl, ControlContainer, FormArray, FormArrayName, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { TableLazyLoadEvent, TableModule } from 'primeng/table';
import { PrimeNG } from 'primeng/config';
import { RestWorldMenuButtonComponent } from "../restworld-menu-button/restworld-menu-button.component";
import { RestWorldInputComponent } from "../restworld-inputs/restworld-inputs";
import { RestWorldDisplayComponent } from "../restworld-displays/restworld-displays";
import { RestWorldTableColumnFilterElementComponent } from "../rest-world-table-column-filter-element/rest-world-table-column-filter-element.component";
import { Router, ActivatedRoute } from "@angular/router";

/**
 * Displays a table based on a search-, an edit-template and a list of items.
 * The search-template is required and used to display the table columns and to filter and sort the items.
 * The edit-template is optional and used to edit the items. For the edit capability, the table must be part of a reactive form.
 * The items are displayed as table rows.
 * The table supports lazy loading, row selection, row menus, and context menus.
 *
 * @example
 * <rw-table
 *   [apiName]="apiName"
 *   [searchTemplate]="searchTemplate"
 *   [editTemplate]="editTemplate"
 *   [rows]="rows"
 *   [rowsPerPageOptions]="[10, 25, 50]"
 *   [headerMenu]="headerMenu"
 *   [rowMenu]="rowMenu"
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
 * </rw-table>
 *
 */
@Component({
    selector: 'rw-table',
    templateUrl: './restworld-table.component.html',
    styleUrls: ['./restworld-table.component.css'],
    standalone: true,
    viewProviders: [{
        provide: ControlContainer,
        deps: [[Optional, FormArrayName]],
        useFactory: (arrayName: FormArrayName) => arrayName,
    }],
    imports: [TableModule, RestWorldMenuButtonComponent, RestWorldInputComponent, RestWorldDisplayComponent, ReactiveFormsModule, ContextMenuModule, RestWorldTableColumnFilterElementComponent, RestWorldTableColumnFilterElementComponent]
})
export class RestWorldTableComponent<TListItem extends Record<string, any>> {
    public readonly PropertyType = PropertyType;
    /**
     * The name of the api.
     * For the editing capability, you must also set the editTemplate and the formArray.
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
     * For the editing capability, you must also set the apiName and the formArray.
     */
    public readonly editTemplate = input<Template>();
    public readonly filters = computed(() => {
        const filter = this.oDataParameters().$filter;
        const properties = this.searchTemplate()?.propertiesRecord;
        if (filter === null || filter === undefined || typeof filter !== "string" || properties === undefined)
            return {};

        return ODataService.createFilterMetadataFromODataFilter(filter, properties);
    });
    /**
     * The form array that contains the form groups for the items.
     * Bind this to the form array that contains the form groups for the items.
     * Each entry in the array represents one row in the currently displayed page of the table.
     * For the editing capability, you must also set the apiName and the editTemplate.
     */
    public readonly formArray = computed(() => this._controlContainer?.control as FormArray<FormGroup<{ [K in keyof TListItem]: AbstractControl<unknown> }>> | undefined);
    /**
     * An optional menu that is displayed at the top right of the table.
     * @see RestWorldMenuButtonComponent
     */
    public readonly headerMenu = input<MenuItem[]>([]);
    public readonly isEditable = computed(() => this.editTemplate() !== undefined && this.formArray() !== undefined && this.apiName() !== undefined);
    /**
     * Indicates whether the table is currently loading.
     * Set this to true while loading new items from the backend when reacting to the `onFilterOrSortChanged` event.
     */
    public readonly isLoading = input(false);
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
     * The default is 10.
     */
    public readonly rowsPerPage = input(10);
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

    //   private _formArray?: FormArray<FormGroup<{ [K in keyof TListItem]: AbstractControl<unknown> }>>;
    private readonly _filterMatchModeOptions: { [K in keyof PrimeNG["filterMatchModeOptions"]]: SelectItem[] } & { "boolean": SelectItem[] } & { "enum": SelectItem[] };
    private readonly timeFormat = new Date(1, 1, 1, 22, 33, 44)
        .toLocaleTimeString()
        .replace("22", "hh")
        .replace("33", "mm")
        .replace("44", "ss");

    private _initialQueryParamsSet = false;
    private _lastUsedFilters: Partial<Record<string, FilterMetadata | FilterMetadata[]>> = {};

    public constructor(
        private readonly _controlContainer: ControlContainer,
        private readonly _formService: FormService,
        router: Router,
        activatedRoute: ActivatedRoute,
        primeNGConfig: PrimeNG) {
        this._filterMatchModeOptions = {
            text: [TranslationKeys.NO_FILTER, ...primeNGConfig.filterMatchModeOptions.text].map(o => ({ label: primeNGConfig.getTranslation(o), value: o })),
            numeric: [TranslationKeys.NO_FILTER, ...primeNGConfig.filterMatchModeOptions.numeric].map(o => ({ label: primeNGConfig.getTranslation(o), value: o })),
            date: [TranslationKeys.NO_FILTER, ...primeNGConfig.filterMatchModeOptions.date].map(o => ({ label: primeNGConfig.getTranslation(o), value: o })),
            boolean: [TranslationKeys.NO_FILTER, TranslationKeys.EQUALS, TranslationKeys.NOT_EQUALS].map(o => ({ label: primeNGConfig.getTranslation(o), value: o })),
            enum: [TranslationKeys.NO_FILTER, TranslationKeys.EQUALS, TranslationKeys.NOT_EQUALS].map(o => ({ label: primeNGConfig.getTranslation(o), value: o })),
        };

        // Update the form array on changes
        effect(() => {
            const formArray = this.formArray();
            const editTemplate = this.editTemplate();
            const rows = this.rows();

            if (!this.isEditable() || !formArray || !editTemplate)
                return;

            formArray.clear();

            const newControls = rows
                .map(r => {
                    const formGroup = this._formService.createFormGroupFromTemplate(editTemplate) as FormGroup;
                    formGroup.patchValue(r);
                    return formGroup;
                });

            for (const control of newControls)
                formArray.push(control);
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
                this.oDataParameters.set(this.prefixObjectProperties(oDataParametersFromUrl, urlParameterPrefix));
                return;
            }

            // Update the query parameters in the url after the first change
            const parameters = this.prefixObjectProperties(oDataParameters, urlParameterPrefix);
            await router.navigate([], { queryParams: parameters, queryParamsHandling: 'merge' });
        });
    }

    public load(event: TableLazyLoadEvent) {
        this.fixUserFilterErrors(event.filters);
        // this.multiSortMeta = event.multiSortMeta;
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
