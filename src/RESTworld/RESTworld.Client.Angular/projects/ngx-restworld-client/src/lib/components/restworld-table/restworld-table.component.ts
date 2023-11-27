import { Component, EventEmitter, Input, Optional, Output, ViewChild } from '@angular/core';
import { FormService, Property, PropertyType, SimpleValue, Template } from '@wertzui/ngx-hal-client';
import * as _ from 'lodash';
import { FilterMatchMode, MenuItem, SortMeta } from 'primeng/api';
import { ODataParameters } from '../../models/o-data';
import { ODataService } from '../../services/o-data.service';
import { ContextMenu } from 'primeng/contextmenu';
import { AbstractControl, ControlContainer, FormArray, FormArrayName, FormGroup } from '@angular/forms';
import { TableLazyLoadEvent, TableRowSelectEvent, TableRowUnSelectEvent } from 'primeng/table';

enum ColumnFilterType {
  text = 'text',
  numeric = 'numeric',
  boolean = 'boolean',
  date = 'date'
}

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
 *   (onFilterOrSortChanged)="onFilterOrSortChanged($event)"
 *   (onRowSelect)="onRowSelect($event)"
 *   (onRowUnselect)="onRowUnselect($event)"
 *   (selectionChange)="selectionChange($event)">
 * </rw-table>
 * 
 */
@Component({
  selector: 'rw-table',
  templateUrl: './restworld-table.component.html',
  styleUrls: ['./restworld-table.component.css'],
  viewProviders: [{
    provide: ControlContainer,
    deps: [[Optional, FormArrayName]],
    useFactory: (arrayName: FormArrayName) => arrayName, }]
  })
export class RestWorldTableComponent<TListItem extends Record<string, any>> {
  /**
   * The name of the api.
   * For the editing capability, you must also set the editTemplate and the formArray.
   */
  @Input()
  public apiName?: string;

  private _formArray?: FormArray<FormGroup<{[K in keyof TListItem] : AbstractControl<unknown>}>>;

  /**
   * The form array that contains the form groups for the items.
   * Bind this to the form array that contains the form groups for the items.
   * Each entry in the array represents one row in the currently displayed page of the table.
   * For the editing capability, you must also set the apiName and the editTemplate.
   */
  @Input()
  public set formArray(value: FormArray<FormGroup<{[K in keyof TListItem] : AbstractControl<unknown>}>> | undefined) {
    this._formArray = value;
  }

  public get formArray(): FormArray<FormGroup<{[K in keyof TListItem] : AbstractControl<unknown>}>> | undefined {
    return this._formArray ?? this._controlContainer?.control as FormArray<FormGroup<{[K in keyof TListItem] : AbstractControl<unknown>}>> | undefined;
  }

  private _searchTemplate: Template = new Template({properties: []});

  /**
   * The template that is used to display the table columns and to filter and sort the items.
   * Bind this to the template that is used to display the table columns and to filter and sort the items.
   * Normally this is returned from the backend as part of the hal-forms resource from a list endpoint.
   */
  @Input({ required: true })
  public set searchTemplate(value: Template){
    this._searchTemplate = value;
    this._columns = value?.properties.filter(p => p.type !== PropertyType.Hidden) ?? [];
    this.setSortFieldAndSortOrder(value)
  }

  public get searchTemplate() {
    return this._searchTemplate;
  }

  private _editTemplate?: Template;

  /**
   * The template that is used to edit the items.
   * Bind this to the template that is used to edit the items.
   * Normally this is returned from the backend as part of the hal-forms resource from a list endpoint.
   * For the editing capability, you must also set the apiName and the formArray.
   */
  @Input()
  public set editTemplate(value: Template | undefined){
    this._editTemplate = value;
    this.updateEditProperties();
    this.updateFormArray();
  }

  public get editTemplate() {
    return this._editTemplate;
  }

  private _editProperties: Record<string, Property<SimpleValue, string, string>> = {};
  public get editProperties() {
    return this._editProperties;
  }

  public get isEditable() {
    return this.editTemplate !== undefined && this.formArray !== undefined && this.apiName;
  }

  private _rows: TListItem[] = [];
  public get rows() {
    return this._rows;
  }

  /**
   * The items that are displayed as table rows.
   * Bind this to the items that are displayed as table rows.
   * Normally this is returned from the backend as part of the hal-forms resource from a list endpoint.
   */
  @Input({ required: true })
  public set rows(value: TListItem[]) {
    this._rows = value;
    this.updateCalculatedFunctionValues(value);
    this.updateFormArray();
  }

  /**
   * The possible values for the number of rows per page.
   * The default is [10, 25, 50].
   */
  @Input()
  public rowsPerPageOptions = [10, 25, 50];

  /**
   * An optional menu that is displayed at the top right of the table.
   * @see RestWorldMenuButtonComponent
   */
  @Input()
  public headerMenu: MenuItem[] = [];

