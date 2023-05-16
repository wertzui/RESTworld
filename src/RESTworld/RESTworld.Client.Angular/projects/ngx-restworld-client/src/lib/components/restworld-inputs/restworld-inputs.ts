import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { ChangeDetectorRef, Component, ContentChild, Input, OnInit, TemplateRef } from '@angular/core';
import { AbstractControl, ControlContainer, FormArray, FormControl, FormGroup, FormGroupDirective } from '@angular/forms';
import { NumberTemplate, Options, Property, PropertyType, ResourceDto, ResourceOfDto, Template, TemplateDto } from '@wertzui/ngx-hal-client';
import { FormService } from '../../services/form.service';
import { MessageService } from 'primeng/api';
import { ProblemDetails } from '../../models/problem-details';
import { PropertyTemplateContext } from '../../models/templating';
import { RestWorldClient } from '../../services/restworld-client';
import { RestWorldClientCollection } from '../../services/restworld-client-collection';
import { PropertyWithOptions, PropertyWithImage } from '../../models/special-properties'
import * as _ from 'lodash';

/**
 * A form element with a label that is automatically created from a property in a form template.
 * This may also be a complex object or a collection in which case multiple and nested input elements may be rendered.
 * If you want a form element without a label, use RestWorldFormInput <rw-form-input>.
 */
@Component({
  selector: 'rw-form-element',
  templateUrl: './restworld-form-element/restworld-form-element.component.html',
  styleUrls: ['./restworld-form-element/restworld-form-element.component.css'],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }]
})
export class RestWorldFormElementComponent<T extends { [K in keyof T]: AbstractControl<any, any>; }> {
  @Input()
  property!: Property;

  @Input()
  apiName!: string;
}

/**
 * A form input element that is automatically created from a property in a form template.
 * This may also be a complex object or a collection in which case multiple and nested input elements may be rendered.
 * If you also want a label, use RestWorldFormElement <rw-form-element>.
 * You can also use one of the different RestWorldInput... <rw-input-...> elements to render a specific input,
 * but it is advised to control the rendered input through the passed in property.
 */
@Component({
  selector: 'rw-input',
  templateUrl: './restworld-input/restworld-input.component.html',
  styleUrls: ['./restworld-input/restworld-input.component.css'],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }]
})
export class RestWorldInputComponent {

  @Input()
  property!: Property;

  @Input()
  apiName!: string;

  public get PropertyType() {
    return PropertyType;
  }

  public get PropertyWithOptions() {
    return PropertyWithOptions;
  }
}


/**
 * A collection that is automatically created from the given property.
 * The collection supports drag & drop to re order the elements and can also be nested.
 * It is advised to use RestWorldInputComponent <rw-input> and control the rendered inputs with the passed in property
 * instead of using this component directly.
 */
@Component({
  selector: 'rw-input-collection',
  templateUrl: './restworld-input-collection/restworld-input-collection.component.html',
  styleUrls: ['./restworld-input-collection/restworld-input-collection.component.css'],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }]
})
export class RestWorldInputCollectionComponent<T extends { [K in keyof T]: AbstractControl<any, any>; }> implements OnInit{

  @Input()
  property!: Property;

  @Input()
  apiName!: string;

  @ContentChild('inputCollection', { static: false })
  inputCollectionRef?: TemplateRef<unknown>;


  private _templates: NumberTemplate[] = [];
  public get templates(): NumberTemplate[] {
    return this._templates;
  }

  private _defaultTemplate?: Template;
  public get defaultTemplate(): Template {
    if(this._defaultTemplate === undefined)
      throw new Error("No default template found.");

    return this._defaultTemplate;
  }

  private _innerFormArray?: FormArray<FormGroup<T>>;
  public get innerFormArray() {
    if (this._innerFormArray === undefined)
      throw new Error("formGroup is not set.");

    return this._innerFormArray;
  }

  constructor (
    private readonly _controlContainer: ControlContainer,
    private readonly _formService: FormService,
    private readonly _changeDetectorRef: ChangeDetectorRef,
  ) {
  }

