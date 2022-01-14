import { Component, Input } from '@angular/core';
import { PropertyType, Resource, Template, Templates, FormsResource, Property } from '@wertzui/ngx-hal-client';
import { RESTworldClient } from '../../services/restworld-client';
import { RESTworldClientCollection } from '../../services/restworld-client-collection';
import { AbstractControl, FormControl, FormGroup } from '@angular/forms';
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
  public get PropertyType() {
    return PropertyType;
  }
  public get templates() {
    return this._templates;
  }
  private _templates: Templates = {};

  public get isLoadingForTheFirstTime() {
    return Object.keys(this.templates).length === 0 && this.isLoading;
  }

  public get formTabs() {
    return this._formTabs;
  }
  private _formTabs: { [name: string]: FormGroup } = {};

  @Input()
  public set apiName(value: string | undefined) {
    this._apiName = value;
    this.load();
  }
  public get apiName(): string | undefined {
    return this._apiName;
  }
  private _apiName?: string;
  //@Input()
  //public set rel(value: string | undefined) {
  //  this._rel = value;
  //}
  //public get rel(): string | undefined {
  //  return this._rel;
  //}
  //private _rel?: string;
  @Input()
  public set uri(value: string | undefined) {
    this._uri = value;
    this.load();
  }
  public get uri(): string | undefined {
    return this._uri;
  }
  private _uri?: string;

  public get resource() {
    return this._resource;
  }
  private _resource?: Resource;
  public isLoading = false;
  public get canSave() {
    const length = this.resource?._links["save"]?.length;
    return length !== undefined && length > 0;
  }
  public get canDelete() {
    const length = this.resource?._links["delete"]?.length;
    return length !== undefined && length > 0;
  }
  public canSubmit(templateName: string) {
    const form = this.formTabs[templateName];
    return form && form.valid;
  }

  @ContentChild('extraTabs', { static: false })
  extraTabsRef?: TemplateRef<unknown>;

  @ContentChild('buttons', { static: false })
  buttonsRef?: TemplateRef<unknown>;

  @ContentChild('inputOptionsSingle', { static: false })
  inputOptionsSingleRef?: TemplateRef<unknown>;

  @ContentChild('inputOptionsMultiple', { static: false })
  inputOptionsMultipleRef?: TemplateRef<unknown>;

  @ContentChild('inputOptions', { static: false })
  inputOptionsRef?: TemplateRef<unknown>;

  @ContentChild('inputHidden', { static: false })
  inputHiddenRef?: TemplateRef<unknown>;

  @ContentChild('inputText', { static: false })
  inputTextRef?: TemplateRef<unknown>;

  @ContentChild('inputTextarea', { static: false })
  inputTextareaRef?: TemplateRef<unknown>;

  @ContentChild('inputSearch', { static: false })
  inputSearchRef?: TemplateRef<unknown>;

  @ContentChild('inputTel', { static: false })
  inputTelRef?: TemplateRef<unknown>;

  @ContentChild('inputUrl', { static: false })
  inputUrlRef?: TemplateRef<unknown>;

  @ContentChild('inputEmail', { static: false })
  inputEmailRef?: TemplateRef<unknown>;

  @ContentChild('inputPassword', { static: false })
  inputPasswordRef?: TemplateRef<unknown>;

  @ContentChild('inputDate', { static: false })
  inputDateRef?: TemplateRef<unknown>;

  @ContentChild('inputMonth', { static: false })
  inputMonthRef?: TemplateRef<unknown>;

  @ContentChild('inputWeek', { static: false })
  inputWeekRef?: TemplateRef<unknown>;

  @ContentChild('inputTime', { static: false })
  inputTimeRef?: TemplateRef<unknown>;

  @ContentChild('inputDatetimeLocal', { static: false })
  inputDatetimeLocalRef?: TemplateRef<unknown>;

  @ContentChild('inputNumber', { static: false })
  inputNumberRef?: TemplateRef<unknown>;

  @ContentChild('inputRange', { static: false })
  inputRangeRef?: TemplateRef<unknown>;

  @ContentChild('inputColor', { static: false })
  inputColorRef?: TemplateRef<unknown>;

  @ContentChild('inputBool', { static: false })
  inputBoolRef?: TemplateRef<unknown>;

  @ContentChild('inputDatetimeOffset', { static: false })
  inputDatetimeOffsetRef?: TemplateRef<unknown>;

  @ContentChild('inputDuration', { static: false })
  inputDurationRef?: TemplateRef<unknown>;

  @ContentChild('inputImage', { static: false })
  inputImageRef?: TemplateRef<unknown>;

  @ContentChild('inputFile', { static: false })
  inputFileRef?: TemplateRef<unknown>;

  @ContentChild('inputDefault', { static: false })
  inputDefaultRef?: TemplateRef<unknown>;

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

  public getTooltip(resource: Resource, keysToExclude?: string[]): string {
    const tooltip = Object.entries(resource)
      .filter(([key]) => !(key.startsWith('_') || ['createdAt', 'createdBy', 'lastChangedAt', 'lastChangedBy', 'timestamp'].includes(key) || keysToExclude?.includes(key)))
      .reduce((prev, [key, value], index) => `${prev}${index === 0 ? '' : '\n'}${key}: ${RESTworldEditViewComponent.jsonStringifyWithElipsis(value)}`, '');

    return tooltip;
  }

  private static jsonStringifyWithElipsis(value: unknown) {
    const maxLength = 200;
    const end = 10;
    const start = maxLength - end - 2;
    const json = JSON.stringify(value);
    const shortened = json.length > maxLength ? json.substring(0, start) + '…' + json.substring(json.length - end) : json;

    return shortened;
  }

  private getClient(): RESTworldClient {
    if (!this.apiName)
      throw new Error('Cannot get a client, because the apiName is not set.');

    return this._clients.getClient(this.apiName);
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
              const formControl = path.reduce<AbstractControl>((control, pathElement) => (control instanceof FormGroup ? control.controls[pathElement] : control) || control, form);
              formControl.setErrors({ remote: errorsForKey });
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

  public showDeleteConfirmatioModal() {
    this._confirmationService.confirm({
      message: 'Do you really want to delete this resource?',
      header: 'Confirm delete',
      icon: 'far fa-trash-alt',
      accept: () => this.delete()
    });
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

  private async setInitialSelectedOptionsElementsForTemplates(templates: Templates) {
    return Promise.all(Object.values(templates)
      .map(template => this.setInitialSelectedOptionsElementsForTemplate(template)));
  }

  public imageChanged(formControl: FormControl, event: { files: File[] }): void {
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

  private async setInitialSelectedOptionsElementsForTemplate(template: Template) {
    return Promise.all(template.properties
      .filter(property => property?.options?.link?.href)
      .map(property => this.setInitialSelectedOptionsElementForProperty(property)));
  }

  private async setInitialSelectedOptionsElementForProperty(property: Property) {
    const options = property?.options;

    if (!options?.link?.href)
      return;

    const templatedUri = options.link.href;
    const filter = `${options.valueField} eq ${property.value}`;
    const response = await this.getClient().getListByUri(templatedUri, { $filter: filter, $top: 10 });
    if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
      const message = `An error occurred while getting the filtered items.`;
      this._messageService.add({ severity: 'error', summary: 'Error', detail: message, data: response });
      return;
    }

    const items = response.body._embedded.items;
    options.inline = items;
  }

  private async getAllTemplates(resource: Resource): Promise<Templates> {
    const formResponses = await this.getClient().getAllForms(resource);

    const failedResponses = formResponses.filter(response => !response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body);
    if (failedResponses.length !== 0) {
      for (const response of failedResponses) {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resource from the API.', data: response });
      }
      return Promise.resolve({});
    }

    const formTemplates = Object.assign({}, ...formResponses.map(response => (response.body as FormsResource)._templates)) as Templates;

    await this.setInitialSelectedOptionsElementsForTemplates(formTemplates);

    return formTemplates;
  }
}
