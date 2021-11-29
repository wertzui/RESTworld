import { HttpHeaders, HttpResponse } from '@angular/common/http';
import * as _ from "lodash";
import { FormsResource, HalClient, Link, PagedListResource, Resource, Template } from "@wertzui/ngx-hal-client"
import { LinkNames } from "../constants/link-names";
import { ProblemDetails } from "../models/problem-details";
import { RESTworldOptions } from "../models/restworld-options";

export class RESTworldClient {

  private _homeResource?: Resource;
  private _defaultCurie?: string;
  public get halClient() {
    return this._halClient;
  }

  constructor(
    private _halClient: HalClient,
    private _options: RESTworldOptions
  ) { }

  public async ensureHomeResourceIsSet(): Promise<void> {
    if (!this._homeResource) {
      const response = await this.getHomeForced();
      if (ProblemDetails.isProblemDetails(response.body)) {
        throw new Error(`Cannot get the home resource from ${this._options.BaseUrl} with Version ${this._options.Version}. Response was: ${response}`);
      }
      if (!response.body)
        throw new Error(`Cannot get the home resource from ${this._options.BaseUrl} with Version ${this._options.Version}. Response was empty.`)
      this._homeResource = response.body;
      this.setDefaultCurie();
    }
  }

  private async getHomeForced(): Promise<HttpResponse<Resource | ProblemDetails>> {
    const header = RESTworldClient.createHeaders('application/hal+json', this._options.Version);
    const response = await this.halClient.get(this._options.BaseUrl, Resource, ProblemDetails, header);
    return response;
  }

  private setDefaultCurie(): void {
    const curies = this._homeResource?._links?.curies;
    if (!curies || curies.length === 0 || !curies[0])
      this._defaultCurie = undefined;
    else
      this._defaultCurie = curies[0].name;
  }

  public async getList(rel: string, parameters: {}, headers?: HttpHeaders, curie?: string): Promise<HttpResponse<PagedListResource | ProblemDetails>>{
    const link = this.getLinkFromHome(rel, LinkNames.getList, curie);
    const uri = link.fillTemplate(parameters);

    const response = await this.halClient.get(uri, PagedListResource, ProblemDetails, headers);

    return response;
  }

  public async getListByUri(uri: string, parameters: {}, headers?: HttpHeaders): Promise<HttpResponse<PagedListResource | ProblemDetails>> {
    const link = new Link();
    link.href = uri;
    const filledUri = link.fillTemplate(parameters);

    const response = await this.halClient.get(filledUri, PagedListResource, ProblemDetails, headers);

    return response;
  }

  public async getSingle(relOrUri: string, id?: number, headers?: HttpHeaders, curie?: string): Promise<HttpResponse<Resource | ProblemDetails>> {
    let uri;
    if (relOrUri.startsWith('http')) {
      if (id !== undefined)
        throw new Error('When supplying a URI, an ID cannot be supplied too.')
      if (curie)
        throw new Error('When supplying a URI, a curie cannot be supplied too.')

      uri = relOrUri;
    }
    else {
      if (!_.isNumber(id))
        throw new Error('When supplying a rel, an ID must be supplied too.')

      const link = this.getLinkFromHome(relOrUri, LinkNames.get, curie);
      uri = link.fillTemplate({ id: id.toString() });
    }
    const response = await this.halClient.get(uri, Resource, ProblemDetails, headers);

    return response;
  }

  public async save(resource: Resource): Promise<HttpResponse<Resource | ProblemDetails>> {
    const saveLink = resource.findLink('save');
    if (!saveLink)
      throw new Error(`The resource ${resource} does not have a save link.`);
    if (!saveLink.name)
      throw new Error(`The save link ${saveLink} does not have a save name.`);

    const uri = saveLink.href;
    const method = saveLink.name.toLowerCase();
    const header = RESTworldClient.createHeaders('application/hal+json', this._options.Version);

    let response;
    switch (method) {
      case 'put':
        response = await this.halClient.put(uri, resource, Resource, ProblemDetails, header);
        break;
      case 'post':
        response = await this.halClient.post(uri, resource, Resource, ProblemDetails, header);
        break;
      default:
        throw new Error(`'${method}' is not allowed as link name for the save link. Only 'POST' and 'PUT' are allowed.`);
    }

    return response;
  }

  public async getAllForms(resource: Resource): Promise<HttpResponse<FormsResource | ProblemDetails>[]> {
    const urls = resource.getFormLinkHrefs();
    const header = RESTworldClient.createHeaders('application/prs.hal-forms+json', this._options.Version);
    const formsPromises = urls.map(url => this._halClient.get(url, FormsResource, ProblemDetails, header));
    const formsAndProblems = await Promise.all(formsPromises);
    return formsAndProblems;
  }

  public async submit(template: Template, formValues: {}): Promise<HttpResponse<FormsResource | ProblemDetails>> {
    const uri = template.target || '';
    const method = template.method?.toLowerCase();
    const header = RESTworldClient.createHeaders('application/prs.hal-forms+json', this._options.Version);

    let response;
    switch (method) {
      case 'put':
        response = await this.halClient.put(uri, formValues, FormsResource, ProblemDetails, header);
        break;
      case 'post':
        response = await this.halClient.post(uri, formValues, FormsResource, ProblemDetails, header);
        break;
      default:
        response = await this.halClient.get(uri, FormsResource, ProblemDetails, header);
    }

    return response;

  }

  public async delete(resource: Resource): Promise<HttpResponse<void | ProblemDetails>> {
    const deleteLink = resource.findLink('delete');
    if (!deleteLink)
      throw new Error(`The resource ${resource} does not have a delete link.`);
    const uri = deleteLink.href;
    const header = RESTworldClient.createHeaders('application/hal+json', this._options.Version);

    const response = await this.halClient.delete(uri, ProblemDetails, header);

    return response;
  }

  public getAllLinksFromHome(): { [rel: string]: Link[] | undefined } {
    if (!this._homeResource)
      throw new Error('Home resource is not set. Call ensureHomeResourceIsSet() first.');

    return this._homeResource._links;
  }

  public getLinkFromHome(rel: string, name?: string, curie?: string): Link {
    const links = this.getLinksFromHome(rel, curie);

    const link = name ? links.find(l => l.name === name) : links[0];

    if (!link)
      throw new Error(`The home resource does not have a link with the rel '${this.getFullRel(rel, curie)}' and the name '${name}'.`)

    return link;
  }

  public getLinksFromHome(rel: string, curie?: string): Link[] {
    if (!this._homeResource)
      throw new Error('Home resource is not set. Call ensureHomeResourceIsSet() first.');

    const fullRel = this.getFullRel(rel, curie);
    const links = this._homeResource._links[fullRel];
    if (!links || links.length === 0)
      throw Error(`The home resource does not have a link with the rel '${fullRel}'.`);

    return links;
  }

  private getFullRel(rel: string, curie?: string): string {
    // rel already includes a curie => just return it
    if (rel.includes(':'))
      return rel;

    // No curie given => use default curie.
    if (!curie)
      curie = this._defaultCurie;

    // Combine curie and rel
    const fullRel = `${curie}:${rel}`;

    return fullRel;
  }

  private static createHeaders(mediaType?: 'application/hal+json' | 'application/prs.hal-forms+json', version?: number): HttpHeaders {
    if (version)
      return new HttpHeaders({ 'Accept': `${mediaType || 'application/hal+json'}; v=${version}` });
    return new HttpHeaders();
  }
}
