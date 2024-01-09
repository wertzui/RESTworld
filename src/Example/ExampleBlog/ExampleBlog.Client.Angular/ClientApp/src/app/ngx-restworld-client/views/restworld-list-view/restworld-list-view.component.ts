import { Component, Input } from '@angular/core';
import { PagedListResource, ProblemDetails, Resource, ResourceDto, ResourceOfDto, Template } from '@wertzui/ngx-hal-client';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { RestWorldClient } from '../../services/restworld-client';
import { RestWorldClientCollection } from '../../services/restworld-client-collection';
import { AvatarGenerator } from '../../services/avatar-generator';
import { Router } from '@angular/router';
import { ODataParameters } from '../../models/o-data';
import { debounce } from '../../util/debounce';

/**
 * A component that displays a list of resources from a RESTworld API.
 * This component is meant to be used in conjunction with the `RESTworldEditViewComponent`, but can also be used with any other component if a custom `editLink` is provided.
 * @example
 * <rw-list apiName="api" rel="rel"></rw-list>
 * @example
 * <rw-list apiName="api" rel="rel" [editLink]="'/edit'"></rw-list>
 * @example
 * <rw-list apiName="api" rel="rel" [createButtonMenu]="[{label: 'Create new', icon: 'fas fa-plus', routerLink: ['/edit', 'api', 'rel', 'new']}]" [editLink]="'/edit'"></rw-list>
 * @example
 * <rw-list apiName="api" rel="rel" [createButtonMenu]="[{label: 'Create new', icon: 'fas fa-plus', routerLink: ['/edit', 'api', 'rel', 'new']}]" [editLink]="'/edit'">
 *  <ng-template pTemplate="header">
 *   <div class="p-d-flex p-ai-center">
 *    <div class="p-mr-2">
 *    <i class="fas fa-search"></i>
 *   </div>
 *  <div class="p-inputgroup">
 *  <input pInputText type="text" placeholder="Search..." (input)="load({$search: $event.target.value})">
 * </div>
 * </div>
 * </ng-template>
 * </rw-list>
 *
 */
@Component({
  selector: 'rw-list',
  templateUrl: './restworld-list-view.component.html',
  styleUrls: ['./restworld-list-view.component.css']
})
export class RESTworldListViewComponent<TListDto extends ResourceDto & Record<string, unknown>> {

  @Input()
  /**
   * An array of menu items to be displayed in the create button dropdown menu.
   * That is the menu at the top right of the list.
   */
  public createButtonMenu?: MenuItem[];
  public isLoading = false;
  public load = debounce(this.loadInternal, 100);
  public resource?: PagedListResource<TListDto>;

  private _editLink = '/edit';

  public get editLink() {
    return this._editLink;
  }

  @Input()
  /**
   * The URL for the edit link of the RESTworld list view.
   * Use it if you want to use a custom edit view instead of the default `RESTworldEditViewComponent`.
   * @param value The new value for the edit link URL.
   */
  public set editLink(value: string) {
    if(value)
      this._editLink = value;
  }
  private _lastParameters: ODataParameters = {};
  private _searchTemplate: Template = new Template({ properties: [] });
  public _totalRecords = 0;

  public headerMenu: MenuItem[] = [];
  public rowMenu: (row: ResourceOfDto<TListDto>, openedByRightClick: boolean) => MenuItem[] = (row, openedByRightClick) => {
    return [
      {
        icon: "fas fa-edit",
        label: openedByRightClick ? "View / Edit" : undefined,
        tooltip: !openedByRightClick ? "View / Edit" : undefined,
        tooltipPosition: "left",
        routerLink: [this.editLink, this.apiName, this.rel, row._links?.self[0].href]
      }, {
        icon: "fas fa-trash-alt",
        label: openedByRightClick ? "Delete" : undefined,
        tooltip: !openedByRightClick ? "Delete" : undefined,
        tooltipPosition: "left",
        styleClass: "p-button-danger",
        command: () => this.showDeleteConfirmatioModal(row)
      }
    ];
  }

  constructor(
    private readonly _clients: RestWorldClientCollection,
    private readonly _confirmationService: ConfirmationService,
    private readonly _messageService: MessageService,
    public readonly avatarGenerator: AvatarGenerator,
    private readonly _router: Router) {
  }

  private _apiName!: string;

  @Input({ required: true })
  /**
   * Sets the name of the API to load and triggers a reload of the data.
   * @param value The name of the API to load.
   */
  public set apiName(value: string) {
    this._apiName = value;
    this.load(this._lastParameters);
  };

  public get apiName() {
    return this._apiName;
  }

  public get newHref(): string | undefined {
    return this.resource?.findLink('new')?.href;
  }

  private _rel!: string;
  public get rel(): string {
    return this._rel;
  }
  @Input({ required: true })
  /**
   * Sets the rel value for the RESTWorld list view component.
   * @param value The new rel value to set.
   */
  public set rel(value: string) {
    this._rel = value;
    this.load(this._lastParameters);
  }

  @Input()
  /**
   * Sets the initial $orderBy value for the RESTWorld list view component.
   * @param value The new $orderBy value to set.
   */
  public set initialOrderby(value: string | undefined) {
    this._lastParameters.$orderby = value;
  }

  public get searchTemplate(): Template {
    return this._searchTemplate;
  }

  public get totalRecords(): number {
    return this._totalRecords;
  }

  public get value(): ResourceOfDto<TListDto>[] {
    return this.resource?._embedded?.items || [];
  }

  public createNew(): Promise<boolean> {
    return this._router.navigate([this.editLink, this.apiName, this.rel, this.newHref]);
  }

  public async delete(resource: Resource): Promise<void> {
    if (!this.apiName || !this.rel)
      return;

    await this.getClient().delete(resource);

    this._messageService.add({ severity: 'success', summary: 'Deleted', detail: 'The resource has been deleted.' });

    this.loadInternal(this._lastParameters);
  }

  public async loadInternal(parameters: ODataParameters): Promise<void> {
    if (!this.apiName || !this.rel)
      return;

    this.isLoading = true;

    const response = await this.getClient().getList<TListDto>(this.rel, parameters);
    if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resources from the API.', data: response });
    }
    else if (response.body) {
      try {
        const templates = await this.getClient().getAllTemplates(response.body);
        const searchTemplate = templates["Search"];
        if (searchTemplate === undefined)
          throw new Error("No search template found in the API response.");
        this._searchTemplate = searchTemplate;
      }
      catch (e: unknown) {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resources from the API. ' + e, data: e });
      }

      this.resource = response.body;
      const rowsPerPage = parameters.$top ?? response.body._embedded.items.length;
      const totalPages = response.body.totalPages ?? 1;
      this._totalRecords = totalPages * rowsPerPage;

      this.headerMenu = [
        {
          icon: "fas fa-plus",
          styleClass: "p-button-success",
          routerLink: [this.editLink, this.apiName, this.rel, this.newHref],
          items: this.createButtonMenu
        }
      ];
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

  private getClient(): RestWorldClient {
    if (!this.apiName)
      throw new Error('Cannot get a client, because the apiName is not set.');

    return this._clients.getClient(this.apiName);
  }
}
