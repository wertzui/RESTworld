import { Component, EventEmitter, Input, Output, ViewChild } from '@angular/core';
import { Property, PropertyType, Template } from '@wertzui/ngx-hal-client';
import * as _ from 'lodash';
import { LazyLoadEvent, MenuItem } from 'primeng/api';
import { ODataParameters } from '../../models/o-data';
import { ODataService } from '../../services/o-data.service';
import { ContextMenu } from 'primeng/contextmenu';

enum ColumnFilterType {
  text = 'text',
  numeric = 'numeric',
  boolean = 'boolean',
  date = 'date'
}

@Component({
  selector: 'rw-table',
  templateUrl: './restworld-table.component.html',
  styleUrls: ['./restworld-table.component.css']
})
export class RestWorldTableComponent<TListDto> {
  @Input()
  public api?: string;

  private _template?: Template;
  @Input()
  public set template(value: Template | undefined){
    this._template = value;
    this._columns = value?.properties.filter(p => p.type !== PropertyType.Hidden) ?? [];
    this.setSortFieldAndSortOrder(value)
  }

  public get template() {
    return this._template;
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
    return this.headerMenu.length > 0 || this.showRowMenuAsColumn;
  }

  @Input()
  public showRowMenuAsColumn = true;

  @Input()
  public showRowMenuOnRightClick = true;

  @Input()
  public totalRecords = 0;

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

  private static readonly _dateFormat = new Date(3333, 10, 22)
    .toLocaleDateString()
    .replace("22", "dd")
    .replace("11", "MM")
    .replace("3333", "y")
    .replace("33", "yy");

  public get dateFormat() {
    return RestWorldTableComponent._dateFormat;
  }

  public get PropertyType(): typeof PropertyType {
    return PropertyType;
  }

  public load(event: LazyLoadEvent) {
    this.sortField = event.sortField;
    this.sortOrder = event.sortOrder;
    const parameters = ODataService.createParametersFromTableLoadEvent(event, this.template);
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
}
