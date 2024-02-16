import { Component, ContentChild, Input, OnInit, TemplateRef } from '@angular/core';
import { Property, SimpleValue, PropertyType, Template, NumberTemplate, TemplateDto, Options, ExtractGenericOptionsItemType, ExtractValueType, ExtractGenericOptionsSelectedValuesType, ProblemDetails, ResourceOfDto } from '@wertzui/ngx-hal-client';
import { PropertyWithOptions, PropertyWithImage } from '../../models/special-properties'
import { PropertyTemplateContext } from '../../models/templating';
import { MessageService } from 'primeng/api';
import { RestWorldClientCollection } from '../../services/restworld-client-collection';
import { RestWorldClient } from '../../services/restworld-client';

/**
 * Display the value of a form element with a label that is automatically created from a property in a form template.
 * This may also be a complex object or a collection in which case multiple and nested display elements may be rendered.
 * If you want to display just the value without a label, use RestWorldDisplayComponent <rw-display>.
 */
@Component({
    selector: 'rw-display-element',
    templateUrl: './restworld-display-element/restworld-display-element.component.html',
    styleUrl: './restworld-display-element/restworld-display-element.component.css'
})
export class RestWorldDisplayElementComponent<TProperty extends Property<SimpleValue, string, string>> {
    @Input({ required: true })
    property!: TProperty;

    @Input({ required: true })
    apiName!: string;

    private _value?: ExtractValueType<TProperty> | ExtractGenericOptionsSelectedValuesType<TProperty>[] | ExtractGenericOptionsSelectedValuesType<TProperty> | Record<string, SimpleValue> | Map<number, Record<string, SimpleValue>>;
    /**
     * The value to display. If not set, the value of the property will be used.
    */
    @Input()
    public set value(value: ExtractValueType<TProperty> | ExtractGenericOptionsSelectedValuesType<TProperty>[] | ExtractGenericOptionsSelectedValuesType<TProperty> | Record<string, SimpleValue> | Map<number, Record<string, SimpleValue>>) {
        this._value = value;
    }
    public get value() {
        return this._value ?? this.property.value as ExtractValueType<TProperty>;
    }

    public get PropertyType() {
        return PropertyType;
    }
}
/**
 * Displays a value that is automatically created from a property in a form template.
 * This may also be a complex object or a collection in which case multiple and nested input elements may be rendered.
 * If you also want a label, use RestWorldDisplayElementComponent <rw-display-element>.
 * You can also use one of the different RestWorldDisplay... <rw-display-...> elements to render a specific  property type,
 * but it is advised to control the rendered element through the passed in property.
 */
@Component({
    selector: 'rw-display',
    templateUrl: './restworld-display/restworld-display.component.html',
    styleUrl: './restworld-display/restworld-display.component.css'
})
export class RestWorldDisplayComponent<TProperty extends Property<SimpleValue, string, string>> {
    @Input({ required: true })
    property!: TProperty;

    @Input({ required: true })
    apiName!: string;

    private _value?: ExtractValueType<TProperty> | ExtractGenericOptionsSelectedValuesType<TProperty>[] | ExtractGenericOptionsSelectedValuesType<TProperty> | Record<string, SimpleValue> | Map<number, Record<string, SimpleValue>>;
    /**
     * The value to display. If not set, the value of the property will be used.
    */
    @Input()
    public set value(value: ExtractValueType<TProperty> | ExtractGenericOptionsSelectedValuesType<TProperty>[] | ExtractGenericOptionsSelectedValuesType<TProperty> | Record<string, SimpleValue> | Map<number, Record<string, SimpleValue>> | undefined) {
        this._value = value;
    }
    public get value() {
        return this._value ?? this.property.value as ExtractValueType<TProperty>;
    }

    public get PropertyType() {
        return PropertyType;
    }

    public get PropertyWithOptions() {
        return PropertyWithOptions<SimpleValue, string, string>;
    }
}

/**
 * Displays the value of a collection that is automatically created from the given property.
 * The elements and can also be nested.
 * @remarks It is advised to use RestWorldDisplayComponent <rw-display> and control the rendered elements with the passed in property
 * instead of using this component directly.
 */
