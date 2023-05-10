import { HttpHeaders, HttpResponse } from '@angular/common/http';
import * as _ from "lodash";
import { FormsResource, HalClient, Link, PagedListResource, Property, Resource, ResourceDto, Template, Templates } from "@wertzui/ngx-hal-client";
import { LinkNames } from "../constants/link-names";
import { ProblemDetails } from "../models/problem-details";
import { RestWorldOptions } from "../models/restworld-options";
import { lastValueFrom } from 'rxjs';
import { AbstractControl, FormGroup } from '@angular/forms';

export class RestWorldClient {
  private _defaultCurie?: string;
  private _homeResource?: Resource;

  constructor(
    private _halClient: HalClient,
    private _options: RestWorldOptions
  ) { }

  public get halClient() {
    return this._halClient;
  }

  private static combineHeaders(originalHeaders: HttpHeaders | undefined, headersToAdd: HttpHeaders | undefined, overwriteExisting: boolean) {
    if (!headersToAdd)
      return originalHeaders;
    if (!originalHeaders)
      return headersToAdd;

    let combinedHeaders = originalHeaders;

    for (const key in headersToAdd.keys) {
      if (!combinedHeaders.has(key) || overwriteExisting) {
        const headerValuesToAdd = headersToAdd.getAll(key);
        if (headerValuesToAdd)
          combinedHeaders = combinedHeaders.set(key, headerValuesToAdd);
      }
    }

    return combinedHeaders;
  }

  private static createHeaders(mediaType?: 'application/hal+json' | 'application/prs.hal-forms+json' | 'text/csv', version?: number): HttpHeaders {
    if (version)
      return new HttpHeaders({
        'Accept': `${mediaType || 'application/hal+json'}; v=${version}`,
        'Content-Type': `${mediaType || 'application/hal+json'}; v=${version}`
      });
    return new HttpHeaders();
  }

  public async delete(resource: Resource): Promise<HttpResponse<void | ProblemDetails>> {
    const deleteLink = resource.findLink('delete');
    if (!deleteLink)
      throw new Error(`The resource ${resource} does not have a delete link.`);
    const uri = deleteLink.href;
    const header = RestWorldClient.createHeaders('application/hal+json', this._options.Version);

    const response = await this.halClient.delete(uri, ProblemDetails, header);

    return response;
  }

  public async ensureHomeResourceIsSet(): Promise<void> {
    if (!this._homeResource) {
      const response = await this.getHomeForced();
      if (ProblemDetails.isProblemDetails(response.body)) {
        throw new Error(`Cannot get the home resource from ${this._options.BaseUrl} with Version ${this._options.Version}. Response was: ${response}`);
      }
      if (!response.body)
        throw new Error(`Cannot get the home resource from ${this._options.BaseUrl} with Version ${this._options.Version}. Response was empty.`);
      this._homeResource = response.body;
      this.setDefaultCurie();
    }
  }

  public async getAllForms(resource: Resource): Promise<HttpResponse<FormsResource | ProblemDetails>[]> {
    const urls = resource.getFormLinkHrefs();
    const formsPromises = urls.map(url => this.getForm(url));
    const formsAndProblems = await Promise.all(formsPromises);
    return formsAndProblems;
  }

  public async getForm(url: string): Promise<HttpResponse<FormsResource | ProblemDetails>> {
    const header = RestWorldClient.createHeaders('application/prs.hal-forms+json', this._options.Version);
    return this._halClient.get(url, FormsResource, ProblemDetails, header);
  }

  public getAllLinksFromHome(): { [rel: string]: Link[] | undefined; } {
    if (!this._homeResource)
      throw new Error('Home resource is not set. Call ensureHomeResourceIsSet() first.');

    return this._homeResource._links;
  }

  public async getAllPagesFromList<TListDto extends ResourceDto>(rel: string, parameters: {}, headers?: HttpHeaders, curie?: string): Promise<HttpResponse<PagedListResource<TListDto> | ProblemDetails>> {
    const link = this.getLinkFromHome(rel, LinkNames.getList, curie);
    const uri = link.href;

    const response = await this.getAllPagesFromListByUri<TListDto>(uri, parameters, headers);

    return response;
  }

  public async getAllPagesFromListByUri<TListDto extends ResourceDto>(uri: string, parameters: {}, headers?: HttpHeaders): Promise<HttpResponse<PagedListResource<TListDto> | ProblemDetails>> {
    const response = await this.getListByUri<TListDto>(uri, parameters, headers);

    if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body?._embedded?.items)
      return response;

    const items = response.body._embedded.items;
    let lastResponse = response;

    while ((lastResponse?.body?._links?.next?.length ?? 0) > 0) {
      // Get the next response
      const nextLinks = lastResponse.body?._links.next;
      const nextLink = nextLinks ? nextLinks[0]: undefined;
      const nextHref = nextLink?.href;
      if (nextHref) {
        lastResponse = await this.getListByUri<TListDto>(nextHref, parameters, headers);

        if (!lastResponse.ok || ProblemDetails.isProblemDetails(lastResponse.body) || !lastResponse.body)
          return lastResponse;

        // Combine the embedded items
        lastResponse.body?._embedded?.items?.forEach(r => items.push(r));
      }
    }

    // We combined everything, so there is just one big page
    response.body.totalPages = 1;
    response.body._links.next = undefined;

