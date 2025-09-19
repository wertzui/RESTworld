import { HttpHeaders, HttpResponse } from '@angular/common/http';
import { FormsResource, HalClient, Link, PagedListResource, PagedListResourceDto, ProblemDetails, Resource, ResourceDto, ResourceFactory, Template, Templates } from "@wertzui/ngx-hal-client";
import { LinkNames } from "../constants/link-names";
import { RestWorldOptions } from "../models/restworld-options";
import { lastValueFrom } from 'rxjs';
import { AbstractControl, FormGroup } from '@angular/forms';

/**
 * A client that can interact with a RESTworld API.
 * It leverages the {@link HalClient} to perform HTTP operations and manage resources.
 */
export class RestWorldClient {
    private _defaultCurie?: string;
    private _homeResource?: Resource;

    constructor(
        private _halClient: HalClient,
        private _options: RestWorldOptions
    ) { }

    /**
     * Gets the underlying {@link HalClient} that is used by this {@link RestWorldClient}.
     */
    public get halClient() {
        return this._halClient;
    }

    private static combineHeaders(originalHeaders: HttpHeaders | undefined, headersToAdd: HttpHeaders | undefined, overwriteExisting: boolean) {
        if (!headersToAdd)
            return originalHeaders;
        if (!originalHeaders)
            return headersToAdd;

        let combinedHeaders = originalHeaders;

        for (const key of headersToAdd.keys()) {
            if (!combinedHeaders.has(key) || overwriteExisting) {
                const headerValuesToAdd = headersToAdd.getAll(key);
                if (headerValuesToAdd)
                    combinedHeaders = combinedHeaders.set(key, headerValuesToAdd);
            }
        }

        return combinedHeaders;
    }

    private static createHeadersWithBody(
        version?: number,
        accept?: string,
        contentType?: string)
        : HttpHeaders {
        if (version)
            return new HttpHeaders({
                'Accept': `${accept ?? 'application/hal+json'}; v=${version}`,
                'Content-Type': `${contentType ?? 'application/json'}; v=${version}`
            });

        return new HttpHeaders({
            'Accept': `${accept ?? 'application/hal+json'}`,
            'Content-Type': `${contentType ?? 'application/json'}`
        });
    }

    private static createHeadersWithoutBody(
        version?: number,
        accept?: string)
        : HttpHeaders {
        if (version)
            return new HttpHeaders({
                'Accept': `${accept ?? 'application/hal+json'}; v=${version}`,
                'Content-Type': `v=${version}`
            });

        return new HttpHeaders({
            'Accept': `${accept ?? 'application/hal+json'}`,
        });
    }

    /**
     * Deletes the specified resource.
     *
     * @param resource - The resource to be deleted.
     * @returns A promise that resolves to an HttpResponse containing either void or ProblemDetails.
     * @throws An error if the resource does not have a delete link.
     */
    public async delete(resource: Resource & { timestamp?: string }): Promise<HttpResponse<void | ProblemDetails>> {
        const deleteLink = resource.findLink('delete');
        if (!deleteLink) {
            const selfLink = resource.findLink('self');
            const resourceInfo = selfLink ? `Resource with self link: ${selfLink.href}` : 'Resource (no self link)';
            throw new Error(`The resource does not have a delete link. ${resourceInfo}`);
        }
        const url = deleteLink.href;

        return this.deleteByUrl(url, resource.timestamp);
    }

    /**
     * Deletes a resource identified by the given template and timestamp supplied by the given form group.
     *
     * @param template - The template containing the target URL for the resource to be deleted.
     * @param formGroup - The form group containing the timestamp.
     * @returns A promise that resolves to an HttpResponse which can either be void or contain ProblemDetails.
     * @throws Error if the target URL in the template is undefined.
     */
    public async deleteByTemplateAndForm(template: Template, formGroup: FormGroup<{ timestamp: AbstractControl<string> }>): Promise<HttpResponse<void | ProblemDetails>> {
        const url = template.target;
        if (url === undefined)
            throw new Error("The target of the given template cannot be undefined.");

        const timestamp = formGroup.value.timestamp

        return this.deleteByUrl(url, timestamp);
    }