@Component({
    selector: 'rw-display-collection',
    templateUrl: './restworld-display-collection/restworld-display-collection.component.html',
    styleUrl: './restworld-display-collection/restworld-display-collection.component.css'
})
export class RestWorldDisplayCollectionComponent implements OnInit {
    @Input({ required: true })
    property!: Property & { _templates: { default: Template } };

    @Input({ required: true })
    apiName!: string;

    private _values?: any[];
    /**
     * The value to display. If not set, the value of the property will be used.
    */
    @Input()
    public set values(values: any[] | Partial<Record<string, any>> | undefined) {
        this._values = RestWorldDisplayCollectionComponent.toArray(values);
    }
    public get values(): any[] | undefined {
        return this._values;
    }

    private _templates: NumberTemplate[] = [];
    public get templates(): NumberTemplate[] {
        return this._templates;
    }

    ngOnInit(): void {
        this._templates = this._values ?
            Array.from(RestWorldDisplayCollectionComponent.map(this._values.entries(), entry => (new NumberTemplate({ ...(this.property._templates.default as TemplateDto), title: entry[0].toString() })))) :
            RestWorldDisplayCollectionComponent.getCollectionEntryTemplates(this.property);
    }

    private static * map<T1, T2>(iterable: IterableIterator<T1>, callback: (value: T1) => T2) {
        for (let x of iterable) {
            yield callback(x);
        }
    }

    private static toArray(arrayOrDictionary: any[] | Partial<Record<string, any>> | undefined): any[] | undefined {
        if (!arrayOrDictionary)
            return undefined;

        if (Array.isArray(arrayOrDictionary))
            return arrayOrDictionary;

        return Object.entries(arrayOrDictionary).map(([key, value]) => ({ key, value }));
    }

    private static getCollectionEntryTemplates(property?: Property): NumberTemplate[] {
        if (!property)
            return [];

        return Object.entries(property._templates)
            .filter(([key, value]) => Number.isInteger(Number.parseInt(key)) && Number.isInteger(Number.parseInt(value?.title ?? "")))
            .map(([, value]) => new NumberTemplate(value as TemplateDto));
    }
}

/**
 * Displays the value of a dropdown that is automatically created from the given property.
 * If set the dropdown will use the `inline` property of the options to render the `selectedValues` Otherwise it will just render the values, but no prompt for them.
 * @remarks It is advised to use RestWorldDisplayComponent <rw-display> and control the rendered elements with the passed in property
 * instead of using this component directly.
 */
@Component({
    selector: 'rw-display-dropdown',
    templateUrl: './restworld-display-dropdown/restworld-display-dropdown.component.html',
    styleUrl: './restworld-display-dropdown/restworld-display-dropdown.component.css'
})
export class RestWorldDisplayDropdownComponent<TProperty extends Property<SimpleValue, string, string> & { options: Options<SimpleValue, string, string> }, TOptionsItem extends ExtractGenericOptionsItemType<TProperty> = ExtractGenericOptionsItemType<TProperty>> implements OnInit {
    @Input({ required: true })
    property!: TProperty;

    @Input({ required: true })
    apiName!: string;

    private _selectedValues?: ExtractGenericOptionsSelectedValuesType<TProperty>[];
    /**
     * The values to display. If not set, the values of the property options will be used.
    */
    @Input()
    public set selectedValues(value: ExtractGenericOptionsSelectedValuesType<TProperty>[] | ExtractGenericOptionsSelectedValuesType<TProperty> | undefined) {
        if (Array.isArray(value))
            this._selectedValues = value as ExtractGenericOptionsSelectedValuesType<TProperty>[];
        else if (value !== undefined && value !== null)
            this._selectedValues = [value];
    }

    public get selectedValues(): ExtractGenericOptionsSelectedValuesType<TProperty>[] {
        return this._selectedValues ?? (this.property.options.selectedValues as unknown as ExtractGenericOptionsSelectedValuesType<TProperty>[] | undefined) ?? [];
    }

    private _inline: TOptionsItem[] = [];

    /**
     * A function that returns the label for the given item.
     * The default returns the prompt and optionally the value in brackets.
     * The value in brackets will only be displayed if the `cols` field of the property is undefined or greater than 1.
     * Overwrite this function to change the label.
     * @param item The item to get the label for.
     */
    @Input()
    getLabel: (item: TOptionsItem) => string = this.getLabelInternal;

