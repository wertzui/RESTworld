import { Component, computed, effect, input, linkedSignal, model, resource } from '@angular/core';
import { PagedListResource, ProblemDetails, Resource, ResourceDto, ResourceOfDto, Template, type PropertyDto, type SimpleValue } from '@wertzui/ngx-hal-client';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { RestWorldClientCollection } from '../../services/restworld-client-collection';
import { AvatarGenerator } from '../../services/avatar-generator';
import { Router } from '@angular/router';
import { ODataParameters } from '../../models/o-data';
import { RestWorldTableComponent } from "../../components/restworld-table/restworld-table.component";
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { ProblemService } from "../../services/problem.service";

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
    styleUrls: ['./restworld-list-view.component.css'],
    standalone: true,
    imports: [RestWorldTableComponent, ConfirmDialogModule]
})
export class RESTworldListViewComponent<TListDto extends ResourceDto & Record<string, unknown>> {
    /**
    * An array of menu items to be displayed in the create button dropdown menu.
    * That is the menu at the top right of the list.
    */
    public readonly createButtonMenu = input<MenuItem[]>();
    public isLoading = computed(() => this.listResource.isLoading() || this.searchTemplate.isLoading());
    public readonly filter = model<string | undefined>(undefined, { alias: "$filter" });
    public readonly orderby = model<string | undefined>(undefined, { alias: "$orderby" });
    public readonly skip = model<number | undefined>(undefined, { alias: "$skip" });
    public readonly top = model<number | undefined>(undefined, { alias: "$top" });

    public readonly oDataParameters = linkedSignal(() => {
        const parameters: ODataParameters = {
            $filter: this.filter(),
            $orderby: this.orderby(),
            $skip: this.skip(),
            $top: this.top()
        };
        return parameters;
    });

    /**
     * The URL for the edit link of the RESTworld list view.
     * Use it if you want to use a custom edit view instead of the default `RESTworldEditViewComponent`.
     * @param value The new value for the edit link URL.
     */
    public readonly editLink = input("/edit", { transform: (value) => value ?? "/edit" });

    public readonly headerMenu = computed(() => [
        {
            icon: "fas fa-plus",
            styleClass: "p-button-success",
            routerLink: [this.editLink(), this.apiName(), this.rel(), this.newHref()],
            items: this.createButtonMenu()
        }
    ]);

    public readonly rowMenu = computed<(row: ResourceOfDto<TListDto>, openedByRightClick: boolean) => MenuItem[]>(() => (row, openedByRightClick) => {
        return [
            {
                icon: "fas fa-edit",
                label: openedByRightClick ? "View / Edit" : undefined,
                tooltip: !openedByRightClick ? "View / Edit" : undefined,
                tooltipPosition: "left",
                routerLink: [this.editLink(), this.apiName(), this.rel(), row._links?.self[0].href]
            }, {
                icon: "fas fa-trash-alt",
                label: openedByRightClick ? "Delete" : undefined,
                tooltip: !openedByRightClick ? "Delete" : undefined,
                tooltipPosition: "left",
                styleClass: "p-button-danger",
                command: () => this.showDeleteConfirmatioModal(row)
            }
        ];
    });

    private static readonly _emptylistResource = new PagedListResource({ _embedded: { items: [] }, _links: { self: [] } });

    private static readonly _emptySearchTemplate: Template<ReadonlyArray<PropertyDto<SimpleValue, string, string>>> = new Template({ properties: [] });

    constructor(
        private readonly _clients: RestWorldClientCollection,
        private readonly _confirmationService: ConfirmationService,
        private readonly _messageService: MessageService,
        public readonly avatarGenerator: AvatarGenerator,
        private readonly _router: Router,
        private readonly _problemService: ProblemService) {
            this.listResource.set(RESTworldListViewComponent._emptylistResource);
            this.searchTemplate.set(RESTworldListViewComponent._emptySearchTemplate);
            effect(() => console.log((this.rel())));
    }