    /**
     * Deletes a resource at the specified URL.
     *
     * @param url - The URL of the resource to delete.
     * @param timestamp - An optional If-Match value to include in the request headers.
     * @returns A promise that resolves to an HttpResponse containing either void or ProblemDetails.
     */
    public async deleteByUrl(url: string, timestamp?: string): Promise<HttpResponse<void | ProblemDetails>> {
        let header = RestWorldClient.createHeadersWithoutBody(this._options.Version);

        if (timestamp !== undefined)
            header = header.append("If-Match", timestamp);

        const response = await this.halClient.delete(url, header);

        return response;
    }

    /**
     * Ensures that the home resource is set. If the home resource is not already set,
     * it attempts to fetch it using the `getHomeForced` method. If the response contains
     * a problem detail or is empty, an error is thrown. Otherwise, the home resource is
     * set and the default CURIE is configured.
     *
     * @remarks You should never need to call this in your application, as the home resource
     * is automatically set when the client is created in the {@link RestWorldClientCollection}.
     * @throws {Error} If the home resource cannot be fetched or the response is empty.
     * @returns {Promise<void>} A promise that resolves when the home resource is set.
     */
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

    /**
     * Retrieves all forms associated with the given resource.
     *
     * @param resource - The resource from which to retrieve form links.
     * @returns A promise that resolves to an array of HTTP responses, each containing either a `FormsResource` or `ProblemDetails`.
     */
    public async getAllForms(resource: Resource): Promise<HttpResponse<FormsResource | ProblemDetails>[]> {
        const urls = resource.getFormLinkHrefs();
        const formsPromises = urls.map(url => this.getForm(url));
        const formsAndProblems = await Promise.all(formsPromises);
        return formsAndProblems;
    }

    /**
     * Retrieves all links from the home resource.
     *
     * @returns An object where the keys are relation types (rel) and the values are arrays of {@link Link} objects or undefined.
     * @throws An error if the home resource is not set. Ensure to call {@link ensureHomeResourceIsSet} first.
     */
    public getAllLinksFromHome(): { [rel: string]: Link[] | undefined; } {
        if (!this._homeResource)
            throw new Error('Home resource is not set. Call ensureHomeResourceIsSet() first.');

        return this._homeResource._links;
    }

    /**
     * Retrieves all pages from a list resource.
     *
     * @template TListDto - The type of the list resource DTO, extending {@link ResourceDto}.
     * @param {string} rel - The relation name to identify the link.
     * @param {Object} parameters - The parameters to be sent with the request.
     * @param {HttpHeaders} [headers] - Optional HTTP headers to be sent with the request.
     * @param {string} [curie] - Optional CURIE to resolve the link.
     * @returns {Promise<HttpResponse<PagedListResource<TListDto> | ProblemDetails>>} - A promise that resolves to an HTTP response containing either a paged list resource or problem details.
     */
    public async getAllPagesFromList<TListDto extends ResourceDto>(rel: string, parameters: {}, headers?: HttpHeaders, curie?: string): Promise<HttpResponse<PagedListResource<TListDto> | ProblemDetails>> {
        const link = this.getLinkFromHome(rel, LinkNames.getList, curie);
        const uri = link.href;

        const response = await this.getAllPagesFromListByUri<TListDto>(uri, parameters, headers);

        return response;
    }

    /**
     * Retrieves all pages from a paginated list by the given URI and combines them into a single response.
     *
     * @template TListDto - The type of the list DTO, which extends {@link ResourceDto}.
     * @param {string} uri - The URI to fetch the list from.
     * @param {{}} parameters - The parameters to include in the request.
     * @param {HttpHeaders} [headers] - Optional HTTP headers to include in the request.
     * @returns {Promise<HttpResponse<PagedListResource<TListDto> | ProblemDetails>>} - A promise that resolves to an HTTP response containing either a combined paginated list resource or problem details.
     *
     * @remarks
     * This method will continue to fetch the next pages as long as there are more pages available, combining all items into a single list.
     * If any request fails or returns a problem detail, the method will return the response immediately.
     */
    public async getAllPagesFromListByUri<TListDto extends ResourceDto>(uri: string, parameters: {}, headers?: HttpHeaders): Promise<HttpResponse<PagedListResource<TListDto> | ProblemDetails>> {
        const response = await this.getListByUri<TListDto>(uri, parameters, headers);

        if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body?._embedded?.items)
            return response;

