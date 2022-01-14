import { CdkDragDrop } from '@angular/cdk/drag-drop';
import { OnInit } from '@angular/core';
import { ChangeDetectorRef } from '@angular/core';
import { Component, ContentChild, Input, TemplateRef } from '@angular/core';
import { AbstractControl, FormArray, FormGroup } from '@angular/forms';
import { Property, PropertyType, Resource, Template, TemplateDto } from '@wertzui/ngx-hal-client';
import { MessageService } from 'primeng/api';
import { ProblemDetails } from '../../models/problem-details';
import { FormService } from '../../services/form.service';
import { RESTworldClient } from '../../services/restworld-client';
import { RESTworldClientCollection } from '../../services/restworld-client-collection';

@Component({
  selector: 'rw-form',
  templateUrl: './restworld-edit-form.component.html',
  styleUrls: ['./restworld-edit-form.component.css']
})
export class RestworldEditFormComponent implements OnInit {
  @Input()
  formGroup!: FormGroup;

  @Input()
  template!: Template;

  @Input()
  apiName?: string;

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

  @ContentChild('inputObject', { static: false })
  inputObjectRef?: TemplateRef<unknown>;

  @ContentChild('inputCollection', { static: false })
  inputCollectionRef?: TemplateRef<unknown>;

  @ContentChild('inputDefault', { static: false })
  inputDefaultRef?: TemplateRef<unknown>;

  public get PropertyType() {
    return PropertyType;
  }

  public get dateFormat(): string {
    return new Date(3333, 10, 22)
      .toLocaleDateString()
      .replace("22", "dd")
      .replace("11", "mm")
      .replace("3333", "yy")
      .replace("33", "y");
  }

  public FormGroup = FormGroup;
  public FormArray = FormArray;
  public Number = Number;

  constructor(
    private _formService: FormService,
    private _changeDetectorRef: ChangeDetectorRef,
    private _messageService: MessageService,
    private _clients: RESTworldClientCollection
  ) { }

  ngOnInit(): void {
    if (!this.formGroup)
      throw new Error("[formGroup] is required on <rw-form>");
    if (!this.template)
      throw new Error("[template] is required on <rw-form>");
    if (!this.apiName)
      throw new Error("[apiName] is required on <rw-form>");
  }

  public getTooltip(resource: Resource, keysToExclude?: string[]): string {
    const tooltip = Object.entries(resource)
      .filter(([key]) => !(key.startsWith('_') || ['createdAt', 'createdBy', 'lastChangedAt', 'lastChangedBy', 'timestamp'].includes(key) || keysToExclude?.includes(key)))
      .reduce((prev, [key, value], index) => `${prev}${index === 0 ? '' : '\n'}${key}: ${RestworldEditFormComponent.jsonStringifyWithElipsis(value)}`, '');

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

  public getCollectionEntryTemplates(property?: Property): Template[] {
    if (!property)
      return [];

    return Object.entries(property._templates)
      .filter(([key,]) => Number.isInteger(Number.parseInt(key)))
      .map(([, value]) => value);
  }

  public addNewItemToCollection(property: Property, formArray: FormArray | AbstractControl): void {
    if (!(formArray instanceof FormArray))
      throw new Error('formArray is not an instance of FormArray.');

    const maxIndex = Math.max(...Object.keys(property._templates)
      .map(key => Number.parseInt(key))
      .filter(key => Number.isSafeInteger(key)));
    const nextIndex = maxIndex + 1;

    const defaultTemplate = property._templates['default'];
    const copiedTemplateDto = JSON.parse(JSON.stringify(defaultTemplate)) as TemplateDto;
    const copiedTemplate = new Template(copiedTemplateDto);
    copiedTemplate.title = nextIndex.toString();

    property._templates[copiedTemplate.title] = copiedTemplate;
    formArray.push(this._formService.createFormGroupFromTemplate(defaultTemplate));
  }

  public deleteItemFromCollection(property: Property, formArray: FormArray | AbstractControl, template: Template): void {
    if (!template.title)
      throw new Error(`Cannot delete the item, because the template '${template}' does not have a title.`);

    if (!(formArray instanceof FormArray))
      throw new Error('formArray is not an instance of FormArray.');

    const templates = property._templates;
    delete templates[template.title];

    formArray.removeAt(Number.parseInt(template.title));
  }

  public collectionItemDropped($event: CdkDragDrop<{ property: Property; formArray: FormArray }>) {
    const formArray = $event.container.data.formArray;
    const previousIndex = $event.previousIndex;
    const currentIndex = $event.currentIndex;
    const movementDirection = currentIndex > previousIndex ? 1 : -1;

    // Move in FormArray
    // We do not need to move the item in the _templates object
    const movedControl = formArray.at(previousIndex);
    for (let i = previousIndex; i * movementDirection < currentIndex * movementDirection; i = i + movementDirection) {
      formArray.setControl(i, formArray.at(i + movementDirection));
    }
    formArray.setControl(currentIndex, movedControl);

    this._changeDetectorRef.markForCheck();

    console.log($event);
  }


  public async onOptionsFiltered(property: Property, event: { originalEvent: unknown; filter: string | null }) {
    const options = property?.options;

    if (!options?.link?.href || !event.filter || event.filter === '')
      return;


    const templatedUri = options.link.href;
    let filter = `contains(${options.promptField}, '${event.filter}')`;
    if (options.valueField?.toLowerCase() === 'id' && !Number.isNaN(Number.parseInt(event.filter)))
      filter = `(${options.valueField} eq ${event.filter})  or (${filter})`;

    const response = await this.getClient().getListByUri(templatedUri, { $filter: filter, $top: 10 });
    if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
      const message = `An error occurred while getting the filtered items.`;
      this._messageService.add({ severity: 'error', summary: 'Error', detail: message, data: response });
      return;
    }

    const items = response.body._embedded.items;
    options.inline = items;
  }

  private getClient(): RESTworldClient {
    if (!this.apiName)
      throw new Error('Cannot get a client, because the apiName is not set.');

    return this._clients.getClient(this.apiName);
  }
}
