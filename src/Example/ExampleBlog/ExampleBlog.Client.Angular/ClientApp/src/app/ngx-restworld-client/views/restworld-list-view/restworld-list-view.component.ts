import { Component, Input, OnInit } from '@angular/core';
import { PagedListResource, Resource } from '@wertzui/ngx-hal-client';
import * as _ from 'lodash'
import { ConfirmationService, FilterMatchMode, FilterMetadata, LazyLoadEvent, MessageService } from 'primeng/api'
import { RESTworldClient } from '../../services/restworld-client';
import { RESTworldClientCollection } from '../../services/restworld-client-collection';
import { ProblemDetails } from '../../models/problem-details';
import { AvatarGenerator } from '../../services/avatar-generator';

@Component({
  selector: 'restworld-list-view',
  templateUrl: './restworld-list-view.component.html',
  styleUrls: ['./restworld-list-view.component.css']
})
export class RESTworldListViewComponent {
  public get columns(): any[] {
    return this._columns;
  }
  private _columns: any[] = [];
  @Input()
  public set editLink(value: string) {
    if (value)
      this._editLink = value;
  }
  public get editLink() {
    return this._editLink;
  }
  private _editLink: string = '/edit';
  @Input()
  public set apiName(value: string | undefined) {
    this._apiName = value;
    if (this.apiName && this.rel && this._lastEvent)
      this.load(this._lastEvent);
  }
  public get apiName(): string | undefined {
    return this._apiName;
  }
  private _apiName?: string;
  @Input()
  public set rel(value: string | undefined) {
    this._rel = value;
    if (this.apiName && this.rel && this._lastEvent)
      this.load(this._lastEvent);
  }
  public get rel(): string | undefined {
    return this._rel;
  }
  private _rel?: string;
  @Input()
  public rowsPerPage: number[];
  public resource?: PagedListResource;
  public isLoading = false;
  private _totalRecords: number = 0;
  private _lastEvent: LazyLoadEvent;
  public get value(): Resource[] {
    return this.resource?._embedded?.items || [];
  }
  public get rows(): number {
    return this._lastEvent?.rows || 0;
  }
  public get totalRecords(): number {
    return this._totalRecords;
  }
  private set totalRecords(value: number | undefined) {
    this._totalRecords = value || 0;
  }
  public get sortOrder() {
    return this._lastEvent.sortOrder || 0;
  }

  public get newHref(): string | undefined {
    return this.resource?.findLink('new')?.href;
  }

  private static _dateFormat = new Date(3333, 10, 22)
    .toLocaleDateString()
    .replace("22", "dd")
    .replace("11", "MM")
    .replace("3333", "y")
    .replace("33", "yy");

  public get dateFormat(): string {
    return RESTworldListViewComponent._dateFormat;
  }

  constructor(
    private _clients: RESTworldClientCollection,
    private _confirmationService: ConfirmationService,
    private _messageService: MessageService,
    public avatarGenerator: AvatarGenerator) {
    this.rowsPerPage = [10, 25, 50];

    this._lastEvent = {
      rows: this.rowsPerPage[0]
    }
  }

  private getClient(): RESTworldClient {
    if (!this.apiName)
      throw new Error('Cannot get a client, because the apiName is not set.');

    return this._clients.getClient(this.apiName);
  }

  public async load(event: LazyLoadEvent): Promise<void> {
    if (!this.apiName || !this.rel)
      return;

    this.isLoading = true;
    this._lastEvent = event;

    const parameters = RESTworldListViewComponent.createParametersFromEvent(event);
    const response = await this.getClient().getList(this.rel, parameters);
    if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resources from the API.',  data: response})
    }
    else {
      this.resource = response.body;
      this.totalRecords = this.resource!.totalPages && parameters.$top ? this.resource!.totalPages * parameters.$top : undefined;
      this._columns = this.createColumns();
    }

