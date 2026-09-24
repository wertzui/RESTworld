import { Component, computed, input, linkedSignal, model, resource } from '@angular/core';
import { Router } from '@angular/router';
import { PagedListResource, ProblemDetails, Resource, ResourceDto, ResourceOfDto, Template, type PropertyDto, type SimpleValue } from '@wertzui/ngx-hal-client';
import { ConfirmationService, MenuItem, MessageService } from 'primeng/api';
import { ConfirmDialogModule } from 'primeng/confirmdialog';
import { RestWorldSignalTableComponent } from "../../components/restworld-signal-table/restworld-signal-table.component";
import { ODataParameters } from '../../models/o-data';
import { AvatarGenerator } from '../../services/avatar-generator';
import { ProblemService } from "../../services/problem.service";
import { RestWorldClientCollection } from '../../services/restworld-client-collection';

/**
 * A component that displays a list of resources from a RESTworld API, using Signal Forms.
 * This is the Signal Forms equivalent of {@link RESTworldListViewComponent} `<rw-list>`.
 * @remarks
 * This component itself does not use any forms (Reactive or Signal) - it only nests
 * {@link RestWorldSignalTableComponent} `<rw-signal-table>` in place of `<rw-table>` for a read-only list.
 * It is meant to be used in conjunction with {@link RestWorldSignalEditViewComponent} `<rw-signal-edit>`, but
 * can also be used with any other component if a custom `editLink` is provided.
 * @example
 * <rw-signal-list apiName="api" rel="rel"></rw-signal-list>
 * @example
 * <rw-signal-list apiName="api" rel="rel" [editLink]="'/edit'"></rw-signal-list>
 * @example
 * <rw-signal-list apiName="api" rel="rel" [createButtonMenu]="[{label: 'Create new', icon: 'fas fa-plus', routerLink: ['/edit', 'api', 'rel', 'new']}]" [editLink]="'/edit'"></rw-signal-list>
 */
@Component({
    selector: 'rw-signal-list',
    templateUrl: './restworld-signal-list-view.component.html',
    styleUrls: ['./restworld-signal-list-view.component.css'],
    imports: [RestWorldSignalTableComponent, ConfirmDialogModule]
})
export class RESTworldSignalListViewComponent<TListDto extends ResourceDto & Record<string, unknown>> {
    /**
    * An array of menu items to be displayed in the create button dropdown menu.
    * That is the menu at the top right of the list.
    */
    public readonly createButtonMenu = input<MenuItem[]>();
    public isLoading = computed(() => this.listResource.isLoading() || this.searchTemplate.isLoading());
    public readonly filter = model<string | undefined>(undefined, { alias: "$filter" });
    public readonly orderby = model<string | undefined>(undefined, { alias: "$orderby" });
    public readonly skip = model<number | undefined>(undefined, { alias: "$skip" });
    public readonly top = model<number>(10, { alias: "$top" });

    public readonly oDataParameters = linkedSignal(() => {
        const parameters: ODataParameters = {
            $filter: this.filter(),
            $orderby: this.orderby(),
            $skip: this.skip(),
            $top: this.top() ?? 10
        };
        return parameters;
    });

    /**
     * The URL for the edit link of the RESTworld list view.
     * Use it if you want to use a custom edit view instead of the default `RESTworldSignalEditViewComponent`.
     * Defaults to `/edit/signal` (unlike {@link RESTworldListViewComponent}'s `/edit`), since this component's
     * row/create links should point at the Signal Forms edit view by default.
     * @param value The new value for the edit link URL.
     */
    public readonly editLink = input("/edit/signal", { transform: (value) => value ?? "/edit/signal" });

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
            this.listResource.set(RESTworldSignalListViewComponent._emptylistResource);
            this.searchTemplate.set(RESTworldSignalListViewComponent._emptySearchTemplate);
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
                return RESTworldSignalListViewComponent._emptylistResource;

            const response = await client.getList<TListDto>(params.rel, params.oDataParameters);
            if (this._problemService.checkResponseAndDisplayErrors(response, undefined, "Error while loading the resources from the API.", "Error")) {
                return response.body;
            }

            return RESTworldSignalListViewComponent._emptylistResource;
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
                return RESTworldSignalListViewComponent._emptySearchTemplate;

            // We only want to load the template once
            const currentSearchTemplate = this.searchTemplate.value() as Template<ReadonlyArray<PropertyDto<SimpleValue, string, string>>> | undefined;
            if (currentSearchTemplate !== undefined && currentSearchTemplate !== RESTworldSignalListViewComponent._emptySearchTemplate)
                return currentSearchTemplate;

            try {
                const templates = await client.getAllTemplates(params.resource);
                if (ProblemDetails.isProblemDetails(templates)) {
                    this._messageService.add({ severity: 'error', summary: 'Error', detail: `No templates found in the API response.`, data: templates });
                    return RESTworldSignalListViewComponent._emptySearchTemplate;
                }

                const searchTemplate = templates["Search"];
                if (searchTemplate === undefined) {
                    this._messageService.add({ severity: 'error', summary: 'Error', detail: `No "Search" template found in the API response.`, data: templates });
                    return RESTworldSignalListViewComponent._emptySearchTemplate;
                }

                return searchTemplate;
            }
            catch (e: unknown) {
                this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resources from the API. ' + e, data: e });
                return RESTworldSignalListViewComponent._emptySearchTemplate;
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
