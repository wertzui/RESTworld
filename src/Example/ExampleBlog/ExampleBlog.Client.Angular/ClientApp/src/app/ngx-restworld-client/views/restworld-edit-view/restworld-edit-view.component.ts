import { Component, Input, OnInit } from '@angular/core';
import { Resource } from '@wertzui/ngx-hal-client';
import { RESTworldClient } from '../../services/restworld-client';
import * as _ from 'lodash'
import { RESTworldClientCollection } from '../../services/restworld-client-collection';
import { FormControl, FormGroup } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { ProblemDetails } from '../../models/problem-details';
import { HttpStatusCode } from '@angular/common/http';
import { ContentChild } from '@angular/core';
import { TemplateRef } from '@angular/core';

interface PropertyDescription {
  field: string,
  value: any,
  label: string,
  type: 'text' | 'numeric' | 'date' | 'boolean' | 'object' | 'array',
  isReadOnly: boolean,
  children?: PropertyDescription[]
}

@Component({
  selector: 'restworld-edit-view',
  templateUrl: './restworld-edit-view.component.html',
  styleUrls: ['./restworld-edit-view.component.css']
})
export class RESTworldEditViewComponent {
  public get properties() {
    return this._properties;
  }
  private _properties: PropertyDescription[] = [];

  public get formGroup() {
    return this._formGroup;
  }
  private _formGroup: FormGroup = new FormGroup({});

  @Input()
  public set apiName(value: string | undefined) {
    this._apiName = value;
    this.load();
  }
  public get apiName(): string | undefined {
    return this._apiName;
  }
  private _apiName?: string;
  //@Input()
  //public set rel(value: string | undefined) {
  //  this._rel = value;
  //}
  //public get rel(): string | undefined {
  //  return this._rel;
  //}
  //private _rel?: string;
  @Input()
  public set uri(value: string | undefined) {
    this._uri = value;
    this.load();
  }
  public get uri(): string | undefined {
    return this._uri;
  }
  private _uri?: string;

  public get resource() {
    return this._resource;
  }
  private _resource?: Resource;
  public isLoading = false;
  public get canSave() {
    const length = this.resource?._links["save"]?.length;
    return length !== undefined && length > 0;
  }
  public get canDelete() {
    const length = this.resource?._links["delete"]?.length;
    return length !== undefined && length > 0;
  }

  public get dateFormat(): string {
    return new Date(3333, 10, 22)
      .toLocaleDateString()
      .replace("22", "dd")
      .replace("11", "mm")
      .replace("3333", "yy")
      .replace("33", "y");
  }

  @ContentChild('visualTab', { static: false })
  visualTabRef?: TemplateRef<any>;

  @ContentChild('form', { static: false })
  formRef?: TemplateRef<any>;

  @ContentChild('formRow', { static: false })
  formRowRef?: TemplateRef<any>;

  @ContentChild('formLabel', { static: false })
  formLabelRef?: TemplateRef<any>;

  @ContentChild('formInput', { static: false })
  formInputRef?: TemplateRef<any>;

  @ContentChild('visualTabAdditional', { static: false })
  visualTabAdditionalRef?: TemplateRef<any>;

  @ContentChild('rawTab', { static: false })
  rawTabRef?: TemplateRef<any>;

  @ContentChild('buttonsRef', { static: false })
  buttonsRef?: TemplateRef<any>;

  constructor(
    private _clients: RESTworldClientCollection,
    private _confirmationService: ConfirmationService,
    private _messageService: MessageService,
    private _location: Location,
    private _router: Router) {

  }

  private getClient(): RESTworldClient {
    if (!this.apiName)
      throw new Error('Cannot get a client, because the apiName is not set.');

    return this._clients.getClient(this.apiName);
  }

  public async save(): Promise<void> {
    if (!this.apiName || !this.uri || !this.resource)
      return;

    Object.assign(this.resource, this.formGroup.value);
    const selfHrefBeforeSave = this.resource._links.self[0].href;

    this.isLoading = true;
    const response = await this.getClient().save(this.resource);
    this.isLoading = false;

    if (!response.ok || ProblemDetails.isProblemDetails(response.body)) {
      const message = response.status === HttpStatusCode.Conflict ? 'Someone else modified the resource. Try reloading it and apply your changes again.' : 'Error while saving the resource.';
      this._messageService.add({ severity: 'error', summary: 'Error', detail: message, data: response });
    }
    else {
      const selfHrefAfterSave = this.resource._links.self[0].href;

      setTimeout(() =>
        this._messageService.add({ severity: 'success', summary: 'Saved', detail: 'The resource has been saved.' }),
        100);

      if (selfHrefBeforeSave !== selfHrefAfterSave) {
        this._router.navigate(['/edit', this.apiName, selfHrefAfterSave]);
      }
    }

  }

