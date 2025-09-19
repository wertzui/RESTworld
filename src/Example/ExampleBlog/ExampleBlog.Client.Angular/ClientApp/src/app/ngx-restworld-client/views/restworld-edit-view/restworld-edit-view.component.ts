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
import { NgTemplateOutlet } from "@angular/common";

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
    imports: [RestWorldIdNavigationComponent, SkeletonModule, TabsModule, RestWorldFormComponent, ConfirmDialogModule, NgTemplateOutlet]
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
        params: () => ({ apiName: this.apiName(), uri: this.uri() }),
        loader: ({ params }) => this.loadInternal(params.apiName, params.uri)
    });
    public displayTab = model("Loading");

    async afterSubmit($event: AfterSubmitOkEvent | AfterSubmitRedirectEvent) {
        if ($event.status == 201) {
            await this._router.navigate(["edit", this.apiName(), this.rel(), $event.location])
        }
    }

    async afterDelete(): Promise<void> {
        await this._router.navigate(["list", this.apiName, this.rel()])
    }

    private async loadInternal(apiName: string, uri: string): Promise<Template[]> {
        if (!apiName || !uri)
            return [];
        try {
            const client = this._clients.getClient(apiName);
            const templatesOrProblemDetails = await client.getAllTemplatesByUri(uri);

            // Check if we got ProblemDetails instead of Templates
            if (ProblemDetails.isProblemDetails(templatesOrProblemDetails)) {
                this._messageService.add({
                    severity: "error",
                    summary: "Error",
                    detail: `Error from API: ${templatesOrProblemDetails.title} - ${templatesOrProblemDetails.detail}`,
                    data: templatesOrProblemDetails,
                    sticky: true
                });
                return [];
            }

            // Process the templates
            const templates = templatesOrProblemDetails as Templates;
            const templateArray = Object.values(templates).filter((t): t is Template => t !== undefined);
            if (templateArray.length > 0)
                this.displayTab.set(templateArray[0].title!);

            return templateArray;
        } catch (e: unknown) {
            if (e instanceof Error)
                this._messageService.add({ severity: "error", summary: "Error", detail: "Error while loading the templates from the API. " + e.message, data: e, sticky: true });
            else
                this._messageService.add({ severity: "error", summary: "Error", detail: "Error while loading the templates from the API. " + e, data: e, sticky: true });

            return [];
        }
    }
}
