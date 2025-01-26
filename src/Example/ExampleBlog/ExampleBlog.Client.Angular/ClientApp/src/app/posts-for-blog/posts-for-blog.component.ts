import { Component, computed, input, model, resource, signal } from '@angular/core';
import { RestWorldClientCollection } from '../ngx-restworld-client/services/restworld-client-collection';
import { MessageService } from 'primeng/api';
import { AvatarGenerator } from '../ngx-restworld-client/services/avatar-generator';
import { ODataParameters } from '../ngx-restworld-client/models/o-data';
import { PostListDto } from './models';
import { PagedListResource, Property, ResourceOfDto, Template } from '@wertzui/ngx-hal-client';
import { FormArray, FormGroup, ReactiveFormsModule } from '@angular/forms';
import { RestWorldTableComponent } from "../ngx-restworld-client/components/restworld-table/restworld-table.component";
import { JsonPipe } from "@angular/common";
import { ProblemService } from "../ngx-restworld-client/services/problem.service";

@Component({
    selector: 'app-posts-for-blog',
    templateUrl: './posts-for-blog.component.html',
    styleUrls: ['./posts-for-blog.component.css'],
    standalone: true,
    imports: [RestWorldTableComponent, JsonPipe, ReactiveFormsModule]
})
export class PostsForBlogComponent {
    public readonly apiName = input.required<string>();
    public readonly formArray = new FormArray([]);
    public readonly formGroup = new FormGroup({ posts: this.formArray });
    public readonly isLoading = computed(() => this.listResource.isLoading() || this.templates.isLoading());
    // public readonly load = debounce(this.loadInternal, 100);
    public readonly rel = input.required<string>();
    public readonly items = computed(() => this.listResource.value()?._embedded.items || []);
    // public readonly searchTemplate = signal<Template>(new Template({ properties: [] }));
    public readonly selection = model<ResourceOfDto<PostListDto>[]>([]);

    public readonly totalRecords = computed(() => {
        const top = this.oDataParameters().$top;
        const listResource = this.listResource.value();
        const rowsPerPage = top ?? listResource?._embedded.items.length ?? 0;
        const totalPages = listResource?.totalPages ?? 1;
        const totalRecords = totalPages * rowsPerPage;
        return totalRecords;
    });
    public readonly oDataParameters = signal<ODataParameters>({ $top: 10 });

    private readonly listResource = resource({
        request: () => ({ oDataParameters: this.oDataParameters(), rel: this.rel() }),
        loader: async ({ request }) => {
            const response = await this._client().getList<PostListDto>(request.rel, request.oDataParameters);
            if (this._problemService.checkResponseAndDisplayErrors(response, undefined, "Error while loading the resources from the API.", "Error")) {
                return response.body;
            }

            return PostsForBlogComponent._emptylistResource;
        },
    });

    public readonly searchTemplate = computed(() => this.templates.value()?.search ?? PostsForBlogComponent._emptyTemplate);
    public readonly editTemplate = computed(() => this.templates.value()?.edit ?? PostsForBlogComponent._emptyTemplate);
    private readonly templates = resource({
        request: () => ({ resource: this.listResource.value() }),
        loader: async ({ request }) => {
            if (request.resource === undefined)
                return { search: PostsForBlogComponent._emptyTemplate, edit: PostsForBlogComponent._emptyTemplate };

            try {
                const templates = await this._client().getAllTemplates(request.resource);

                const searchTemplate = templates["Search"];
                if (searchTemplate === undefined)
                    throw new Error("No search template found in the API response.");

                const editTemplate = templates["Edit"];
                if (editTemplate === undefined)
                    throw new Error("No edit template found in the API response.");

                return { search: searchTemplate, edit: editTemplate };
            }
            catch (e: unknown) {
                this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resources from the API. ' + e, data: e });
                return { search: PostsForBlogComponent._emptyTemplate, edit: PostsForBlogComponent._emptyTemplate };
            }
        },
    });

    private readonly _client = computed(() => this._clients.getClient(this.apiName()));

    private static readonly _emptylistResource = new PagedListResource({ _embedded: { items: [] }, _links: { self: [] } });

    private static readonly _emptyTemplate = new Template({ properties: [] });

    constructor(
        private readonly _clients: RestWorldClientCollection,
        private readonly _messageService: MessageService,
        public readonly avatarGenerator: AvatarGenerator,
        private readonly _problemService: ProblemService) {
        this.listResource.set(PostsForBlogComponent._emptylistResource);
        this.templates.set({ search: PostsForBlogComponent._emptyTemplate, edit: PostsForBlogComponent._emptyTemplate });
    }

    public generateCellClasses(row: ResourceOfDto<PostListDto>, property: Property, rowIndex: number, columnIndex: number) {
        if (row[property.name] === "Post number 1")
            return 'special';
        return columnIndex % 2 === 0 ? 'cell-even' : 'cell-odd';
    }

    public generateRowClasses(row: ResourceOfDto<PostListDto>, rowIndex: number) {
        return rowIndex % 2 === 0 ? 'row-even' : 'row-odd';
    }

    public onRowSelect(row: ResourceOfDto<PostListDto>) {
        this._messageService.add({
            severity: 'info',
            summary: 'Row selected',
            detail: row.headline
        });
    }

    public onRowUnselect(row: ResourceOfDto<PostListDto>) {
        this._messageService.add({
            severity: 'info',
            summary: 'Row unselected',
            detail: row.headline
        });
    }
}