  public showDeleteConfirmatioModal() {
    this._confirmationService.confirm({
      message: 'Do you really want to delete this resource?',
      header: 'Confirm delete',
      icon: 'far fa-trash-alt',
      accept: () => this.delete()
    });
  }

  public async delete(): Promise<void> {
    if (!this.apiName || !this.uri || !this.resource)
      return;

    Object.assign(this.resource, this.formGroup.value);

    await this.getClient().delete(this.resource);
    setTimeout(() =>
      this._messageService.add({ severity: 'success', summary: 'Deleted', detail: 'The resource has been deleted.' }),
      100);

    this._location.back();
  }

  public async load(): Promise<void> {
    if (!this.apiName || !this.uri)
      return;

    this.isLoading = true;

    const response = await this.getClient().getSingle(this.uri);
    if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resource from the API.', data: response })
    }
    else {
      this._resource = response.body;
      this._properties = RESTworldEditViewComponent.createProperyInfos(this._resource);
      this._formGroup = RESTworldEditViewComponent.createFormGroup(this._properties);
    }

    this.isLoading = false;
  }

  static createFormGroup(properties: PropertyDescription[]): FormGroup {
    const controls = Object.fromEntries(properties.map(p => [
      p.field,
      p.type === 'object' && p.children ? RESTworldEditViewComponent.createFormGroup(p.children) : new FormControl(p.value)
    ]));
    const formGroup = new FormGroup(controls);
    return formGroup;
  }

  private static createProperyInfos(resource: any): PropertyDescription[] {
    if (!resource)
      return [];

    // Get all distinct properties from all rows
    // We look at all rows to eliminate possible undefined values
    const properties = Object.entries(resource)
      .filter(p =>
        p[0] !== '_links' &&
        p[0] !== '_embedded' &&
        p[0] !== 'id' &&
        p[0] !== 'timestamp');

    // Check if the rows are entities with change tracking
    const withoutChangeTrackingProperties = properties.filter(p =>
      p[0] !== 'createdAt' &&
      p[0] !== 'createdBy' &&
      p[0] !== 'lastChangedAt' &&
      p[0] !== 'lastChangedBy');
    const hasChangeTrackingProperties = withoutChangeTrackingProperties.length < properties.length;

    // First the id, then all other properties
    const sortedProperties: [string, any][] = [];
    if (Object.hasOwnProperty('id'))
      sortedProperties.push(['id', resource['id']]);

    sortedProperties.push(...withoutChangeTrackingProperties);

    // And change tracking properties at the end
    if (hasChangeTrackingProperties) {
      sortedProperties.push(['createdAt', resource['createdAt']]);
      sortedProperties.push(['createdBy', resource['createdBy']]);
      sortedProperties.push(['lastChangedAt', resource['lastChangedAt']]);
      sortedProperties.push(['lastChangedBy', resource['lastChangedBy']]);
    }

    const propertyDescriptions: PropertyDescription[] = sortedProperties
      .map(p => ({
        field: p[0],
        value: p[1],
        label: RESTworldEditViewComponent.toTitleCase(p[0]),
        type: RESTworldEditViewComponent.getColumnType(p[0], p[1]),
        isReadOnly: RESTworldEditViewComponent.getIsReadOnly(p[0])
      }));

    for (var description of propertyDescriptions) {
      if (description.type === 'object') {
        const children = RESTworldEditViewComponent.createProperyInfos(description.value);
        children.forEach(d => description.field + '.' + d.field);
        description.children = children;
      }
    }

    return propertyDescriptions;
  }

  private static getIsReadOnly(field: string) {
    return field === 'id' || field === 'createdAt' || field === 'createdBy' || field === 'lastChangedAt' || field === 'lastChangedBy';
  }

  private static getColumnType(field: string, value: any) {
    if (value === null || value === undefined)
      return 'text';

    if (_.isNumber(value))
      return 'numeric';

    if (_.isDate(value))
      return 'date'

    if (_.isString(value)) {
      return 'text';
    }

    if (_.isBoolean(value))
      return 'boolean';

    if (_.isObject(value))
      return 'object'

    if (_.isArray(value))
      return 'array'

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
}
