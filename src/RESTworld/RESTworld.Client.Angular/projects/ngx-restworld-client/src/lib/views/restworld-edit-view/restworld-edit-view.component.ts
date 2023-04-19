import { Component, Input } from '@angular/core';
import { PropertyType, Resource, Template, Templates } from '@wertzui/ngx-hal-client';
import { RestWorldClient } from '../../services/restworld-client';
import { RestWorldClientCollection } from '../../services/restworld-client-collection';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ProblemDetails } from '../../models/problem-details';
import { ContentChild } from '@angular/core';
import { TemplateRef } from '@angular/core';
import { ValdemortConfig } from 'ngx-valdemort';

@Component({
  selector: 'rw-edit',
  templateUrl: './restworld-edit-view.component.html',
  styleUrls: ['./restworld-edit-view.component.css']
})
export class RESTworldEditViewComponent {
  @ContentChild('extraTabs', { static: false })
  extraTabsRef?: TemplateRef<unknown>;
  public idNavigationForm = new FormGroup < {
    id: FormControl<number | null>
  }>({
    id: new FormControl(null, Validators.compose([Validators.min(1), Validators.max(Number.MAX_SAFE_INTEGER)]))
  });

  public isLoading = true;
  private _templates: Template[] = [];
  public get templates(): Template[] {
    return this._templates;
  }
  private _resource?: Resource;

  constructor(
    private _clients: RestWorldClientCollection,
    private _messageService: MessageService,
    valdemortConfig: ValdemortConfig) {
    valdemortConfig.errorClasses = 'p-error text-sm';
  }

  public get PropertyType() {
    return PropertyType;
  }

  private _apiName!: string;
  public get apiName(): string {
    return this._apiName;
  }
  @Input()
  public set apiName(value: string) {
    this._apiName = value;
    this.load();
  }

  @Input()
  public rel!: string;

  public get resource() {
    return this._resource;
  }

  private _uri!: string;
  public get uri(): string {
    return this._uri;
  }
  @Input()
  public set uri(value: string) {
    this._uri = value;
    this.load();
  }

  public async load(): Promise<void> {
    if (!this.apiName || !this.uri)
      return;

    this.isLoading = true;

    const response = await this.getClient().getSingleByUri(this.uri);
    if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resource from the API.', data: response, sticky: true });
    }
    else {
      this._resource = response.body;
      const templates = await this.getAllTemplates(this._resource);
      this._templates = Object.values(templates);
    }

    this.isLoading = false;
  }

  private async getAllTemplates(resource: Resource): Promise<Templates> {
    try {
      return this.getClient().getAllTemplates(resource);
    }
    catch (e: unknown) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the templates from the API. ' + e, data: e });
      return {};
    }
  }

  private getClient(): RestWorldClient {
    if (!this.apiName)
      throw new Error('Cannot get a client, because the apiName is not set.');

    return this._clients.getClient(this.apiName);
  }
}