  private _rowMenu: (row: TListItem, openedByRightClick: boolean) => MenuItem[] = () => [];
  public get rowMenu(): (row: TListItem, openedByRightClick: boolean) => MenuItem[] {
    return this._rowMenu;
  }

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
  @Input()
  public set rowMenu(value: (row: TListItem, openedByRightClick: boolean) => MenuItem[]) {
    this._rowMenu = value;
    this.updateRowMenus(this.rows);
  }

  private _rowMenus: MenuItem[][] = [];

  private updateCalculatedFunctionValues(value: TListItem[]) {
    this.updateRowMenus(value);
    this.updateRowStyles(value);
    this.updateCellStyles(value);
  }

  private updateCellStyles(value: TListItem[]) {
    this._cellStyleClasses = value.map((r, ri) => Object.fromEntries(this.columns.map((c, ci) => [c.name, this.cellStyleClass(r, c, ri, ci)])));
  }

  private updateRowStyles(value: TListItem[]) {
    this._rowStyleClasses = value.map((r, i) => this.rowStyleClass(r, i));
  }

  private updateRowMenus(value: TListItem[]) {
    if (this.showRowMenuAsColumn)
      this._rowMenus = value.map(r => this.rowMenu(r, false));
  }

  public get rowMenus() {
    return this._rowMenus;
  }

  public get showMenuColumn() {
    return this.headerMenu.length > 0 || (this.showRowMenuAsColumn && this.rowMenus.some(m => m.length > 0));
  }

  /**
   * Indicates whether the row menu is displayed as a column to the right of the table.
   */
  @Input()
  public showRowMenuAsColumn = true;

  /**
   * Indicates whether the row menu is displayed as a context menu when the user right clicks on a row.
   */
  @Input()
  public showRowMenuOnRightClick = true;

  private _rowStyleClass: (row: TListItem, rowIndex: number) => string = () => "";
  public get rowStyleClass(): (row: TListItem, rowIndex: number) => string {
    return this._rowStyleClass;
  }

  /**
   * A function that returns the style class for a row.
   * @param row The row for which to return the style class.
   * @param rowIndex The index of the row on the currently displayed page.
   * @returns The style class for the row.
   */
  @Input()
  public set rowStyleClass(value: (row: TListItem, rowIndex: number) => string) {
    this._rowStyleClass = value;
    this.updateRowStyles(this.rows);
  }

  private _rowStyleClasses: string[] = [];
  public get rowStyleClasses() {
    return this._rowStyleClasses;
  }

  private _cellStyleClass: (row: TListItem, column: Property<SimpleValue, string, string>, rowIndex: number, columnIndex: number) => string = () => "";
  public get cellStyleClass(): (row: TListItem, column: Property<SimpleValue, string, string>, rowIndex: number, columnIndex: number) => string {
    return this._cellStyleClass;
  }
  /**
   * A function that returns the style class for a cell.
   * @param row The row for which to return the style class.
   * @param column The column for which to return the style class.
   * @param rowIndex The index of the row on the currently displayed page.
   * @param columnIndex The index of the column.
   * @returns The style class for the cell.
   */
  @Input()
  public set cellStyleClass(value: (row: TListItem, column: Property<SimpleValue, string, string>, rowIndex: number, columnIndex: number) => string) {
    this._cellStyleClass = value;
    this.updateCellStyles(this.rows);
  }

  private _cellStyleClasses: Record<string,string>[] = [];
  public get cellStyleClasses() {
    return this._cellStyleClasses;
  }

  @Input()
  public totalRecords = 0;

  public constructor(
    private readonly _controlContainer: ControlContainer,
    private readonly _formService: FormService,) {
  }

  private setSortFieldAndSortOrder(template: Template | undefined) {
    const orderBy = template?.properties?.find(p => p?.name === "$orderby")?.value;

    if(orderBy === null || orderBy === undefined || typeof orderBy !== 'string')
    return;

    const [field, order] = orderBy.split(" ");
    const orderAsNumber = order?.toLowerCase() === "desc" ? -1 : 1;

    if (this.multiSortMeta?.length !== 1 || this.multiSortMeta[0].field !== field || this.multiSortMeta[0].order !== orderAsNumber)
      this.multiSortMeta = [{field: field, order: orderAsNumber}]
  }

  public multiSortMeta?: SortMeta[] | null;

  /**
   * The style class for the table.
   * The default is "".
   */
  @Input()
  public styleClass: string = "";

  /**
   * The inline style for the table.
   */
  @Input()
  public tableStyle?: Record<string,string>;

  /**
   * Indicates whether the table is scrollable.
   * The default is `true`.
   */
  @Input()
  public scrollable: boolean = true;

  /**
   * The height of the scrollable table.
   * The default is "flex".
   */
  @Input()
  public scrollHeight: string = "flex";

  /**
   * Emitted when the filter or sort parameters have changed.
   * Subscribe to this event to load the items from the backend.
   * This is one of the core features of the table.
   */
  @Output()
  public onFilterOrSortChanged = new EventEmitter<ODataParameters>();

