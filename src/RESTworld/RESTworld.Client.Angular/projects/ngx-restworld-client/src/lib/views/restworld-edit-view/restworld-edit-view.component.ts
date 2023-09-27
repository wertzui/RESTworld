import { Component, Input } from '@angular/core';
import { ProblemDetails, PropertyType, Resource, Template, Templates } from '@wertzui/ngx-hal-client';
import { RestWorldClient } from '../../services/restworld-client';
import { RestWorldClientCollection } from '../../services/restworld-client-collection';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { MessageService } from 'primeng/api';
import { ContentChild } from '@angular/core';
import { TemplateRef } from '@angular/core';
import { ValdemortConfig } from 'ngx-valdemort';

@Component({
  selector: 'rw-edit',
  templateUrl: './restworld-edit-view.component.html',
  styleUrls: ['./restworld-edit-view.component.css']
})
/**
 * Component for editing a resource in the RESTworld application.
 * This component loads the forms resource from the API and renders a tab view where every template of the forms resource is rendered as a tab.
 * Every template becomes one `RestworldFormComponent` in the tab view.
 * It also allows adding extra tabs to the view.
 * An ID navigation form is also rendered at the top right of the view.
 * @example
 * <rw-edit apiName="api" rel="rel" uri="/api/rel/1"></rw-edit>
 * @example
 * <rw-edit apiName="api" rel="rel" uri="/api/rel/1">
 * <ng-template #extraTabs>
 * <p-tabPanel header="Extra tab">
 * <p>Some extra content</p>
 * </p-tabPanel>
 * </ng-template>
 * </rw-edit>
 */
export class RESTworldEditViewComponent {
  @ContentChild('extraTabs', { static: false })
  /**
   * A reference to an optional template that can be used to add extra tabs to the view.
   */
  public extraTabsRef?: TemplateRef<unknown>;
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
  @Input({ required: true })
  /**
   * The name of the API to load the resource from.
   */
  public set apiName(value: string) {
    this._apiName = value;
    this.load();
  }

  @Input({ required: true })
  /**
   * The relation to load the resource from.
   */
  public rel!: string;

  public get resource() {
    return this._resource;
  }

  private _uri!: string;
  public get uri(): string {
    return this._uri;
  }
  @Input({ required: true })
  /**
   * Sets the URI for the REST API endpoint to be displayed in the view.
   * @param value The URI to set.
   */
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
      const templates = await this.getClient().getAllTemplates(resource);
      return templates;
    }
    catch (e: unknown) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the templates from the API. ' + e, data: e , sticky: true});
      return {};
    }
  }

  private getClient(): RestWorldClient {
    if (!this.apiName)
      throw new Error('Cannot get a client, because the apiName is not set.');

    return this._clients.getClient(this.apiName);
  }
}