  ngOnInit(): void {
    const formGroup = this._controlContainer.control as FormGroup<any>;
    this._innerFormArray = formGroup.controls[this.property.name] as FormArray<FormGroup<T>>;
    this._templates = RestWorldInputCollectionComponent.getCollectionEntryTemplates(this.property);
    this._defaultTemplate = this.property._templates.default;
  }

  private static getCollectionEntryTemplates(property?: Property): NumberTemplate[] {
    if (!property)
      return [];

    return Object.entries(property._templates)
      .filter(([key, value]) => Number.isInteger(Number.parseInt(key)) && Number.isInteger(Number.parseInt(value.title ?? "")))
      .map(([, value]) => new NumberTemplate(value));
  }

  public addNewItemToCollection(): void {
    const maxIndex = Math.max(...Object.keys(this.templates)
      .map(key => Number.parseInt(key))
      .filter(key => Number.isSafeInteger(key)));
    const nextIndex = maxIndex < 0 ? 0 : maxIndex + 1;

    const copiedTemplateDto = JSON.parse(JSON.stringify(this.defaultTemplate)) as TemplateDto;
    const copiedTemplate = new NumberTemplate(copiedTemplateDto);
    copiedTemplate.title = nextIndex;

    this.templates[copiedTemplate.title] = copiedTemplate;
    this.innerFormArray.push(this._formService.createFormGroupFromTemplate(this.defaultTemplate));
  }

  public deleteItemFromCollection(template: NumberTemplate): void {
    delete this.templates[template.title];
    this.innerFormArray.removeAt(template.title);
  }

  public collectionItemDropped($event: CdkDragDrop<{ }>) {
    const previousIndex = $event.previousIndex;
    const currentIndex = $event.currentIndex;
    const movementDirection = currentIndex > previousIndex ? 1 : -1;

    // Move in FormArray
    // We do not need to move the item in the _templates object
    const movedControl = this.innerFormArray.at(previousIndex);
    for (let i = previousIndex; i * movementDirection < currentIndex * movementDirection; i = i + movementDirection) {
      this.innerFormArray.setControl(i, this.innerFormArray.at(i + movementDirection));
    }
    this.innerFormArray.setControl(currentIndex, movedControl);

    this._changeDetectorRef.markForCheck();
  }
}

/**
 * A dropdown that is automatically created from the given property.
 * The dropdown supports searching through a RESTWorld list endpoint on the backend.
 * It is advised to use RestWorldInputComponent <rw-input> and control the rendered inputs with the passed in property
 * instead of using this component directly.
 */
@Component({
  selector: 'rw-input-dropdown',
  templateUrl: './restworld-input-dropdown/restworld-input-dropdown.component.html',
  styleUrls: ['./restworld-input-dropdown/restworld-input-dropdown.component.css'],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }]
})
export class RestWorldInputDropdownComponent<T> implements OnInit {
  @Input()
  property!: Property & { options: Options };

  @Input()
  apiName!: string;

  @ContentChild('inputOptionsSingle', { static: false })
  inputOptionsSingleRef?: TemplateRef<PropertyTemplateContext>;

  @ContentChild('inputOptionsMultiple', { static: false })
  inputOptionsMultipleRef?: TemplateRef<PropertyTemplateContext>;

  private _formControl!: AbstractControl<T, T>;
  private _filterValue: string = "";

  public get filterValue(): string {
    return this._filterValue;
  }

  public get valueField(): string {
    return this.property.options.valueField ?? "value";
  }

  public get promptField(): string {
    return this.property.options.promptField ?? "prompt";
  }

  constructor (
    private readonly _messageService: MessageService,
    private readonly _clients: RestWorldClientCollection,
    private readonly _controlContainer: ControlContainer,
  ) {
  }

  async ngOnInit(): Promise<void> {
    const formGroup = this._controlContainer.control as FormGroup<any>;
    this._formControl = formGroup.controls[this.property.name] as AbstractControl<T>;
    await this.setInitialSelectedOptionsElementForProperty();
  }

