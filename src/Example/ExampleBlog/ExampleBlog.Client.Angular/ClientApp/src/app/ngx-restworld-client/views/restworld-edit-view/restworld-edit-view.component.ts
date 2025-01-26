import { Component, ContentChild, TemplateRef, computed, effect, input, model, resource, signal, untracked } from "@angular/core";
import { FormControl, FormGroup, Validators } from "@angular/forms";
import { Router } from "@angular/router";
import { ProblemDetails, PropertyType, Resource, Template, Templates } from "@wertzui/ngx-hal-client";
import { ValdemortConfig } from "ngx-valdemort";
import { MessageService } from "primeng/api";
import { ConfirmDialogModule } from "primeng/confirmdialog";
import { SkeletonModule } from "primeng/skeleton";
import { TabsModule } from "primeng/tabs";
import { RestWorldFormComponent } from "../../components/restworld-form/restworld-form.component";
import { RestWorldIdNavigationComponent } from "../../components/restworld-id-navigation/restworld-id-navigation.component";
import { AfterSubmitOkEvent, AfterSubmitRedirectEvent } from "../../models/events";
import { RestWorldClientCollection } from "../../services/restworld-client-collection";

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
*  <ng-template #extraTabs>
*    <p-tabPanel header="Extra tab">
*      <p>Some extra content</p>
*    </p-tabPanel>
*  </ng-template>
* </rw-edit>
*/
@Component({
    selector: "rw-edit",
    templateUrl: "./restworld-edit-view.component.html",
    styleUrls: ["./restworld-edit-view.component.css"],
    standalone: true,
    imports: [RestWorldIdNavigationComponent, SkeletonModule, TabsModule, RestWorldFormComponent, ConfirmDialogModule]
})
export class RESTworldEditViewComponent {
    @ContentChild("extraTabs", { static: false })
    /**
    * A reference to an optional template that can be used to add extra tabs to the view.
    */
    public extraTabsRef?: TemplateRef<unknown>;
    public idNavigationForm = new FormGroup<{
        id: FormControl<number | null>
    }>({
        id: new FormControl(null, Validators.compose([Validators.min(1), Validators.max(Number.MAX_SAFE_INTEGER)]))
    });

    constructor(
        private readonly _clients: RestWorldClientCollection,
        private readonly _messageService: MessageService,
        private readonly _router: Router,
        valdemortConfig: ValdemortConfig) {
        valdemortConfig.errorClasses = "p-error text-sm";
    }

    public get PropertyType() {
        return PropertyType;
    }

    /**
    * The relation to load the resource from.
    */
    public readonly rel = input.required<string>();

    /**
    * The URI of the resource to load.
    */
    public uri = input.required<string>();
    /**
    * The name of the API to load the resource from.
    */
    public apiName = input.required<string>();
    public templates = resource({
        request: () => ({apiName: this.apiName(), uri: this.uri()}),
        loader: ({request}) => this.loadInternal(request.apiName, request.uri)
    });
    public displayTab = model("Loading");

    async afterSubmit($event: AfterSubmitOkEvent | AfterSubmitRedirectEvent) {
        if ($event.status == 201) {
            await this._router.navigate(["edit", this.apiName, this.rel(), $event.location])
        }
    }

    async afterDelete(): Promise<void> {
        await this._router.navigate(["list", this.apiName, this.rel()])
    }

    private async loadInternal(apiName: string, uri: string): Promise<Template[]> {
        if (!apiName || !uri)
            return [];

        const client = this._clients.getClient(apiName);
        const response = await client.getSingleByUri(uri);
        if (!response.ok || ProblemDetails.isProblemDetails(response.body) || response.body === null) {
            if (ProblemDetails.isProblemDetails(response.body))
                this._messageService.add({ severity: "error", summary: "Error", detail: response.body.detail, data: response, sticky: true });
            else
                this._messageService.add({ severity: "error", summary: "Error", detail: "Error while loading the resource from the API.", data: response, sticky: true });
        }
        else {
            const templates = await this.getAllTemplates(response.body, apiName);
            const templateArray = Object.values(templates).filter((t: Template | undefined): t is Template => t !== undefined);
            if (templateArray.length > 0)
                this.displayTab.set(templateArray[0].title!);
            return templateArray;
        }

        return [];
    }

    private async getAllTemplates(resource: Resource, apiName: string): Promise<Templates> {
        try {
            const client = this._clients.getClient(apiName);
            const templates = await client.getAllTemplates(resource);
            return templates;
        }
        catch (e: unknown) {
            this._messageService.add({ severity: "error", summary: "Error", detail: "Error while loading the templates from the API. " + e, data: e, sticky: true });
            return {};
        }
    }
}
