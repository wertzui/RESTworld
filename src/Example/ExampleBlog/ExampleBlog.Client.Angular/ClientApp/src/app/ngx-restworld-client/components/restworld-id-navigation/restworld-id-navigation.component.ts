import { Component, Input } from '@angular/core';
import { FormControl, FormGroup, Validators } from '@angular/forms';
import { ProblemDetails, Resource } from '@wertzui/ngx-hal-client';
import { RestWorldClient } from '../../services/restworld-client';
import { ActivatedRoute, Router } from '@angular/router';
import { MessageService } from 'primeng/api';
import { RestWorldClientCollection } from '../../services/restworld-client-collection';

/**
 * Component for navigating to a resource by its ID.
 * 
 * @remarks
 * This component is used to navigate to a resource by its ID. It takes in the API name, the `rel` of the resource, and an optional `urlPrefix` to use for the URL that is returned from the backend. It also provides a form for entering the ID of the resource to navigate to.
 */
@Component({
  selector: 'rw-id-navigation',
  templateUrl: './restworld-id-navigation.component.html',
  styleUrls: ['./restworld-id-navigation.component.css']
})
export class RestWorldIdNavigationComponent {

  @Input({ required: true })
  public apiName!: string;

  @Input({ required: true })
  public rel!: string;

  /**
   * A prefix to use for the URL that is returned from the backend.
   * If none is provided, a relative navigation will be performed which means that the last part of the current URL is replaced with the one from the backend.
   */
  @Input()
  public urlPrefix?: string;

  public idNavigationForm = new FormGroup<{
    id: FormControl<number | null>
  }>({
    id: new FormControl(null, Validators.compose([Validators.min(1), Validators.max(Number.MAX_SAFE_INTEGER)]))
  });

  constructor(
    private readonly _clients: RestWorldClientCollection,
    private readonly _messageService: MessageService,
    private readonly _router: Router,
    private readonly _route: ActivatedRoute) {
  }

  public async navigateById(): Promise<void> {
    if (!this.rel)
      throw new Error('The "rel" must be set through the uri of this page for the ID navigation to work.');

    if (!this.idNavigationForm.valid) {
      this._messageService.add({ detail: 'You must enter a valid ID to naviage to.', severity: 'error' });
      return;
    }
    var idToNavigateTo = this.idNavigationForm.controls.id.value;

    var client = this.getClient();

    var response = await client.getList<Resource>(this.rel, { $filter: `id eq ${idToNavigateTo}` });
    if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resources from the API.', data: response });
      return;
    }

    var resource = response.body?._embedded?.items?.[0];
    if (!resource) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: 'No resource found with the specified ID.' });
      return;
    }

    if(this.urlPrefix !== undefined)
      await this._router.navigate([this.urlPrefix, resource._links.self[0].href]);
    else
      await this._router.navigate(["..", resource._links.self[0].href], { relativeTo: this._route});

    this.idNavigationForm.reset();
  }

  private getClient(): RestWorldClient {
    if (!this.apiName)
      throw new Error('Cannot get a client, because the apiName is not set.');

    return this._clients.getClient(this.apiName);
  }
}
