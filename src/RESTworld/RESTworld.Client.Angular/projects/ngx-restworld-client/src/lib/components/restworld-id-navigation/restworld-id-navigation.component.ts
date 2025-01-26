import { Component, computed, input } from '@angular/core';
import { FormControl, FormGroup, ReactiveFormsModule, Validators } from '@angular/forms';
import { ProblemDetails, Resource } from '@wertzui/ngx-hal-client';
import { RestWorldClient } from '../../services/restworld-client';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { RestWorldClientCollection } from '../../services/restworld-client-collection';
import { InputNumberModule } from "primeng/inputnumber";
import { ButtonModule } from "primeng/button";

/**
 * Component for navigating to a resource by its ID.
 * 
 * @remarks
 * This component is used to navigate to a resource by its ID. It takes in the API name, the `rel` of the resource, and an optional `urlPrefix` to use for the URL that is returned from the backend. It also provides a form for entering the ID of the resource to navigate to.
 */
@Component({
    selector: 'rw-id-navigation',
    templateUrl: './restworld-id-navigation.component.html',
    styleUrls: ['./restworld-id-navigation.component.css'],
    standalone: true,
    imports: [InputNumberModule, ButtonModule, ReactiveFormsModule]
})
export class RestWorldIdNavigationComponent {
    public readonly apiName = input.required<string>();
    public readonly rel = input.required<string>();
    /**
     * A prefix to use for the URL that is returned from the backend.
     * If none is provided, a relative navigation will be performed which means that the last part of the current URL is replaced with the one from the backend.
     */
    public readonly urlPrefix = input<string>();

    public idNavigationForm = new FormGroup<{
        id: FormControl<number | null>
    }>({
        id: new FormControl(null, Validators.compose([Validators.min(1), Validators.max(Number.MAX_SAFE_INTEGER)]))
    });

    private _client = computed(() => this._clients.getClient(this.apiName()));

    constructor(
        private readonly _clients: RestWorldClientCollection,
        private readonly _messageService: MessageService,
        private readonly _router: Router,
        private readonly _route: ActivatedRoute) {
    }

    public async navigateById(): Promise<void> {
        const rel = this.rel();
        if (!rel)
            throw new Error('The "rel" must be set through the uri of this page for the ID navigation to work.');

        if (!this.idNavigationForm.valid) {
            this._messageService.add({ detail: 'You must enter a valid ID to naviage to.', severity: 'error' });
            return;
        }
        var idToNavigateTo = this.idNavigationForm.controls.id.value;

        var client = this._client();

        var response = await client.getList<Resource>(rel, { $filter: `id eq ${idToNavigateTo}` });
        if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
            this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resources from the API.', data: response });
            return;
        }

        var resource = response.body?._embedded?.items?.[0];
        if (!resource) {
            this._messageService.add({ severity: 'error', summary: 'Error', detail: 'No resource found with the specified ID.' });
            return;
        }

        const urlPrefix = this.urlPrefix();
        if (urlPrefix !== undefined)
            await this._router.navigate([urlPrefix, resource._links.self[0].href]);
        else
            await this._router.navigate(["..", resource._links.self[0].href], { relativeTo: this._route });

        this.idNavigationForm.reset();
    }
}