  /**
   * Emitted when a row has been selected.
   */
  @Output()
  public onRowSelect = new EventEmitter<TListItem>();

  /**
   * Emitted when a row has been unselected.
   */
  @Output()
  public onRowUnselect = new EventEmitter<TListItem>();

  /**
   * The mode how rows can be selected.
   * The default is `null` which means rows cannot be selected.
   */
  @Input()
  public selectionMode: "single" | "multiple" | null = null;

  /**
   * Indicates whether the table rows are highlighted when the mouse hovers over them.
   */
  @Input()
  public rowHover: boolean = false;

  /**
   * The currently selected rows.
   */
  @Input()
  public get selection(): TListItem[] {
    return _.isArray(this._selection) ? this._selection : [this._selection];
  }
  public set selection(value: TListItem[]) {
    this._selection = value;
  }
  public _selection: TListItem | TListItem[] = [];

  /**
   * Emitted when the selection has changed.
   * @param event The new selection.
   */
  @Output()
  selectionChange: EventEmitter<TListItem[]> = new EventEmitter();

  @ViewChild("contextMenu")
  contextMenu?: ContextMenu;

  private _contextMenuItems: MenuItem[] = [];
  public get contextMenuItems() {
    return this._contextMenuItems;
  }

  /**
   * Indicates whether the table is currently loading.
   * Set this to true while loading new items from the backend when reacting to the `onFilterOrSortChanged` event.
   */
  @Input()
  public isLoading = false;

  private _columns: Property<SimpleValue, string, string>[] = [];
  public get columns(): Property<SimpleValue, string, string>[] {
    return this._columns;
  }

  private _rowsBeforeCurrentPage: number = 0;
  public get rowsBeforeCurrentPage(): number {
    return this._rowsBeforeCurrentPage;
  }

  private static readonly _dateFormat = new Date(3333, 10, 22) // months start at 0 in JS
    .toLocaleDateString()
    .replace("22", "dd")
    .replace("11", "MM")
    .replace("3333", "yy")
    .replace("33", "y");

  public get dateFormat(): string {
    return RestWorldTableComponent._dateFormat;
  }

  private static readonly _timeFormat = new Date(1, 1, 1, 22, 33, 44)
    .toLocaleTimeString()
    .replace("22", "hh")
    .replace("33", "mm")
    .replace("44", "ss");

  public get timeFormat() {
    return RestWorldTableComponent._timeFormat;
  }

  public get PropertyType(): typeof PropertyType {
    return PropertyType;
  }

  public load(event: TableLazyLoadEvent) {
    this.multiSortMeta = event.multiSortMeta;
    this._rowsBeforeCurrentPage = event.first ?? 0;
    const parameters = ODataService.createParametersFromTableLoadEvent(event, this.searchTemplate);
    this.onFilterOrSortChanged.emit(parameters);
  }

  public openContextMenu(event: MouseEvent, row: TListItem): void {
    if (!this.showRowMenuOnRightClick || this.contextMenu === undefined)
      return;

    this._contextMenuItems = this.rowMenu(row, true);
    this.contextMenu.show(event);
    event.stopPropagation();
  }

  public onRowSelectInternal(event: TableRowSelectEvent): void {
    this.onRowSelect.emit(event.data);
  }

  public onRowUnselectInternal(event: TableRowUnSelectEvent): void {
    this.onRowUnselect.emit(event.data);
  }

  public onSelectionChangeInternal(event: TListItem | TListItem[]): void {
    this.selectionChange.emit(_.isArray(event) ? event : [event]);
  }

  public toColumnFilterType(propertyType: PropertyType) : ColumnFilterType{
    switch(propertyType) {
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
        return ColumnFilterType.text;
    }
  }

  public toMatchMode(propertyType: PropertyType): string | undefined {
    // There is a bug in PrimeNG returning CONTAINS for boolean columns
    // Once the bug has been fixed, we can remove this workaround
    // https://github.com/primefaces/primeng/issues/14210
    if (propertyType === PropertyType.Bool)
      return FilterMatchMode.EQUALS;

      return undefined;
  }

  public showInputField(column: Property): boolean {
    if (!this.isEditable)
      return false;

    const editProperty = this.editProperties[column.name];

    return editProperty !== undefined && editProperty.type !== PropertyType.Hidden && !editProperty.readOnly;
  }

  private updateFormArray(): void {
    if(!this.isEditable || !this.rows || !this.formArray || !this.editTemplate)
      return;

    this.formArray.clear();

    const newControls = this.rows
      .map(r => {
        const formGroup = this._formService.createFormGroupFromTemplate(this.editTemplate!) as FormGroup;
        formGroup.patchValue(r);
        return formGroup;
      });

    for (const control of newControls)
      this.formArray.push(control);
  }

  private updateEditProperties(): void {
    if (!this.editTemplate)
      return;

    this._editProperties = Object.fromEntries(this.editTemplate.properties.map(p => [ p.name, p ]));
  }
}
