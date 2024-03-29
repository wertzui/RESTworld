import { Component, Input, OnInit } from '@angular/core';
import { RestWorldClientCollection } from '../ngx-restworld-client/services/restworld-client-collection';
import { ConfirmationService, MessageService } from 'primeng/api';
import { AvatarGenerator } from '../ngx-restworld-client/services/avatar-generator';
import { ODataParameters } from '../ngx-restworld-client/models/o-data';
import { RestWorldClient } from '../ngx-restworld-client/services/restworld-client';
import { PostListDto } from './models';
import { FormService, PagedListResource, ProblemDetails, Property, ResourceOfDto, Template } from '@wertzui/ngx-hal-client';
import { FormArray, FormGroup } from '@angular/forms';
import { debounce } from '../ngx-restworld-client/util/debounce';

@Component({
  selector: 'app-posts-for-blog',
  templateUrl: './posts-for-blog.component.html',
  styleUrls: ['./posts-for-blog.component.css']
})
export class PostsForBlogComponent implements OnInit {
  @Input()
  public apiName?: string;

  @Input()
  public rel?: string;

  public load = debounce(this.loadInternal, 100);
  public isLoading: boolean;
  public searchTemplate: Template;
  public resource: PagedListResource<PostListDto>;
  public totalRecords: number;
  public editTemplate: Template;
  public rows: ResourceOfDto<PostListDto>[] = [];
  public formArray: FormArray = new FormArray([]);
  public formGroup: FormGroup = new FormGroup({ blablup: this.formArray });
  public selection: ResourceOfDto<PostListDto>[] = [];

  constructor(
    private readonly _clients: RestWorldClientCollection,
    private readonly _messageService: MessageService,
    public readonly avatarGenerator: AvatarGenerator,
    private readonly _formService: FormService) {
  }
  public async ngOnInit(): Promise<void> {
    return this.loadInternal({ $top: 10 });
  }

  public async loadInternal(parameters: ODataParameters): Promise<void> {
    if (!this.apiName || !this.rel)
      return;

    this.isLoading = true;

    const response = await this.getClient().getList<PostListDto>(this.rel, parameters);
    if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resources from the API.', data: response });
    }
    else if (response.body) {
      try {
        const templates = await this.getClient().getAllTemplates(response.body);
        this.searchTemplate = templates["Search"];
        if (this.searchTemplate === undefined)
          throw new Error("No search template found in the API response.");
        this.editTemplate = templates["Edit"];
        if (this.editTemplate === undefined)
          throw new Error("No edit template found in the API response.");
      }
      catch (e: unknown) {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resources from the API. ' + e, data: e });
      }

      this.resource = response.body;
      const rowsPerPage = parameters.$top ?? response.body._embedded.items.length;
      const totalPages = response.body.totalPages ?? 1;
      this.totalRecords = totalPages * rowsPerPage;
      this.rows = response.body._embedded.items;
      // const editGroup = this._formService.createFormGroupFromTemplate(this.editTemplate);
      // this.formArray = new FormArray(this.rows.map(r => {
      //   const formGroup = this._formService.createFormGroupFromTemplate(this.editTemplate);
      //   formGroup.patchValue(r);
      //   return formGroup;
      // }));
    }

    this.isLoading = false;
  }

  public generateRowClasses(row: ResourceOfDto<PostListDto>, rowIndex: number) {
    return rowIndex % 2 === 0 ? 'row-even' : 'row-odd';
  }

  public generateCellClasses(row: ResourceOfDto<PostListDto>, property: Property, rowIndex: number, columnIndex: number) {
    if (row[property.name] === "Post number 1")
      return 'special';
    return columnIndex % 2 === 0 ? 'cell-even' : 'cell-odd';
  }

  private getClient(): RestWorldClient {
    if (!this.apiName)
      throw new Error('Cannot get a client, because the apiName is not set.');

    return this._clients.getClient(this.apiName);
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