    this.isLoading = false;
  }


  public showDeleteConfirmatioModal(resource: Resource) {
    this._confirmationService.confirm({
      message: 'Do you really want to delete this resource?',
      header: 'Confirm delete',
      icon: 'far fa-trash-alt',
      accept: () => this.delete(resource)
    });
  }

  public async delete(resource: Resource): Promise<void> {
    if (!this.apiName || !this.rel)
      return;

    await this.getClient().delete(resource);

    this._messageService.add({ severity: 'success', summary: 'Deleted', detail: 'The resource has been deleted.' });

    this.load(this._lastEvent)
  }

  private createColumns(): any[] {
    if (this.value.length === 0)
      return [];

    // Get all distinct properties from all rows
    // We look at all rows to eliminate possible undefined values
    const rowsWithProperties = this.value
      .map(resource =>
        Object.entries(resource)
          .filter(p =>
            p[0] !== '_links' &&
            p[0] !== '_embedded' &&
            p[0] !== 'id' &&
            p[0] !== 'timestamp'));

    let distinctProperties = rowsWithProperties[0];
    for (let propertiesOfRow of rowsWithProperties) {
      for (var property of propertiesOfRow) {
        const propertyName = property[0];
        const propertyValue = property[1];
        const alreadyFoundPropertyWithSameName = distinctProperties.find(p => p[0] === propertyName);
        if (!alreadyFoundPropertyWithSameName) // Add new property
          distinctProperties.push(property);
        else if (!alreadyFoundPropertyWithSameName[1] && propertyValue) // Use defined value instead of existing undefined value
          alreadyFoundPropertyWithSameName[1] = propertyValue
      }
    }

    // Check if the rows are entities with change tracking
    const withoutChangeTrackingProperties = distinctProperties.filter(p =>
      p[0] !== 'createdAt' &&
      p[0] !== 'createdBy' &&
      p[0] !== 'lastChangedAt' &&
      p[0] !== 'lastChangedBy');
    const hasChangeTrackingProperties = withoutChangeTrackingProperties.length < distinctProperties.length;

    // First the id, then all other properties
    const sortedProperties: [string, any][] = [
      ['id', 0],
      ...withoutChangeTrackingProperties
    ];

    // And change tracking properties at the end
    if (hasChangeTrackingProperties) {
      sortedProperties.push(['createdAt', new Date()]);
      sortedProperties.push(['createdBy', '']);
      sortedProperties.push(['lastChangedAt', new Date()]);
      sortedProperties.push(['lastChangedBy', '']);
    }

    const columns = sortedProperties
      .map(p => ({
        header: RESTworldListViewComponent.toTitleCase(p[0]),
        field: p[0],
        type: RESTworldListViewComponent.getColumnType(p[1]),
      }));

    return columns;
  }

  private static getColumnType(value: any) {
    if (value === null || value === undefined)
      return 'text';

    if (_.isNumber(value))
      return 'numeric';

    if (_.isDate(value))
      return 'date'

    if (_.isString(value))
      return 'text';

    if (_.isBoolean(value))
      return 'boolean';

    return 'text'
  }

  private static toTitleCase(anyCase: string) {
    return anyCase
      .replace(/(_)+/g, ' ')                              // underscore to whitespace
      .replace(/([a-z])([A-Z][a-z])/g, "$1 $2")           // insert space before each new word if there is none
      .replace(/([A-Z][a-z])([A-Z])/g, "$1 $2")           // insert space after each word if there is none
      .replace(/([a-z])([A-Z]+[a-z])/g, "$1 $2")          // insert space after single letter word if there is none
      .replace(/([A-Z]+)([A-Z][a-z][a-z])/g, "$1 $2")     // insert space before single letter word if there is none
      .replace(/([a-z]+)([A-Z0-9]+)/g, "$1 $2")           // insert space after numbers
      .replace(/^./, (match) => match.toUpperCase());     // change first letter to be upper case
  }

  private static createParametersFromEvent(event: LazyLoadEvent) {
    const oDataParameters = {
      $filter: RESTworldListViewComponent.createFilterFromEvent(event),
      $orderby: RESTworldListViewComponent.createOrderByFromEvent(event),
      $top: RESTworldListViewComponent.createTopFromEvent(event),
      $skip: RESTworldListViewComponent.createSkipFromEvent(event)
    };

    return oDataParameters;
  }

  static createSkipFromEvent(event: LazyLoadEvent): number | undefined {
    return event.first;
  }

  static createTopFromEvent(event: LazyLoadEvent): number | undefined {
    return event.rows;
  }

  static createOrderByFromEvent(event: LazyLoadEvent): string | undefined {
    if (event.sortField) {
      const order = !event.sortOrder || event.sortOrder > 0 ? 'asc' : 'desc';
      return `${event.sortField} ${order}`;
    }

    return undefined;
  }
  static createFilterFromEvent(event: LazyLoadEvent): string | undefined {
    if (!event.filters)
      return undefined;

    const filter = Object.entries(event.filters)
      .map(([property, filter]) => RESTworldListViewComponent.createFilterForProperty(property, filter))
      .filter(filter => !!filter)
      .join(' and ');

    if (filter === '')
      return undefined;

    return `(${filter})`
  }

  static createFilterForProperty(property: string, filter: FilterMetadata): string | undefined {
    if (!filter.value)
      return undefined;

    const oDataOperator = RESTworldListViewComponent.createODataOperator(
      filter.matchMode,
    );
    const comparisonValue = RESTworldListViewComponent.createComparisonValue(filter.value);

    switch (oDataOperator) {
      case 'contains':
      case 'not contains':
      case 'startswith':
      case 'endswith':
        return `${oDataOperator}(${property}, ${comparisonValue})`;
      default:
        return `${property} ${oDataOperator} ${comparisonValue}`;
    }
  }

  static createODataOperator(matchMode?: string): string {
    switch (matchMode) {
      case FilterMatchMode.STARTS_WITH:
        return 'startswith';
      case FilterMatchMode.CONTAINS:
        return 'contains';
      case FilterMatchMode.NOT_CONTAINS:
        return 'not contains';
      case FilterMatchMode.ENDS_WITH:
        return 'endswith';
      case FilterMatchMode.EQUALS:
        return 'eq';
      case FilterMatchMode.NOT_EQUALS:
        return 'ne';
      case FilterMatchMode.IN:
        return 'in';
      case FilterMatchMode.LESS_THAN:
        return 'lt';
      case FilterMatchMode.LESS_THAN_OR_EQUAL_TO:
        return 'le';
      case FilterMatchMode.GREATER_THAN:
        return 'gt';
      case FilterMatchMode.GREATER_THAN_OR_EQUAL_TO:
        return 'ge';
      case FilterMatchMode.IS:
        return 'eq';
      case FilterMatchMode.IS_NOT:
        return 'ne';
      case FilterMatchMode.BEFORE:
        return 'lt';
      case FilterMatchMode.AFTER:
        return 'gt';
      default:
        throw Error(`Unknown matchMode ${matchMode}`);
    }
  }

  static createComparisonValue(value: unknown): string {
    if (value === null || value === undefined)
      return 'null';

    if (_.isNumber(value))
      return `${value}`;

    if (_.isDate(value))
      return `cast(${value.toISOString()}, Edm.DateTimeOffset)`;

    if (_.isString(value)) {
      const date = new Date(value);
      if (!isNaN(date.valueOf()))
        return `cast(${date.toISOString()}, Edm.DateTimeOffset)`;

      return `'${value}'`;
    }

    if (_.isArray(value))
      return `(${value
        .map((v) => this.createComparisonValue(v))
        .join(',')})`;

    throw new Error(`Unknown type of value: ${value}.`);
  }

}