  public getValue(item: T): any {
    if(item === undefined || item === null || !item.hasOwnProperty(this.valueField))
      return null;

    return item[this.valueField as keyof T];
  }

  public getPrompt(item: T): string {
    if(item === undefined || item === null || !item.hasOwnProperty(this.promptField))
      return "";

    return item[this.promptField as keyof T] as string;
  }

  public getLabel(item: T): string {
    if(item === undefined || item === null)
      return "";

    const label = `${this.getPrompt(item)} (${this.getValue(item)})`;

    return label;
  }

  public getTooltip(item: T): string {
    if(item === undefined || item === null)
      return "";

    const tooltip = Object.entries(item)
      .filter(([key]) => !(key.startsWith('_') || ['createdAt', 'createdBy', 'lastChangedAt', 'lastChangedBy', 'timestamp', this.promptField, this.valueField].includes(key)))
      .reduce((prev, [key, value], index) => `${prev}${index === 0 ? '' : '\n'}${key}: ${RestWorldInputDropdownComponent.jsonStringifyWithElipsis(value)}`, '');

    return tooltip;
  }

  public getItemByValue(value: any): T {
    return this.property.options.inline.find(item => this.getValue(item as T) === value) as T;
  }

  public removeByValue(value: any): void {
    const formValue = this._formControl.value as unknown[];
    const newValue = formValue.filter(i => i !== value) as T;
    this._formControl.setValue(newValue);
  }

  public async onOptionsFiltered(event: { originalEvent: unknown; filter: string | null }) {
    const options = this.property?.options;

    if (!options?.link?.href || !event.filter || event.filter === '')
      return;


    // const templatedUri = options.link.href;
    let filter = `contains(${options.promptField}, '${event.filter}')`;
    if (options.valueField?.toLowerCase() === 'id' && !Number.isNaN(Number.parseInt(event.filter)))
      filter = `(${options.valueField} eq ${event.filter})  or (${filter})`;

    await this.SetInlineOptionsFromFilter(filter, event.filter);
  }

  private async setInitialSelectedOptionsElementForProperty(): Promise<void> {
    const options = this.property.options;

    if (!options.link?.href || !options.selectedValues) {
      if (!_.isArray(options.inline) || options.inline.length === 0)
        options.inline = [];
      return;
    }

    const filter = `${options.valueField} in (${options.selectedValues})`;
    await this.SetInlineOptionsFromFilter(filter, "");
  }

  private async SetInlineOptionsFromFilter(filter: string, eventFilter: string) {
    const options = this.property.options;
    if  (!options.link?.href)
      throw new Error('The property does not have a link href.');

    const templatedUri = options.link.href;
    const response = await this.getClient().getListByUri(templatedUri, { $filter: filter, $top: 10 });
    if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
      const message = `An error occurred while getting the initial selected items for the property ${this.property.name}.`;
      this._messageService.add({ severity: 'error', summary: 'Error', detail: message, data: response, sticky: true });
    }

    const items = response.body!._embedded.items;
    const newItems = this.combineInlineWithSelected(items);
    options.inline = newItems;

    // The multiselect component does not update the options when the inline options are updated, so we need to trigger a change detection.
    this._filterValue = eventFilter;
  }

  private combineInlineWithSelected(items: ResourceOfDto<ResourceDto>[]): unknown[] {
    const oldInline = this.property.options.inline as ResourceOfDto<ResourceDto>[];
    if (!oldInline)
      return items;

    const selectedValues = this.getSelectedValues();
    const itemsToKeep = oldInline.filter(i => selectedValues.includes(this.getValue(i as T)));
    const newItems = _.uniqBy(items.concat(itemsToKeep), i => this.getValue(i as T));

    return newItems;
  }

  private getSelectedValues(): unknown[] {
    const value = this._formControl.value;
    if (_.isArray(value))
      return value

    return [value];
  }

  private getClient(): RestWorldClient {
    if (!this.apiName)
      throw new Error('Cannot get a client, because the apiName is not set.');

    return this._clients.getClient(this.apiName);
  }

  private static jsonStringifyWithElipsis(value: unknown) {
    const maxLength = 200;
    const end = 10;
    const start = maxLength - end - 2;
    const json = JSON.stringify(value);
    const shortened = json.length > maxLength ? json.substring(0, start) + '…' + json.substring(json.length - end) : json;

    return shortened;
  }
}