    /**
     * A function that returns the tooltip for the given item.
     * The default returns all properties of the item except the ones that start with an underscore or the ones that are in the list of default properties to exclude.
     * The default properties to exclude are: createdAt, createdBy, lastChangedAt, lastChangedBy, timestamp, promptField, valueField.
     * Overwrite this function to change the tooltip.
     * @param item The item to get the label for.
     */
    @Input()
    getTooltip: (item: TOptionsItem) => string = this.getTooltipInternal;

    @ContentChild('inputOptions', { static: false })
    inputOptionsRef?: TemplateRef<PropertyTemplateContext>;

    public get valueField(): string {
        return this.property.options.valueField ?? "value";
    }

    public get promptField(): string {
        return this.property.options.promptField ?? "prompt";
    }

    constructor(
        private readonly _messageService: MessageService,
        private readonly _clients: RestWorldClientCollection,
    ) {
    }

    async ngOnInit(): Promise<void> {
        if (this.property.options?.inline && this.property.options.inline.length > 0)
            this._inline = Array.from(this.property.options.inline) as TOptionsItem[];

        await this.setInitialSelectedOptionsElementForProperty();
    }

    public getOptionItem(value: ExtractGenericOptionsSelectedValuesType<TProperty>): TOptionsItem {
        return this._inline.find(o => this.getValue(o as TOptionsItem) === value) as TOptionsItem ?? this.createOptionsItem(value);
    }

    private createOptionsItem(value: ExtractGenericOptionsSelectedValuesType<TProperty>): TOptionsItem {
        return { [this.valueField]: value, [this.promptField]: "" } as TOptionsItem;
    }

    public getValue(item: TOptionsItem): ExtractGenericOptionsSelectedValuesType<TProperty> {
        if (item === undefined || item === null || !item.hasOwnProperty(this.valueField))
            throw new Error(`The item does not have a property ${this.valueField}.`);

        return item[this.valueField as keyof TOptionsItem] as ExtractGenericOptionsSelectedValuesType<TProperty>;
    }

    public getPrompt(item: TOptionsItem): string {
        if (item === undefined || item === null || !item.hasOwnProperty(this.promptField))
            return "";

        return item[this.promptField as keyof TOptionsItem] as string;
    }

    public getLabelInternal(item: TOptionsItem): string {
        if (item === undefined || item === null)
            return "";

        const prompt = this.getPrompt(item);
        const value = this.getValue(item);

        let label = prompt;
        if (label === undefined || label === null || label === "")
            label = (value?.toString() ?? "");
        else if ((this.property.cols === undefined || this.property.cols > 1) && prompt?.toLowerCase() !== value?.toString().toLowerCase())
            label += ` (${value})`;

        return label;
    }

    private getTooltipInternal(item: TOptionsItem): string {
        if (item === undefined || item === null)
            return "";

        const tooltip = Object.entries(item)
            .filter(([key]) => !(key.startsWith('_') || ['createdAt', 'createdBy', 'lastChangedAt', 'lastChangedBy', 'timestamp', this.promptField, this.valueField].includes(key)))
            .reduce((prev, [key, value], index) => `${prev}${index === 0 ? '' : '\n'}${key}: ${RestWorldDisplayDropdownComponent.jsonStringifyWithElipsis(value)}`, '');

        return tooltip;
    }

    private async setInitialSelectedOptionsElementForProperty(): Promise<void> {
        const options = this.property.options;

        if (!options.link?.href || !this.selectedValues || this.selectedValues.length === 0 || this.selectedValues.every(v => this._inline.some(i => this.getValue(i as TOptionsItem) === v) ?? false))
            return;

        const filter = `${options.valueField} in (${this.selectedValues})`;
        await this.SetInlineOptionsFromFilter(filter);
    }

    private async SetInlineOptionsFromFilter(filter: string) {
        const options = this.property.options;
        if (!options.link?.href)
            throw new Error('The property does not have a link href.');

        const templatedUri = options.link.href;
        const response = await this.getClient().getListByUri<TOptionsItem>(templatedUri, { $filter: filter, $top: 10 });
        if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
            const message = `An error occurred while getting the initial selected items for the property ${this.property.name}.`;
            this._messageService.add({ severity: 'error', summary: 'Error', detail: message, data: response, sticky: true });
        }

