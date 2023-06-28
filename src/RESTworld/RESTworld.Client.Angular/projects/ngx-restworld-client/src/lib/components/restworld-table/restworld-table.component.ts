import { Component, EventEmitter, Input, OnInit, Output, ViewChild } from '@angular/core';
import { NumberTemplate, Property, PropertyType, Template } from '@wertzui/ngx-hal-client';
import * as _ from 'lodash';
import { LazyLoadEvent, MenuItem } from 'primeng/api';
import { ODataParameters } from '../../models/o-data';
import { ODataService } from '../../services/o-data.service';
import { ContextMenu } from 'primeng/contextmenu';
import { AbstractControl, ControlContainer, FormArray, FormGroup, FormGroupDirective, FormRecord, UntypedFormArray, UntypedFormGroup } from '@angular/forms';
import { FormService } from '../../services/form.service';

enum ColumnFilterType {
  text = 'text',
  numeric = 'numeric',
  boolean = 'boolean',
  date = 'date'
}

@Component({
  selector: 'rw-table',
  templateUrl: './restworld-table.component.html',
  styleUrls: ['./restworld-table.component.css'],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }]
})
export class RestWorldTableComponent<TListDto extends Record<string, unknown>> {
  @Input()
  public apiName?: string;

  public get formArray(): FormArray<UntypedFormGroup> | undefined {
    return this._controlContainer.control as FormArray<UntypedFormGroup> | undefined;
  }

  private _searchTemplate?: Template;
  @Input()
  public set searchTemplate(value: Template | undefined){
    this._searchTemplate = value;
    this._columns = value?.properties.filter(p => p.type !== PropertyType.Hidden) ?? [];
    this.setSortFieldAndSortOrder(value)
  }

  public get searchTemplate() {
    return this._searchTemplate;
  }

  private _editTemplate?: Template;
  @Input()
  public set editTemplate(value: Template | undefined){
    this._editTemplate = value;
    this.updateEditProperties();
    this.updateFormArray();
  }

  public get editTemplate() {
    return this._editTemplate;
  }

  private _editProperties: Record<string, Property> = {};
  public get editProperties() {
    return this._editProperties;
  }

  public get isEditable() {
    return this.editTemplate !== undefined && this.formArray !== undefined && this.apiName;
  }

  private _rows: TListDto[] = [];
  public get rows() {
    return this._rows;
  }
  @Input()
  public set rows(value: TListDto[]) {
    this._rows = value;
    if(this.showRowMenuAsColumn)
      this._rowMenus = value.map(r => this.rowMenu(r, false));
    this.updateFormArray();
  }

  @Input()
  public rowsPerPageOptions = [10, 25, 50];

  @Input()
  public headerMenu: MenuItem[] = [];

  @Input()
  public rowMenu: (row: TListDto, openedByRightClick: boolean) => MenuItem[] = () => [];

  private _rowMenus: MenuItem[][] = [];
  public get rowMenus() {
    return this._rowMenus;
  }

  public get showMenuColumn() {
    return this.headerMenu.length > 0 || (this.showRowMenuAsColumn && this.rowMenus.some(m => m.length > 0));
  }

  @Input()
  public showRowMenuAsColumn = true;

  @Input()
  public showRowMenuOnRightClick = true;

  @Input()
  public totalRecords = 0;

  public constructor(
    private readonly _controlContainer: ControlContainer,
    private readonly _formService: FormService) {

  }

  private setSortFieldAndSortOrder(template: Template | undefined) {
    const orderBy = template?.properties?.find(p => p?.name === "$orderby")?.value;

    if(orderBy === null || orderBy === undefined || typeof orderBy !== 'string')
    return;

    const [field, order] = orderBy.split(" ");
    const orderAsNumber = order?.toLowerCase() === "desc" ? -1 : 1;

    if (this.sortField !== field)
      this.sortField = field;
    if (this.sortOrder !== orderAsNumber)
      this.sortOrder = orderAsNumber;
  }

  public sortField?: string;
  public sortOrder?: number;

  @Input()
  public styleClass: string = "";

  @Input()
  public tableStyle?: string;

  @Input()
  public scrollable: boolean = true;

  @Input()
  public scrollHeight: string = "flex";

  @Output()
  public onFilterOrSortChanged = new EventEmitter<ODataParameters>();

  @ViewChild("contextMenu")
  contextMenu?: ContextMenu;

  private _contextMenuItems: MenuItem[] = [];
  public get contextMenuItems() {
    return this._contextMenuItems;
  }

  @Input()
  public isLoading = false;

  private _columns: Property[] = [];
  public get columns(): Property[] {
    return this._columns;
  }

  // private _editColumns: Property[] = [];
  // public get editColumns(): Property[] {
  //   return this._editColumns;
  // }

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

  public load(event: LazyLoadEvent) {
    this.sortField = event.sortField;
    this.sortOrder = event.sortOrder;
    this._rowsBeforeCurrentPage = event.first ?? 0;
    const parameters = ODataService.createParametersFromTableLoadEvent(event, this.searchTemplate);
    this.onFilterOrSortChanged.emit(parameters);
  }

  public openContextMenu(event: MouseEvent, row: TListDto): void {
    if (!this.showRowMenuOnRightClick || this.contextMenu === undefined)
      return;

    this._contextMenuItems = this.rowMenu(row, true);
    this.contextMenu.show(event);
    event.stopPropagation();
  }

  public toColumnFilterType(propertyType: PropertyType) : ColumnFilterType{
    switch(propertyType) {
      case PropertyType.Number:
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
        const formGroup = this._formService.createFormGroupFromTemplate(this.editTemplate!);
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