        const items = response.body._embedded.items;
        let lastResponse = response;

        while ((lastResponse?.body?._links?.next?.length ?? 0) > 0) {
            // Get the next response
            const nextLinks = lastResponse.body?._links.next;
            const nextLink = nextLinks ? nextLinks[0] : undefined;
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

    /**
     * Retrieves all templates associated with a given resource.
     *
     * This method fetches all forms related to the provided {@link Resource} and extracts the templates from the responses.
     * If any of the form responses indicate a failure, it creates a combined ProblemDetails object with information from all failures.
     *
     * @param resource - The {@link Resource} for which to retrieve templates.
     * @returns A promise that resolves to the {@link Templates} associated with the resource or ProblemDetails if there's an error.
     */
    public async getAllTemplates(resource: Resource): Promise<Templates | ProblemDetails> {
        const formResponses = await this.getAllForms(resource);

        const failedResponses = formResponses.filter(response => !response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body);
        if (failedResponses.length !== 0) {
            // If we have multiple failed responses, create a combined problem details
            if (failedResponses.length > 1) {
                const failureDetails = failedResponses.map(response => {
                    if (ProblemDetails.isProblemDetails(response.body)) {
                        return response.body.detail || `Problem details from ${response.url}`;
                    }
                    return `Error ${response.status} (${response.statusText}) from ${response.url}`;
                }).join('; ');

                // Check if all failed responses have the same status code
                const allStatuses = failedResponses.map(response => {
                    if (ProblemDetails.isProblemDetails(response.body)) {
                        return response.body.status;
                    }
                    return response.status;
                });

                // Use the common status code if all are the same, otherwise use 500
                const uniqueStatuses = new Set(allStatuses);
                const status = uniqueStatuses.size === 1 ? allStatuses[0] : 500;

                const problemDetailsDto = {
                    _links: { self: [{ href: resource.findLink('self')?.href || "" }] },
                    detail: `Multiple errors occurred while loading form responses: ${failureDetails}`,
                    status: status,
                    title: "Multiple Form Loading Errors",
                    type: "https://tools.ietf.org/html/rfc7231#section-6.5"
                };

                return new ProblemDetails(problemDetailsDto);
            }

            // Single failure case
            const firstFailedResponse = failedResponses[0];
            if (ProblemDetails.isProblemDetails(firstFailedResponse.body)) {
                return firstFailedResponse.body;
            }

            // If we don't have a ProblemDetails object but the response failed,
            // create a new ProblemDetails object with the error information
            const problemDetailsDto = {
                _links: { self: [{ href: firstFailedResponse.url || "" }] },
                detail: `Error while loading the response from ${firstFailedResponse.url}`,
                status: firstFailedResponse.status,
                title: firstFailedResponse.statusText,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.1"
            };

            return new ProblemDetails(problemDetailsDto);
        }

        const formTemplates = Object.assign({}, ...formResponses.map(response => (response.body as FormsResource)._templates)) as Templates;

        return formTemplates;
    }

    /**
     * Fetches a form resource from the specified URL.
     *
     * @param {string} url - The URL of the form resource to fetch.
     * @returns A promise that resolves to an HttpResponse containing either a FormsResource or a ProblemDetails object.
     */
    public async getForm(url: string): Promise<HttpResponse<FormsResource | ProblemDetails>> {
        const header = RestWorldClient.createHeadersWithoutBody(this._options.Version, 'application/prs.hal-forms+json');
        return this._halClient.get(url, ResourceFactory.createFormResource, header);
    }

    /**
     * Retrieves a specific link from the home resource based on the provided relation type, name, and curie.
     *
     * @param rel - The relation type of the link to retrieve.
     * @param name - (Optional) The name of the link to retrieve. If not provided, the first link with the specified relation type is returned.
     * @param curie - (Optional) The CURIE (Compact URI) to use for the relation type.
     * @returns The {@link Link} that matches the specified criteria.
     * @throws {Error} If no link matches the specified criteria.
     */
    public getLinkFromHome(rel: string, name?: string, curie?: string): Link {
        const links = this.getLinksFromHome(rel, curie);

        const link = name ? links.find(l => l.name === name) : links[0];

        if (!link)
            throw new Error(`The home resource does not have a link with the rel '${this.getFullRel(rel, curie)}' and the name '${name}'.`);

        return link;
    }

    /**
     * Retrieves links from the home resource based on the provided relation type.
     *
     * @param rel - The relation type to look for in the home resource.
     * @param curie - An optional CURIE (Compact URI) to expand the relation type.
     * @returns An array of {@link Link} objects that match the specified relation type.
     * @throws Will throw an error if the home resource is not set or if no links are found for the specified relation type.
     */
    public getLinksFromHome(rel: string, curie?: string): Link[] {
        if (!this._homeResource)
            throw new Error('Home resource is not set. Call ensureHomeResourceIsSet() first.');

        const fullRel = this.getFullRel(rel, curie);
        const links = this._homeResource._links[fullRel];
        if (!links || links.length === 0)
            throw Error(`The home resource does not have a link with the rel '${fullRel}'.`);

        return links;
    }

    /**
     * Retrieves a list of resources from the REST API.
     *
     * @template TListDto - The type of the resource DTO.
     * @param {string} rel - The relation name to identify the link.
     * @param {Object} parameters - The query parameters to include in the request.
     * @param {HttpHeaders} [headers] - Optional HTTP headers to include in the request.
     * @param {string} [curie] - Optional CURIE to resolve the link.
     * @returns {Promise<HttpResponse<PagedListResource<TListDto> | ProblemDetails>>} - A promise that resolves to an HTTP response containing either a paged list of resources or problem details.
     */
    public async getList<TListDto extends ResourceDto>(rel: string, parameters: {}, headers?: HttpHeaders, curie?: string): Promise<HttpResponse<PagedListResource<TListDto> | ProblemDetails>> {
        const link = this.getLinkFromHome(rel, LinkNames.getList, curie);
        const uri = link.href

        const response = await this.getListByUri<TListDto>(uri, parameters, headers);

        return response;
    }

    /**
     * Fetches a list as a CSV file from the RESTworld API.
     *
     * @param rel - The relation name to identify the link.
     * @param parameters - The parameters to include in the request.
     * @param headers - Optional HTTP headers to include in the request.
     * @param curie - Optional CURIE (Compact URI) to resolve the link.
     * @returns A promise that resolves to an HttpResponse containing a Blob of the CSV data, or undefined if the request fails.
     */
    public async getListAsCsv(rel: string, parameters: {}, headers?: HttpHeaders, curie?: string): Promise<HttpResponse<Blob> | undefined> {
        const link = this.getLinkFromHome(rel, LinkNames.getList, curie);
        const uri = link.href;

        const response = await this.getListByUriAsCsv(uri, parameters, headers);

        return response;
    }

    /**
     * Fetches a list of resources from the specified URI.
     *
     * @template TListDto - The type of the resource DTO.
     * @param {string} uri - The URI to fetch the list from.
     * @param {Object} parameters - The parameters to fill the URI template.
     * @param {HttpHeaders} [headers] - Optional HTTP headers to include in the request.
     * @returns {Promise<HttpResponse<PagedListResource<TListDto> | ProblemDetails>>} - A promise that resolves to an HTTP response containing either a paged list of resources or problem details.
     */
    public async getListByUri<TListDto extends ResourceDto>(uri: string, parameters: {}, headers?: HttpHeaders): Promise<HttpResponse<PagedListResource<TListDto> | ProblemDetails>> {
        const link = new Link();
        link.href = uri;
        const filledUri = link.fillTemplate(parameters);
        const defaultHeaders = RestWorldClient.createHeadersWithoutBody(this._options.Version);
        const combinedHeaders = RestWorldClient.combineHeaders(headers, defaultHeaders, false);

        const response = await this.halClient.get<PagedListResourceDto<TListDto>, PagedListResource<TListDto>>(filledUri, ResourceFactory.createPagedListResource<TListDto, ResourceDto>, combinedHeaders);

        return response;
    }

    /**
     * Fetches a list from the given URI and returns it as a CSV file.
     *
     * @param uri - The URI to fetch the list from.
     * @param parameters - The parameters to fill the URI template.
     * @param headers - Optional HTTP headers to include in the request.
     * @returns A promise that resolves to an HTTP response containing the CSV file as a Blob, or undefined if the request fails.
     */
    public async getListByUriAsCsv(uri: string, parameters: {}, headers?: HttpHeaders): Promise<HttpResponse<Blob> | undefined> {
        const link = new Link();
        link.href = uri;
        const filledUri = link.fillTemplate(parameters);
        const defaultHeaders = RestWorldClient.createHeadersWithoutBody(this._options.Version, 'text/csv');
        const combinedHeaders = RestWorldClient.combineHeaders(headers, defaultHeaders, false);

        const response = await lastValueFrom(this.halClient.httpClient.get(filledUri, { headers: combinedHeaders, responseType: 'blob', observe: 'response' }));

        return response;
    }

    /**
     * Retrieves a single resource or problem details based on the provided relationship and ID.
     *
     * @param rel - The relationship name to identify the resource.
     * @param id - The ID of the resource to retrieve. Must be an integer.
     * @param headers - Optional HTTP headers to include in the request.
     * @param curie - Optional CURIE (Compact URI) to resolve the relationship.
     * @returns A promise that resolves to an HTTP response containing either the resource or problem details.
     * @throws An error if the provided ID is not an integer.
     */
    public async getSingle(rel: string, id: number, headers?: HttpHeaders, curie?: string): Promise<HttpResponse<Resource | ProblemDetails>> {
        if (!Number.isInteger(id))
            throw new Error('When supplying a rel, an ID must be supplied too.');

        const link = this.getLinkFromHome(rel, LinkNames.get, curie);
        const uri = link.fillTemplate({ id: id.toString() });

        const response = this.getSingleByUri(uri, headers);

        return response;
    }

    /**
     * Fetches a single resource by its URI.
     *
     * @template TState - The type of the state object.
     * @param {string} uri - The URI of the resource to fetch.
     * @param {HttpHeaders} [headers] - Optional HTTP headers to include in the request.
     * @returns {Promise<HttpResponse<Resource | ProblemDetails>>} - A promise that resolves to the HTTP response containing the resource or problem details.
     */
    public async getSingleByUri<TState>(uri: string, headers?: HttpHeaders): Promise<HttpResponse<Resource | ProblemDetails>> {
        const defaultHeaders = RestWorldClient.createHeadersWithoutBody(this._options.Version);
        const combinedHeaders = RestWorldClient.combineHeaders(headers, defaultHeaders, false);
        const response = await this.halClient.get<TState & ResourceDto, Resource & TState>(uri, ResourceFactory.createResource, combinedHeaders);

        return response;
    }

    /**
     * Sends a POST request to the specified URI with the given parameters and body, expecting a CSV response.
     *
     * @param uri - The URI to send the POST request to.
     * @param parameters - The parameters to fill the URI template.
     * @param body - The body of the POST request (optional).
     * @param headers - Additional headers to include in the request (optional).
     * @returns A promise that resolves to an HttpResponse containing a Blob of the CSV data, or undefined if the request fails.
     */
    public async postListByUriAsCsv(uri: string, parameters: {}, body?: any, headers?: HttpHeaders): Promise<HttpResponse<Blob> | undefined> {
        const link = new Link();
        link.href = uri;
        const filledUri = link.fillTemplate(parameters);
        const defaultHeaders = RestWorldClient.createHeadersWithBody(this._options.Version, 'text/csv');
        const combinedHeaders = RestWorldClient.combineHeaders(headers, defaultHeaders, false);

        const response = await lastValueFrom(this.halClient.httpClient.post(filledUri, body, { headers: combinedHeaders, responseType: 'blob', observe: 'response' }));

        return response;
    }

    /**
     * Saves the given resource using the appropriate HTTP method (PUT or POST) based on the name of the save link.
     *
     * @template TState - The type of the state to be saved.
     * @param {Resource & TState} resource - The resource to be saved, which must contain a save link.
     * @returns {Promise<HttpResponse<Resource & TState | ProblemDetails>>} - A promise that resolves to the HTTP response,
     * which can be either the saved resource or a ProblemDetails object in case of an error.
     * @throws {Error} - Throws an error if the resource does not have a save link or if the save link does not have a name.
     */
    public async save<TState>(resource: Resource & TState): Promise<HttpResponse<Resource & TState | ProblemDetails>> {
        const saveLink = resource.findLink('save');
        if (!saveLink) {
            const selfLink = resource.findLink('self');
            const resourceInfo = selfLink ? `Resource with self link: ${selfLink.href}` : 'Resource (no self link)';
            throw new Error(`The resource does not have a save link. ${resourceInfo}`);
        }
        if (!saveLink.name) {
            throw new Error(`The save link with href '${saveLink.href}' does not have a save name.`);
        }

        const uri = saveLink.href;
        const method = saveLink.name.toLowerCase();
        const header = RestWorldClient.createHeadersWithBody(this._options.Version);

        let response;
        switch (method) {
            case 'put':
                response = await this.halClient.put<TState, Resource & TState>(uri, resource, ResourceFactory.createResource<TState>, header);
                break;
            case 'post':
                response = await this.halClient.post<TState, Resource & TState>(uri, resource, ResourceFactory.createResource<TState>, header);
                break;
            default:
                throw new Error(`'${method}' is not allowed as link name for the save link. Only 'POST' and 'PUT' are allowed.`);
        }

        return response;
    }

    /**
     * Submits the form values to the specified template target URI using the appropriate HTTP method.
     *
     * @param template - The template containing the target URI and HTTP method.
     * @param formValues - The form values to be submitted.
     * @returns A promise that resolves to an {@link HttpResponse} containing either a {@link FormsResource} or {@link ProblemDetails}.
     */
    public async submit(template: Template, formValues: {}): Promise<HttpResponse<FormsResource | ProblemDetails>> {
        const uri = template.target || '';
        const method = template.method?.toLowerCase();
        const header = RestWorldClient.createHeadersWithBody(this._options.Version, 'application/prs.hal-forms+json');

        let response;
        switch (method) {
            case 'put':
                response = await this.halClient.put<void, FormsResource>(uri, formValues, ResourceFactory.createFormResource, header);
                break;
            case 'post':
                response = await this.halClient.post<void, FormsResource>(uri, formValues, ResourceFactory.createFormResource, header);
                break;
            default:
                response = await this.halClient.get<void, FormsResource>(uri, ResourceFactory.createFormResource, header);
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
        const header = RestWorldClient.createHeadersWithoutBody(this._options.Version);
        const response = await this.halClient.get(this._options.BaseUrl, ResourceFactory.createResource, header);
        return response;
    }

    private setDefaultCurie(): void {
        const curies = this._homeResource?._links?.curies;
        if (!curies || curies.length === 0 || !curies[0])
            this._defaultCurie = undefined;
        else
            this._defaultCurie = curies[0].name;
    }

    /**
     * Retrieves all forms associated with a resource fetched by its URI.
     *
     * This method first fetches the resource from the given URI and then retrieves all forms for that resource.
     * If the resource response contains a problem detail, it returns an array with a single HTTP response containing that problem detail.
     *
     * @param uri - The URI of the resource to fetch.
     * @param headers - Optional HTTP headers to include in the request.
     * @returns A promise that resolves to an array of HTTP responses, each containing either a `FormsResource` or `ProblemDetails`.
     */
    public async getAllFormsByUri(uri: string, headers?: HttpHeaders): Promise<HttpResponse<FormsResource | ProblemDetails>[]> {
        const resourceResponse = await this.getSingleByUri(uri, headers);

        if (!resourceResponse.ok || ProblemDetails.isProblemDetails(resourceResponse.body)) {
            // Return the problem details as is, wrapped in an array
            if (ProblemDetails.isProblemDetails(resourceResponse.body)) {
                return [resourceResponse as HttpResponse<ProblemDetails>];
            }

            // If no body or problem details, create a proper ProblemDetails object
            const problemDetailsDto = {
                _links: { self: [{ href: uri }] },
                detail: `Error while loading the resource from ${uri}`,
                status: resourceResponse.status,
                title: resourceResponse.statusText,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5"
            };

            const problemDetails = new ProblemDetails(problemDetailsDto);

            // Modify the response body to include our problem details
            const errorResponse = { ...resourceResponse, body: problemDetails } as HttpResponse<ProblemDetails>;
            return [errorResponse];
        }

        if (!resourceResponse.body) {
            // Handle empty response with a proper ProblemDetails object
            const problemDetailsDto = {
                _links: { self: [{ href: uri }] },
                detail: `Resource from ${uri} was empty`,
                status: 404,
                title: "Resource Not Found",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            };

            const problemDetails = new ProblemDetails(problemDetailsDto);

            const errorResponse = { ...resourceResponse, body: problemDetails } as HttpResponse<ProblemDetails>;
            return [errorResponse];
        }

        return this.getAllForms(resourceResponse.body);
    }

    /**
     * Retrieves all templates associated with a resource fetched by its URI.
     *
     * This method first fetches the resource from the given URI and then retrieves all templates for that resource.
     * It combines functionality from {@link getSingleByUri} and {@link getAllTemplates}.
     * If the resource response contains a problem detail or if any form responses fail,
     * the method returns the problem detail rather than throwing an error.
     *
     * @param uri - The URI of the resource to fetch.
     * @param headers - Optional HTTP headers to include in the request.
     * @returns A promise that resolves to the {@link Templates} associated with the resource or a ProblemDetails object if there was an error.
     */
    public async getAllTemplatesByUri(uri: string, headers?: HttpHeaders): Promise<Templates | ProblemDetails> {
        const resourceResponse = await this.getSingleByUri(uri, headers);

        if (!resourceResponse.ok || ProblemDetails.isProblemDetails(resourceResponse.body)) {
            if (ProblemDetails.isProblemDetails(resourceResponse.body)) {
                return resourceResponse.body;
            }

            // If no problem details in body but response not OK, create a proper ProblemDetails object
            const problemDetailsDto = {
                _links: { self: [{ href: uri }] },
                detail: `Error while loading the resource from ${uri}`,
                status: resourceResponse.status,
                title: resourceResponse.statusText,
                type: "https://tools.ietf.org/html/rfc7231#section-6.5"
            };
            return new ProblemDetails(problemDetailsDto);
        }

        if (!resourceResponse.body) {
            // Handle empty response with a proper ProblemDetails object
            const problemDetailsDto = {
                _links: { self: [{ href: uri }] },
                detail: `Resource from ${uri} was empty`,
                status: 404,
                title: "Resource Not Found",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            };
            return new ProblemDetails(problemDetailsDto);
        }

        return this.getAllTemplates(resourceResponse.body);
    }

    /**
     * Retrieves a specific template by title from a resource fetched by its URI.
     *
     * This method first fetches all templates from the resource at the given URI and then
     * returns the template with the specified title. If the resource cannot be fetched,
     * if it contains a problem detail, or if any form responses fail, this method returns
     * the problem detail rather than throwing an error.
     *
     * @param uri - The URI of the resource to fetch.
     * @param templateTitle - The title of the template to retrieve.
     * @param headers - Optional HTTP headers to include in the request.
     * @returns A promise that resolves to the {@link Template} with the specified title or a ProblemDetails object if there was an error.
     */
    public async getTemplateByUri(uri: string, templateTitle: string, headers?: HttpHeaders): Promise<Template | ProblemDetails> {
        const templatesOrProblemDetails = await this.getAllTemplatesByUri(uri, headers);

        // If we got ProblemDetails instead of Templates, return it
        if (ProblemDetails.isProblemDetails(templatesOrProblemDetails)) {
            return templatesOrProblemDetails;
        }

        const templates = templatesOrProblemDetails as Templates;
        const template = templates[templateTitle];

        if (template === undefined) {
            const templateTitles = Object.keys(templates);
            // Instead of throwing an error, return a properly constructed ProblemDetails object
            const problemDetailsDto = {
                _links: { self: [{ href: uri }] },
                detail: `No template with title '${templateTitle}' found. Available templates are: ${templateTitles.join(', ')}`,
                status: 404,
                title: "Template Not Found",
                type: "https://tools.ietf.org/html/rfc7231#section-6.5.4"
            };
            return new ProblemDetails(problemDetailsDto);
        }

        return template;
    }
}