    return response;
  }

  public async getAllTemplates(resource: Resource): Promise<Templates> {
    const formResponses = await this.getAllForms(resource);

    const failedResponses = formResponses.filter(response => !response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body);
    if (failedResponses.length !== 0) {
      for (const response of failedResponses) {
        throw new Error(`Error while loading the response from ${response.url}, Status ${response.status} - ${response.statusText}, Content: ${JSON.stringify(response.body)}`);
      }
      return Promise.resolve({});
    }

    const formTemplates = Object.assign({}, ...formResponses.map(response => (response.body as FormsResource)._templates)) as Templates;

    return formTemplates;
  }

  public getLinkFromHome(rel: string, name?: string, curie?: string): Link {
    const links = this.getLinksFromHome(rel, curie);

    const link = name ? links.find(l => l.name === name) : links[0];

    if (!link)
      throw new Error(`The home resource does not have a link with the rel '${this.getFullRel(rel, curie)}' and the name '${name}'.`);

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

  public async getList<TListDto extends ResourceDto>(rel: string, parameters: {}, headers?: HttpHeaders, curie?: string): Promise<HttpResponse<PagedListResource<TListDto> | ProblemDetails>> {
    const link = this.getLinkFromHome(rel, LinkNames.getList, curie);
    const uri = link.href

    const response = await this.getListByUri<TListDto>(uri, parameters, headers);

    return response;
  }

  public async getListAsCsv(rel: string, parameters: {}, headers?: HttpHeaders, curie?: string): Promise<HttpResponse<Blob> | undefined> {
    const link = this.getLinkFromHome(rel, LinkNames.getList, curie);
    const uri = link.href;

    const response = await this.getListByUriAsCsv(uri, parameters, headers);

    return response;
  }

  public async getListByUri<TListDto extends ResourceDto>(uri: string, parameters: {}, headers?: HttpHeaders): Promise<HttpResponse<PagedListResource<TListDto> | ProblemDetails>> {
    const link = new Link();
    link.href = uri;
    const filledUri = link.fillTemplate(parameters);
    const defaultHeaders = RestWorldClient.createHeaders('application/hal+json', this._options.Version);
    const combinedHeaders = RestWorldClient.combineHeaders(headers, defaultHeaders, false);

    const response = await this.halClient.get<PagedListResource<TListDto>, ProblemDetails>(filledUri, PagedListResource, ProblemDetails, combinedHeaders);

    return response;
  }

  public async getListByUriAsCsv(uri: string, parameters: {}, headers?: HttpHeaders): Promise<HttpResponse<Blob> | undefined> {
    const link = new Link();
    link.href = uri;
    const filledUri = link.fillTemplate(parameters);
    const defaultHeaders = RestWorldClient.createHeaders('text/csv', this._options.Version);
    const combinedHeaders = RestWorldClient.combineHeaders(headers, defaultHeaders, false);

    const response = await lastValueFrom(this.halClient.httpClient.get(filledUri, { headers: combinedHeaders, responseType: 'blob', observe: 'response' }));

    return response;
  }

  public async getSingle(rel: string, id: number, headers?: HttpHeaders, curie?: string): Promise<HttpResponse<Resource | ProblemDetails>> {
    if (!_.isNumber(id))
      throw new Error('When supplying a rel, an ID must be supplied too.');

    const link = this.getLinkFromHome(rel, LinkNames.get, curie);
    const uri = link.fillTemplate({ id: id.toString() });

    const response = this.getSingleByUri(uri, headers);

    return response;
  }

  public async getSingleByUri(uri: string, headers?: HttpHeaders): Promise<HttpResponse<Resource | ProblemDetails>> {
    const defaultHeaders = RestWorldClient.createHeaders('application/hal+json', this._options.Version);
    const combinedHeaders = RestWorldClient.combineHeaders(headers, defaultHeaders, false);
    const response = await this.halClient.get(uri, Resource, ProblemDetails, combinedHeaders);

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
    const header = RestWorldClient.createHeaders('application/hal+json', this._options.Version);

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

  public async submit(template: Template, formValues: {}): Promise<HttpResponse<FormsResource | ProblemDetails>> {
    const uri = template.target || '';
    const method = template.method?.toLowerCase();
    const header = RestWorldClient.createHeaders('application/prs.hal-forms+json', this._options.Version);

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

  private async getHomeForced(): Promise<HttpResponse<Resource | ProblemDetails>> {
    const header = RestWorldClient.createHeaders('application/hal+json', this._options.Version);
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

  public async deleteByTemplateAndForm(template: Template, formGroup: FormGroup<{timestamp: AbstractControl<string>}>): Promise<HttpResponse<void | ProblemDetails>> {
    const url = template.target;
    if(url === undefined)
      throw new Error("The target of the given template cannot be undefined.");

    const timestamp = formGroup.value.timestamp

    return this.deleteByUrl(url, timestamp);
  }

  public async deleteByUrl(url: string, timestamp?: string): Promise<HttpResponse<void | ProblemDetails>> {
    let header = RestWorldClient.createHeaders('application/hal+json', this._options.Version);

    if (timestamp !== undefined)
      header = header.append("ETag", timestamp);

    const response = await this.halClient.delete(url, ProblemDetails, header);

    return response;
  }

}