/**
 * A complex object with multiple properties that is automatically created from the given property.
 * The object can also be nested.
 * It is advised to use RestWorldInputComponent <rw-input> and control the rendered inputs with the passed in property
 * instead of using this component directly.
 */
@Component({
  selector: 'rw-input-object',
  templateUrl: './restworld-input-object/restworld-input-object.component.html',
  styleUrls: ['./restworld-input-object/restworld-input-object.component.css'],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }]
})
export class RestWorldInputObjectComponent<T extends { [K in keyof T]: AbstractControl<any, any>; }> implements OnInit {

  @Input()
  property!: Property;

  @Input()
  apiName!: string;

  @ContentChild('inputObject', { static: false })
  inputObjectRef?: TemplateRef<unknown>;

  private _innerFormGroup?: FormGroup<T>;

  public get innerFormGroup() {
    if (this._innerFormGroup === undefined)
      throw new Error("formGroup is not set.");

    return this._innerFormGroup;
  }

  constructor (
    private readonly _controlContainer: ControlContainer
  ) {
  }

  ngOnInit(): void {
    const formGroup = this._controlContainer.control as FormGroup<any>;
    this._innerFormGroup = formGroup.controls[this.property.name] as FormGroup<T>;
  }
}

/**
 * A simple input element, like a text, or a number that is automatically created from the given property.
 * It is advised to use RestWorldInputComponent <rw-input> and control the rendered inputs with the passed in property
 * instead of using this component directly.
 */
@Component({
  selector: 'rw-input-simple',
  templateUrl: './restworld-input-simple/restworld-input-simple.component.html',
  styleUrls: ['./restworld-input-simple/restworld-input-simple.component.css'],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }]
})
export class RestWorldInputSimpleComponent<T> implements OnInit {

  @Input()
  property!: Property;

  public get PropertyType() {
    return PropertyType;
  }

  private static readonly _dateFormat = new Date(3333, 10, 22) // months start at 0 in JS
    .toLocaleDateString()
    .replace("22", "dd")
    .replace("11", "mm")
    .replace("3333", "yy")
    .replace("33", "y");

  public get dateFormat(): string {
    return RestWorldInputSimpleComponent._dateFormat;
  }

  private static readonly _timeFormat = new Date(1, 1, 1, 22, 33, 44)
    .toLocaleTimeString()
    .replace("22", "hh")
    .replace("33", "mm")
    .replace("44", "ss");

  public get timeFormat() {
    return RestWorldInputSimpleComponent._timeFormat;
  }

  private _formControl?: FormControl<T>;

  public get formControl() {
    if (this._formControl === undefined)
      throw new Error("formGroup is not set.");

    return this._formControl;
  }

  public get PropertyWithImage() {
    return PropertyWithImage;
  }

  constructor (
    private readonly _controlContainer: ControlContainer
  ) {
  }

  ngOnInit(): void {
    const formGroup = this._controlContainer.control as FormGroup<any>;
    this._formControl = formGroup.controls[this.property.name] as FormControl<T>;
  }
}

/**
 * A collection of rw-form-elemtns automatically created from a template.
 * Does not have any buttons on its own.
 * If you want buttons, use RestWorldForm <rw-form>.
 */
@Component({
  selector: 'rw-input-template',
  templateUrl: './restworld-input-template/restworld-input-template.component.html',
  styleUrls: ['./restworld-input-template/restworld-input-template.component.css'],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }]
})
export class RestWorldInputTemplateComponent<T extends { [K in keyof T]: AbstractControl<any, any>; }> {
  @Input()
  apiName!: string;

  @Input()
  template!: Template;
}