        const items = response.body!._embedded.items as ResourceOfDto<TOptionsItem>[];
        const newItems = this.combineInlineWithSelected(items);
        this._inline = newItems;
    }

    private combineInlineWithSelected(items: ResourceOfDto<TOptionsItem>[]): TOptionsItem[] {
        const oldInline = this._inline as ResourceOfDto<TOptionsItem>[];
        if (!oldInline || oldInline.length === 0)
            return items;

        const itemsToKeep = oldInline.filter(i => this.selectedValues.includes(this.getValue(i as TOptionsItem)));
        const newItems = items.concat(itemsToKeep.filter(i => !items.includes(i)));

        return newItems;
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
 * Displays the value of a complex object with multiple properties that is automatically created from the given property.
 * The object can also be nested.
 * @remarks It is advised to use RestWorldDisplayComponent <rw-display> and control the rendered elements with the passed in property
 * instead of using this component directly.
 */
@Component({
    selector: 'rw-display-object',
    templateUrl: './restworld-display-object/restworld-display-object.component.html',
    styleUrl: './restworld-display-object/restworld-display-object.component.css'
})
export class RestWorldDisplayObjectComponent {
    @Input({ required: true })
    property!: Property<null, never, never> & { _templates: { default: Template } };

    @Input({ required: true })
    apiName!: string;

    private _value?: Record<string, SimpleValue>;
    /**
     * The value to display. If not set, the value of the template properties will be used.
    */
    @Input()
    public set value(value: Record<string, SimpleValue> | undefined) {
        this._value = value;
    }
    public get value() {
        return this._value;
    }
}

/**
 * Displays the value of a simple element, like a string, a number or a Date that is automatically created from the given property.
 * @remarks It is advised to use RestWorldDisplayComponent <rw-display> and control the rendered elements with the passed in property
 * instead of using this component directly.
 */
@Component({
    selector: 'rw-display-simple',
    templateUrl: './restworld-display-simple/restworld-display-simple.component.html',
    styleUrl: './restworld-display-simple/restworld-display-simple.component.css'
})
export class RestWorldDisplaySimpleComponent<TProperty extends Property<SimpleValue, string, string>> {
    @Input({ required: true })
    property!: TProperty;

    @Input({ required: true })
    apiName!: string;

    private _value?: SimpleValue;
    /**
     * The value to display. If not set, the value of the property will be used.
    */
    @Input()
    public set value(value: SimpleValue) {
        this._value = value;
    }
    public get value() {
        return this._value ?? this.property.value;
    }

    public get PropertyType() {
        return PropertyType;
    }

    private static readonly _dateFormat = new Date(3333, 10, 22) // months start at 0 in JS
        .toLocaleDateString()
        .replace("22", "dd")
        .replace("11", "MM")
        .replace("3333", "yyyy")
        .replace("33", "yy");

    public get dateFormat(): string {
        return RestWorldDisplaySimpleComponent._dateFormat;
    }

    private static readonly _timeFormat = new Date(1, 1, 1, 22, 33, 44)
        .toLocaleTimeString()
        .replace("22", "hh")
        .replace("33", "mm")
        .replace("44", "ss");

    public get timeFormat() {
        return RestWorldDisplaySimpleComponent._timeFormat;
    }

    public get PropertyWithImage() {
        return PropertyWithImage;
    }
}

/**
 * Displays the values of a collection of rw-display-elements automatically created from a template.
 */
@Component({
    selector: 'rw-display-template',
    templateUrl: './restworld-display-template/restworld-display-template.component.html',
    styleUrl: './restworld-display-template/restworld-display-template.component.css'
})
export class RestWorldDisplayTemplateComponent {
    @Input({ required: true })
    template!: Template;

    @Input({ required: true })
    apiName!: string;

    private _value?: Record<string, any>;
    /**
     * The value to display. If not set, the values of the template properties will be used.
    */
    @Input()
    public set value(value: Record<string, any> | undefined) {
        this._value = value;
    }
    public get value() {
        return this._value;
    }
}
