import { Component, Input } from '@angular/core';
import { PropertyType, Resource, Template, Templates, FormsResource, Property } from '@wertzui/ngx-hal-client';
import { RESTworldClient } from '../../services/restworld-client';
import * as _ from 'lodash';
import { RESTworldClientCollection } from '../../services/restworld-client-collection';
import { AbstractControl, FormControl, FormGroup,  Validators } from '@angular/forms';
import { ConfirmationService, MessageService } from 'primeng/api';
import { Location } from '@angular/common';
import { Router } from '@angular/router';
import { ProblemDetails } from '../../models/problem-details';
import { ContentChild } from '@angular/core';
import { TemplateRef } from '@angular/core';
import { ValdemortConfig } from 'ngx-valdemort';

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

  public get dateFormat(): string {
    return new Date(3333, 10, 22)
      .toLocaleDateString()
      .replace("22", "dd")
      .replace("11", "mm")
      .replace("3333", "yy")
      .replace("33", "y");
  }

  @ContentChild('extraTabs', { static: false })
  extraTabsRef?: TemplateRef<any>;

  @ContentChild('buttons', { static: false })
  buttonsRef?: TemplateRef<any>;

  @ContentChild('inputOptionsSingle', { static: false })
  inputOptionsSingleRef?: TemplateRef<any>;

  @ContentChild('inputOptionsMultiple', { static: false })
  inputOptionsMultipleRef?: TemplateRef<any>;

  @ContentChild('inputOptions', { static: false })
  inputOptionsRef?: TemplateRef<any>;

  @ContentChild('inputHidden', { static: false })
  inputHiddenRef?: TemplateRef<any>;

  @ContentChild('inputText', { static: false })
  inputTextRef?: TemplateRef<any>;

  @ContentChild('inputTextarea', { static: false })
  inputTextareaRef?: TemplateRef<any>;

  @ContentChild('inputSearch', { static: false })
  inputSearchRef?: TemplateRef<any>;

  @ContentChild('inputTel', { static: false })
  inputTelRef?: TemplateRef<any>;

  @ContentChild('inputUrl', { static: false })
  inputUrlRef?: TemplateRef<any>;

  @ContentChild('inputEmail', { static: false })
  inputEmailRef?: TemplateRef<any>;

  @ContentChild('inputPassword', { static: false })
  inputPasswordRef?: TemplateRef<any>;

  @ContentChild('inputDate', { static: false })
  inputDateRef?: TemplateRef<any>;

  @ContentChild('inputMonth', { static: false })
  inputMonthRef?: TemplateRef<any>;

  @ContentChild('inputWeek', { static: false })
  inputWeekRef?: TemplateRef<any>;

  @ContentChild('inputTime', { static: false })
  inputTimeRef?: TemplateRef<any>;

  @ContentChild('inputDatetimeLocal', { static: false })
  inputDatetimeLocalRef?: TemplateRef<any>;

  @ContentChild('inputNumber', { static: false })
  inputNumberRef?: TemplateRef<any>;

  @ContentChild('inputRange', { static: false })
  inputRangeRef?: TemplateRef<any>;

  @ContentChild('inputColor', { static: false })
  inputColorRef?: TemplateRef<any>;

  @ContentChild('inputBool', { static: false })
  inputBoolRef?: TemplateRef<any>;

  @ContentChild('inputDatetimeOffset', { static: false })
  inputDatetimeOffsetRef?: TemplateRef<any>;

  @ContentChild('inputDuration', { static: false })
  inputDurationRef?: TemplateRef<any>;

  @ContentChild('inputImage', { static: false })
  inputImageRef?: TemplateRef<any>;

  @ContentChild('inputFile', { static: false })
  inputFileRef?: TemplateRef<any>;

  @ContentChild('inputDefault', { static: false })
  inputDefaultRef?: TemplateRef<any>;

  constructor(
    private _clients: RESTworldClientCollection,
    private _confirmationService: ConfirmationService,
    private _messageService: MessageService,
    private _location: Location,
    private _router: Router,
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

  public async submit(templateName:string, template: Template, formValue: {}) {
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
      this._formTabs = RESTworldEditViewComponent.createFormTabs(this._templates);
    }

    this.isLoading = false;
  }

  public async onOptionsFiltered(property: Property, event: { originalEvent: any, filter: string | null; }) {
    if (!property?.options?.link?.href || !event.filter || event.filter == '')
      return;

    const templatedUri = property.options.link.href;
    let filter = `contains(${property.options.promptField}, '${event.filter}')`;
    if (property.options.valueField?.toLowerCase() === 'id' && !Number.isNaN(Number.parseInt(event.filter)))
      filter = `(${property.options.valueField} eq ${event.filter})  or (${filter})`;

    const response = await this.getClient().getListByUri(templatedUri, { $filter: filter, $top: 10 });
    if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
      const message = `An error occurred while getting the filtered items.`;
      this._messageService.add({ severity: 'error', summary: 'Error', detail: message, data: response });
      return;
    }

    const items = response.body._embedded.items;
    property.options.inline = items;
  }

  private async setInitialSelectedOptionsElementsForTemplates(templates: Templates) {
    return Promise.all(Object.values(templates)
      .map(template => this.setInitialSelectedOptionsElementsForTemplate(template)));
  }

  public imageChanged(formControl: FormControl, event: { files: File[]; }): void {
    const file = event.files[0];
    console.log(file);
    const reader = new FileReader();
    reader.onload = e => {
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
    if (!property?.options?.link?.href)
      return;

    const templatedUri = property.options.link.href;
    const filter = `${property.options.valueField} eq ${property.value}`;
    const response = await this.getClient().getListByUri(templatedUri, { $filter: filter, $top: 10 });
    if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
      const message = `An error occurred while getting the filtered items.`;
      this._messageService.add({ severity: 'error', summary: 'Error', detail: message, data: response });
      return;
    }

    const items = response.body._embedded.items;
    property.options.inline = items;
  }

  private async getAllTemplates(resource: Resource): Promise<Templates> {
    const formResponses = await this.getClient().getAllForms(resource);

    const failedResponses = formResponses.filter(response => !response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body);
    if (failedResponses.length !== 0) {
      for (var response of failedResponses) {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resource from the API.', data: response });
      }
      return Promise.resolve({});
    }

    const formTemplates = Object.assign({}, ...formResponses.map(response => (response.body as FormsResource)._templates)) as Templates;

    await this.setInitialSelectedOptionsElementsForTemplates(formTemplates);

    return formTemplates;
  }

  static createFormTabs(templates: Templates): { [key: string]: FormGroup } {
    const tabs = Object.fromEntries(Object.entries(templates).map(([name, template]) => [
      name,
      this.createFormGroup(template)
    ]));

    return tabs;
  }

  static createFormGroups(templates: Templates): FormGroup {
    const controls = Object.fromEntries(Object.entries(templates).map(([name, template]) => [
      name,
      this.createFormGroup(template)
    ]));
    const formGroup = new FormGroup(controls);
    return formGroup;
  }

  static createFormGroup(template: Template): FormGroup {
    const controls = Object.fromEntries(template.properties.map(p => [
      p.name,
      (p.type === PropertyType.Object || p.type === PropertyType.Collection) && p._templates ? RESTworldEditViewComponent.createFormGroups(p._templates) : this.createFormControl(p)
    ]));
    const formGroup = new FormGroup(controls);
    return formGroup;
  }

  private static createFormControl(property: Property): FormControl | FormGroup {
    if (property.type === PropertyType.Object || property.type === PropertyType.Collection)
      return RESTworldEditViewComponent.createFormGroups(property._templates);

    const control = new FormControl(property.value);
    if (property.max)
      control.addValidators(Validators.max(property.max));
    if (property.maxLength)
      control.addValidators(Validators.maxLength(property.maxLength));
    if (property.min)
      control.addValidators(Validators.min(property.min));
    if (property.minLength)
      control.addValidators(Validators.minLength(property.minLength));
    if (property.regex)
      control.addValidators(Validators.pattern(property.regex));
    if (property.required)
      control.addValidators(Validators.required);
    if (property.type === PropertyType.Email)
      control.addValidators(Validators.email);

    return control;
  }
}
