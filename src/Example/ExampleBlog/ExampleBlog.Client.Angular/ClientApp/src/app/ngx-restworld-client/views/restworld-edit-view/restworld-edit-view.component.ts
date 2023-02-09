import { Component, Input } from '@angular/core';
import { PropertyType, Resource, Template, Templates, FormsResource, Property, ListResource } from '@wertzui/ngx-hal-client';
import { RESTworldClient } from '../../services/restworld-client';
import { RESTworldClientCollection } from '../../services/restworld-client-collection';
import { AbstractControl, FormControl, FormGroup, UntypedFormArray, UntypedFormControl, UntypedFormGroup, Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { ProblemDetails } from '../../models/problem-details';
import { ContentChild } from '@angular/core';
import { TemplateRef } from '@angular/core';
import { ValdemortConfig } from 'ngx-valdemort';
import { FormService } from '../../services/form.service';

@Component({
  selector: 'rw-edit',
  templateUrl: './restworld-edit-view.component.html',
  styleUrls: ['./restworld-edit-view.component.css']
})
export class RESTworldEditViewComponent {
  @ContentChild('buttons', { static: false })
  buttonsRef?: TemplateRef<unknown>;
  @ContentChild('extraTabs', { static: false })
  extraTabsRef?: TemplateRef<unknown>;
  public idNavigationForm = new FormGroup < {
    id: FormControl<number | null>
  }>({
    id: new FormControl(null, Validators.compose([Validators.min(1), Validators.max(Number.MAX_SAFE_INTEGER)]))
  });
  @ContentChild('inputBool', { static: false })
  inputBoolRef?: TemplateRef<unknown>;
  @ContentChild('inputColor', { static: false })
  inputColorRef?: TemplateRef<unknown>;
  @ContentChild('inputDate', { static: false })
  inputDateRef?: TemplateRef<unknown>;
  @ContentChild('inputDatetimeLocal', { static: false })
  inputDatetimeLocalRef?: TemplateRef<unknown>;
  @ContentChild('inputDatetimeOffset', { static: false })
  inputDatetimeOffsetRef?: TemplateRef<unknown>;
  @ContentChild('inputDefault', { static: false })
  inputDefaultRef?: TemplateRef<unknown>;
  @ContentChild('inputDuration', { static: false })
  inputDurationRef?: TemplateRef<unknown>;
  @ContentChild('inputEmail', { static: false })
  inputEmailRef?: TemplateRef<unknown>;
  @ContentChild('inputFile', { static: false })
  inputFileRef?: TemplateRef<unknown>;
  @ContentChild('inputHidden', { static: false })
  inputHiddenRef?: TemplateRef<unknown>;
  @ContentChild('inputImage', { static: false })
  inputImageRef?: TemplateRef<unknown>;
  @ContentChild('inputMonth', { static: false })
  inputMonthRef?: TemplateRef<unknown>;
  @ContentChild('inputNumber', { static: false })
  inputNumberRef?: TemplateRef<unknown>;
  @ContentChild('inputOptionsMultiple', { static: false })
  inputOptionsMultipleRef?: TemplateRef<unknown>;
  @ContentChild('inputOptions', { static: false })
  inputOptionsRef?: TemplateRef<unknown>;
  @ContentChild('inputOptionsSingle', { static: false })
  inputOptionsSingleRef?: TemplateRef<unknown>;
  @ContentChild('inputPassword', { static: false })
  inputPasswordRef?: TemplateRef<unknown>;
  @ContentChild('inputRange', { static: false })
  inputRangeRef?: TemplateRef<unknown>;
  @ContentChild('inputSearch', { static: false })
  inputSearchRef?: TemplateRef<unknown>;
  @ContentChild('inputTel', { static: false })
  inputTelRef?: TemplateRef<unknown>;
  @ContentChild('inputText', { static: false })
  inputTextRef?: TemplateRef<unknown>;
  @ContentChild('inputTextarea', { static: false })
  inputTextareaRef?: TemplateRef<unknown>;
  @ContentChild('inputTime', { static: false })
  inputTimeRef?: TemplateRef<unknown>;
  @ContentChild('inputUrl', { static: false })
  inputUrlRef?: TemplateRef<unknown>;
  @ContentChild('inputWeek', { static: false })
  inputWeekRef?: TemplateRef<unknown>;
  public isLoading = false;

  private _apiName?: string;
  private _formTabs: { [name: string]: UntypedFormGroup } = {};
  private _rel?: string;
  private _resource?: Resource;
  private _templates: Templates = {};
  private _uri?: string;

  constructor(
    private _clients: RESTworldClientCollection,
    private _confirmationService: ConfirmationService,
    private _messageService: MessageService,
    private _location: Location,
    private _router: Router,
    private _formService: FormService,
    valdemortConfig: ValdemortConfig) {
    valdemortConfig.errorClasses = 'p-error text-sm';
  }

  public get PropertyType() {
    return PropertyType;
  }

  public get apiName(): string | undefined {
    return this._apiName;
  }

  @Input()
  public set apiName(value: string | undefined) {
    this._apiName = value;
    this.load();
  }

  public get canDelete() {
    const length = this.resource?._links["delete"]?.length;
    return length !== undefined && length > 0;
  }

  public get canSave() {
    const length = this.resource?._links["save"]?.length;
    return length !== undefined && length > 0;
  }

  public get formTabs() {
    return this._formTabs;
  }

  public get isLoadingForTheFirstTime() {
    return Object.keys(this.templates).length === 0 && this.isLoading;
  }

  public get rel(): string | undefined {
    return this._rel;
  }

  @Input()
  public set rel(value: string | undefined) {
    this._rel = value;
  }

  public get resource() {
    return this._resource;
  }

  public get templates() {
    return this._templates;
  }

  public get uri(): string | undefined {
    return this._uri;
  }

  @Input()
  public set uri(value: string | undefined) {
    this._uri = value;
    this.load();
  }

  private static getSubControl(control: AbstractControl, pathElement: string): AbstractControl {
    if (control instanceof UntypedFormGroup)
      return control.controls[pathElement];
    if (control instanceof UntypedFormArray) {
      const index = Number.parseInt(pathElement);
      if (Number.isInteger(index))
        return control.controls[index];
    }
    return control;
  }

  private static jsonStringifyWithElipsis(value: unknown) {
    const maxLength = 200;
    const end = 10;
    const start = maxLength - end - 2;
    const json = JSON.stringify(value);
    const shortened = json.length > maxLength ? json.substring(0, start) + '…' + json.substring(json.length - end) : json;

    return shortened;
  }

  public canSubmit(templateName: string) {
    const form = this.formTabs[templateName];
    return form && form.valid;
  }

  public async delete(): Promise<void> {
    if (!this.apiName || !this.uri || !this.resource)
      return;

    Object.assign(this.resource, this.formTabs.value);

    await this.getClient().delete(this.resource);
    setTimeout(() =>
      this._messageService.add({ severity: 'success', summary: 'Deleted', detail: 'The resource has been deleted.' }),
      100);

    this._location.back();
  }

  public getTooltip(resource: Resource, keysToExclude?: string[]): string {
    const tooltip = Object.entries(resource)
      .filter(([key]) => !(key.startsWith('_') || ['createdAt', 'createdBy', 'lastChangedAt', 'lastChangedBy', 'timestamp'].includes(key) || keysToExclude?.includes(key)))
      .reduce((prev, [key, value], index) => `${prev}${index === 0 ? '' : '\n'}${key}: ${RESTworldEditViewComponent.jsonStringifyWithElipsis(value)}`, '');

    return tooltip;
  }

  public imageChanged(formControl: UntypedFormControl, event: { files: File[] }): void {
    const file = event.files[0];
    console.log(file);
    const reader = new FileReader();
    reader.onload = () => {
      const uri = reader.result;
      console.log(uri);
      formControl.setValue(uri);
    };
    reader.readAsDataURL(file);
  }

  public async load(): Promise<void> {
    if (!this.apiName || !this.uri)
      return;

    this.isLoading = true;

    const response = await this.getClient().getSingle(this.uri);
    if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resource from the API.', data: response });
    }
    else {
      this._resource = response.body;
      this._templates = await this.getAllTemplates(this._resource);
      this._formTabs = this._formService.createFormGroupsFromTemplates(this._templates);
    }

    this.isLoading = false;
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

    await this._router.navigate(['/edit', this.apiName, this.rel, resource._links.self[0].href]);

    this.idNavigationForm.reset();
  }

  public showDeleteConfirmatioModal() {
    this._confirmationService.confirm({
      message: 'Do you really want to delete this resource?',
      header: 'Confirm delete',
      icon: 'far fa-trash-alt',
      accept: () => this.delete()
    });
  }

  public async submit(templateName: string, template: Template, formValue: {}) {
    this.isLoading = true;

    try {
      const targetBeforeSave = template.target;
      const response = await this.getClient().submit(template, formValue);

      if (!response.ok) {
        let summary = 'Error';
        let detail = 'Error while saving the resource.';
        if (ProblemDetails.isProblemDetails(response.body)) {
          const problemDetails = response.body as ProblemDetails;
          summary = problemDetails.title || summary;
          detail = problemDetails.detail || detail;
          // display validation errors
          if (problemDetails['errors'] as {}) {
            const form = this.formTabs[templateName];
            for (const [key, errorsForKey] of Object.entries(problemDetails['errors'] as {})) {
              const path = key.split(/\.|\[/).map(e => e.replace("]", ""));
              // The path might start with a $, indicating the root.
              if (path.length > 0 && path[0] === '$')
                path.shift();
              const formControl = path.reduce<AbstractControl>(RESTworldEditViewComponent.getSubControl, form);
              formControl.setErrors({ ...formControl.errors, ...{ remote: errorsForKey } });
              formControl.markAsTouched();
            }
          }
        }

        this._messageService.add({ severity: 'error', summary: summary, detail: detail, data: response, life: 10000 });
      }
      else {
        const responseResource = (response.body as FormsResource);
        const targetAfterSave = responseResource._templates[templateName].target;

        setTimeout(() =>
          this._messageService.add({ severity: 'success', summary: 'Saved', detail: 'The resource has been saved.' }),
          100);

        if (targetBeforeSave !== targetAfterSave) {
          this._router.navigate(['/edit', this.apiName, responseResource._links.self[0].href]);
        }
      }
    }
    catch (e: unknown) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: `An unknown error occurred. ${JSON.stringify(e)}`, life: 10000 });
      console.log(e);
    }

    this.isLoading = false;
  }

  private async getAllTemplates(resource: Resource): Promise<Templates> {
    try {
      return this.getClient().getAllTemplates(resource);
    }
    catch (e: unknown) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the templates from the API. ' + e, data: e });
      return {};
    }
  }

  private getClient(): RESTworldClient {
    if (!this.apiName)
      throw new Error('Cannot get a client, because the apiName is not set.');

    return this._clients.getClient(this.apiName);
  }
}
