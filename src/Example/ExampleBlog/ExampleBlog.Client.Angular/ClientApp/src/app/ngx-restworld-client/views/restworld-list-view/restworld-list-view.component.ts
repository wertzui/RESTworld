import { Component, Input } from '@angular/core';
import { PagedListResource, Property, PropertyType, Resource, ResourceDto, Template } from '@wertzui/ngx-hal-client';
import * as _ from 'lodash';
import { ConfirmationService, FilterMatchMode, FilterMetadata, LazyLoadEvent, MenuItem, MessageService } from 'primeng/api';
import { RESTworldClient } from '../../services/restworld-client';
import { RESTworldClientCollection } from '../../services/restworld-client-collection';
import { ProblemDetails } from '../../models/problem-details';
import { AvatarGenerator } from '../../services/avatar-generator';
import { Router } from '@angular/router';
import { ODataService } from '../../services/o-data.service'

export enum ColumnFilterType {
  text = 'text',
  numeric = 'numeric',
  boolean = 'boolean',
  date = 'date'
}

@Component({
  selector: 'rw-list',
  templateUrl: './restworld-list-view.component.html',
  styleUrls: ['./restworld-list-view.component.css']
})
export class RESTworldListViewComponent<TListDto extends ResourceDto> {
  private static _dateFormat = new Date(3333, 10, 22)
    .toLocaleDateString()
    .replace("22", "dd")
    .replace("11", "MM")
    .replace("3333", "y")
    .replace("33", "yy");

  @Input()
  public createButtonMenu?: MenuItem[];
  public isLoading = false;
  public load = _.debounce(this.load2, 100);
  public resource?: PagedListResource<TListDto>;
  @Input()
  public rowsPerPage: number[];

  private _apiName?: string;
  private _editLink = '/edit';
  private _lastEvent: LazyLoadEvent;
  private _rel?: string;
  private _sortField = '';
  private _sortOrder = 1;
  private _template?: Template;
  private _totalRecords = 0;

  constructor(
    private _clients: RESTworldClientCollection,
    private _confirmationService: ConfirmationService,
    private _messageService: MessageService,
    public avatarGenerator: AvatarGenerator,
    private readonly _router: Router) {
    this.rowsPerPage = [10, 25, 50];

    this._lastEvent = {
      rows: this.rowsPerPage[0]
    };
  }

  public get PropertyType(): typeof PropertyType {
    return PropertyType;
  }

  public get apiName(): string | undefined {
    return this._apiName;
  }

  @Input()
  public set apiName(value: string | undefined) {
    this._apiName = value;
    this.load(this._lastEvent);
  }

  public get columns(): Property[] {
    return this.template?.properties.filter(p => p.type !== PropertyType.Hidden) ?? [];
  }

  public get dateFormat(): string {
    return RESTworldListViewComponent._dateFormat;
  }

  public get editLink() {
    return this._editLink;
  }

  @Input()
  public set editLink(value: string) {
    if (value)
      this._editLink = value;
  }

  public get newHref(): string | undefined {
    return this.resource?.findLink('new')?.href;
  }

  public get rel(): string | undefined {
    return this._rel;
  }

  @Input()
  public set rel(value: string | undefined) {
    this._rel = value;
    this.load(this._lastEvent);
  }

  public get rows(): number {
    return this._lastEvent?.rows || 0;
  }

  public get sortField(): string {
    return this._sortField;
  }

  @Input()
  public set sortField(value: string | undefined) {
    this._sortField = value ?? '';
    this._lastEvent.sortField = value;
    this.load(this._lastEvent);
  }

  public get sortOrder(): number {
    return this._sortOrder;
  }

  @Input()
  public set sortOrder(value: number | undefined) {
    this._sortOrder = value ?? 1;
    this._lastEvent.sortOrder = value;
    this.load(this._lastEvent);
  }

  public get template(): Template | undefined {
    return this._template;
  }

  public get totalRecords(): number {
    return this._totalRecords;
  }

  public get value(): Resource[] {
    return this.resource?._embedded?.items || [];
  }

  private set totalRecords(value: number | undefined) {
    this._totalRecords = value || 0;
  }

  public createNew(): Promise<boolean> {
    return this._router.navigate([this.editLink, this.apiName, this.rel, this.newHref]);
  }

  public async delete(resource: Resource): Promise<void> {
    if (!this.apiName || !this.rel)
      return;

    await this.getClient().delete(resource);

    this._messageService.add({ severity: 'success', summary: 'Deleted', detail: 'The resource has been deleted.' });

    this.load(this._lastEvent);
  }

  public async load2(event: LazyLoadEvent): Promise<void> {
    if (!this.apiName || !this.rel)
      return;

    this.isLoading = true;
    this._lastEvent = event;

    const parameters = ODataService.createParametersFromTableLoadEvent(event, this.template);
    const response = await this.getClient().getList<TListDto>(this.rel, parameters);
    if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resources from the API.', data: response });
    }
    else if (response.body) {
      try {
        const templates = await this.getClient().getAllTemplates(response.body);
        this._template = templates["Search"];
        if (this._template === undefined)
          throw new Error("No search template found in the API response.");
      }
      catch (e: unknown) {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resources from the API. ' + e, data: e });
      }

      this.resource = response.body;
      this.totalRecords = this.resource.totalPages && parameters.$top ? this.resource.totalPages * parameters.$top : undefined;
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

  private getClient(): RESTworldClient {
    if (!this.apiName)
      throw new Error('Cannot get a client, because the apiName is not set.');

    return this._clients.getClient(this.apiName);
  }
}