    /**
     * Sets the name of the API to load and triggers a reload of the data.
     * @param value The name of the API to load.
     */
    public readonly apiName = input<string>();
    /**
     * Sets the rel value for the RESTWorld list view component.
     * @param value The new rel value to set.
     */
    public readonly rel = input<string>();

    public readonly newHref = computed(() => this.listResource.value()?.findLink('new')?.href);
    public readonly items = computed(() => this.listResource.value()?._embedded.items || []);

    public createNew(): Promise<boolean> {
        return this._router.navigate([this.editLink, this.apiName, this.rel, this.newHref]);
    }

    public async delete(resource: Resource): Promise<void> {
        const client = this._client();
        if (client === undefined) {
            this._messageService.add({ severity: 'error', summary: 'Error', detail: 'No client found for the API.' });
            return;
        }


        const response = await client.delete(resource);
        if (this._problemService.checkResponseAndDisplayErrors(response, undefined, "Error while deleting the resource from the API.", "Error")) {
            this._messageService.add({ severity: 'success', summary: 'Deleted', detail: 'The resource has been deleted.' });

            this.listResource.reload();
        }
    }

    public readonly listResource = resource({
        params: () => ({ oDataParameters: this.oDataParameters(), rel: this.rel() }),
        loader: async ({ params }) => {
            const client = this._client();
            if (params.rel === undefined || client === undefined)
                return RESTworldListViewComponent._emptylistResource;

            const response = await client.getList<TListDto>(params.rel, params.oDataParameters);
            if (this._problemService.checkResponseAndDisplayErrors(response, undefined, "Error while loading the resources from the API.", "Error")) {
                return response.body;
            }

            return RESTworldListViewComponent._emptylistResource;
        },
    });

    public readonly totalRecords = computed(() => {
        const top = this.top();
        const listResource = this.listResource.value();
        const rowsPerPage = top ?? listResource?._embedded.items.length ?? 0;
        const totalPages = listResource?.totalPages ?? 1;
        const totalRecords = totalPages * rowsPerPage;
        return totalRecords;
    });

    public readonly searchTemplate = resource({
        params: () => ({ resource: this.listResource.value() }),
        loader: async ({ params }) => {
            const client = this._client();
            if (params.resource === undefined || client === undefined)
                return RESTworldListViewComponent._emptySearchTemplate;
            
            // We only want to load the template once
            const currentSearchTemplate = this.searchTemplate.value() as Template<ReadonlyArray<PropertyDto<SimpleValue, string, string>>> | undefined;
            if (currentSearchTemplate !== undefined && currentSearchTemplate !== RESTworldListViewComponent._emptySearchTemplate)
                return currentSearchTemplate;
            
            // return RESTworldListViewComponent._emptySearchTemplate;
            try {
                const templates = await client.getAllTemplates(params.resource);
                if (ProblemDetails.isProblemDetails(templates)) {
                    this._messageService.add({ severity: 'error', summary: 'Error', detail: `No templates found in the API response.`, data: templates });
                    return RESTworldListViewComponent._emptySearchTemplate;
                }

                const searchTemplate = templates["Search"];
                if (searchTemplate === undefined) {
                    this._messageService.add({ severity: 'error', summary: 'Error', detail: `No "Search" template found in the API response.`, data: templates });
                    return RESTworldListViewComponent._emptySearchTemplate;
                }
                
                return searchTemplate;
            }
            catch (e: unknown) {
                this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resources from the API. ' + e, data: e });
                return RESTworldListViewComponent._emptySearchTemplate;
            }
        }
    });

    public showDeleteConfirmatioModal(resource: Resource) {
        this._confirmationService.confirm({
            message: 'Do you really want to delete this resource?',
            header: 'Confirm delete',
            icon: 'far fa-trash-alt',
            accept: () => this.delete(resource)
        });
    }

    private readonly _client = computed(() => {
        const apiName = this.apiName();
        if (apiName === undefined)
            return undefined;

        return this._clients.getClient(apiName);
    });
}
