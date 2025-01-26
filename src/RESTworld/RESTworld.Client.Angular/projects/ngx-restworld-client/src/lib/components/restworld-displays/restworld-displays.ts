import { Component, computed, forwardRef, input, OnInit } from '@angular/core';
import { Property, SimpleValue, PropertyType, Template, NumberTemplate, TemplateDto, Options, ExtractGenericOptionsItemType, ExtractValueType, ExtractGenericOptionsSelectedValuesType } from '@wertzui/ngx-hal-client';
import { PropertyWithOptions, PropertyWithImage } from '../../models/special-properties'
import { RestWorldLabelComponent } from "../restworld-label/restworld-label.component";
import { Tooltip } from "primeng/tooltip";
import { RestWorldAvatarComponent } from "../restworld-avatar/restworld-avatar.component";
import { PropertyTypeFormatPipe } from "../../pipes/property-type-format.pipe";
import { CheckboxModule } from "primeng/checkbox";
import { FormsModule } from "@angular/forms";
import { OptionsManager, OptionsService } from "../../services/options.service";

/**
 * Displays the value of a collection that is automatically created from the given property.
 * The elements and can also be nested.
 * @remarks It is advised to use {@link RestWorldDisplayComponent} `<rw-display>` and control the rendered elements with the passed in property
 * instead of using this component directly.
 * @example
 * <rw-display-collection [property]="property" [apiName]="apiName" [values]="values"></rw-display-collection>
 */
@Component({
    selector: 'rw-display-collection',
    templateUrl: './restworld-display-collection/restworld-display-collection.component.html',
    styleUrl: './restworld-display-collection/restworld-display-collection.component.css',
    standalone: true,
    imports: [forwardRef(() => RestWorldDisplayTemplateComponent)]
})
export class RestWorldDisplayCollectionComponent {
    /**
     * The name of the API to use for the property.
     * @required
     * @remarks This is the name of the API as defined in the `RestWorldClientCollection`.
     */
    public readonly apiName = input.required<string>();
    /**
     * The property to display.
     * @required
     */
    public readonly property = input.required<Property & { _templates: { default: Template } }>();
    public readonly templates = computed(() => {
        const values = this.values();
        const property = this.property();

        return values ?
            Array.from(RestWorldDisplayCollectionComponent.map(RestWorldDisplayCollectionComponent.toArray(values)!.entries(), (entry: [string | number, any]) => (new NumberTemplate({ ...(property._templates.default as TemplateDto), title: entry[0].toString() })))) :
            RestWorldDisplayCollectionComponent.getCollectionEntryTemplates(property);
    });
    /**
     * The value to display. If not set, the value of the property will be used.
    */
    public readonly values = input<any[] | Partial<Record<string, any> | undefined>>();

    private static getCollectionEntryTemplates(property?: Property): NumberTemplate[] {
        if (!property)
            return [];

        return Object.entries(property._templates)
            .filter(([key, value]) => Number.isInteger(Number.parseInt(key)) && Number.isInteger(Number.parseInt(value?.title ?? "")))
            .map(([, value]) => new NumberTemplate(value as TemplateDto));
    }

    private static * map<T1, T2>(iterable: IterableIterator<T1>, callback: (value: T1) => T2) {
        for (let x of iterable) {
            yield callback(x);
        }
    }

    private static toArray<T>(arrayOrDictionary: T[] | Partial<Record<string, T>> | undefined): T[] | { key: string, value: T }[] | undefined {
        if (!arrayOrDictionary)
            return undefined;

        if (Array.isArray(arrayOrDictionary))
            return arrayOrDictionary;

        return Object.entries(arrayOrDictionary).map(([key, value]) => ({ key, value: value! }));
    }
}

/**
 * Displays a value that is automatically created from a property in a form template.
 * This may also be a complex object or a collection in which case multiple and nested input elements may be rendered.
 * If you also want a label, use {@link RestWorldDisplayElementComponent} `<rw-display-element>`.
 * You can also use one of the different RestWorldDisplay... `<rw-display-...>` elements to render a specific property type,
 * but it is advised to control the rendered element through the passed in property.
 * @example
 * <rw-display [property]="property" [apiName]="apiName" [value]="value"></rw-display>
 */
@Component({
    selector: 'rw-display',
    templateUrl: './restworld-display/restworld-display.component.html',
    styleUrl: './restworld-display/restworld-display.component.css',
    standalone: true,
    imports: [forwardRef(() => RestWorldDisplayDropdownComponent), forwardRef(() => RestWorldDisplayObjectComponent), forwardRef(() => RestWorldDisplaySimpleComponent), RestWorldDisplayCollectionComponent]
})
export class RestWorldDisplayComponent<TProperty extends Property<SimpleValue, string, string>> {
    /**
     * The name of the API to use for the property.
     * @required
     * @remarks This is the name of the API as defined in the `RestWorldClientCollection`.
     */
    public readonly apiName = input.required<string>();
    /**
     * Either the value or the property.value if the value is not set.
     */
    public readonly computedValue = computed(() => this.value() ?? this.property().value as ExtractValueType<TProperty>);
    /**
     * The property to display.
     * @required
     */
    public readonly property = input.required<TProperty>();
    /**
     * The value to display. If not set, the value of the property will be used.
    */
    public readonly value = input<ExtractValueType<TProperty> | ExtractGenericOptionsSelectedValuesType<TProperty>[] | ExtractGenericOptionsSelectedValuesType<TProperty> | Record<string, SimpleValue> | Map<number, Record<string, SimpleValue>>>();

    public get PropertyType() {
        return PropertyType;
    }

    public get PropertyWithOptions() {
        return PropertyWithOptions<SimpleValue, string, string>;
    }
}

/**
 * Displays the value of a dropdown that is automatically created from the given property.
 * If set the dropdown will use the `inline` property of the options to render the `selectedValues` Otherwise it will just render the values, but no prompt for them.
 * @remarks It is advised to use {@link RestWorldDisplayComponent} `<rw-display>` and control the rendered elements with the passed in property
 * instead of using this component directly.
 * @example
 * <rw-display-dropdown [property]="property" [apiName]="apiName" [selectedValues]="selectedValues"></rw-display-dropdown>
 */
@Component({
    selector: 'rw-display-dropdown',
    templateUrl: './restworld-display-dropdown/restworld-display-dropdown.component.html',
    styleUrl: './restworld-display-dropdown/restworld-display-dropdown.component.css',
    standalone: true,
    imports: [Tooltip]
})
export class RestWorldDisplayDropdownComponent<TProperty extends Property<SimpleValue, string, string> & { options: Options<SimpleValue, string, string> }, TOptionsItem extends ExtractGenericOptionsItemType<TProperty> = ExtractGenericOptionsItemType<TProperty>> implements OnInit {
    /**
     * The name of the API to use for the property.
     * @required
     * @remarks This is the name of the API as defined in the `RestWorldClientCollection`.
     */
    public readonly apiName = input.required<string>();
    /**
     * A function that returns the label for the given item.
     * The default returns the prompt and optionally the value in brackets.
     * The value in brackets will only be displayed if the `cols` field of the property is undefined or greater than 1.
     * Overwrite this function to change the label.
     * @param item The item to get the label for.
     */
    public readonly getLabel = input<(itemOrValue: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>) => string | undefined>();
    /**
     * A function that returns the tooltip for the given item.
     * The default returns all properties of the item except the ones that start with an underscore or the ones that are in the list of default properties to exclude.
     * The default properties to exclude are: createdAt, createdBy, lastChangedAt, lastChangedBy, timestamp, promptField, valueField.
     * Overwrite this function to change the tooltip.
     * @param item The item to get the label for.
     */
    public readonly getTooltip = input<(itemOrValue: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>) => string | undefined>();
    public readonly options = computed(() => this.property().options!);
    /**
     * The property to display.
     * @required
     */
    public readonly property = input.required<TProperty>();
    public readonly selectedValues = input([] as ExtractGenericOptionsSelectedValuesType<TProperty>[], { transform: (value: ExtractGenericOptionsSelectedValuesType<TProperty> | ExtractGenericOptionsSelectedValuesType<TProperty>[]) => (Array.isArray(value) ? value : [value]) as ExtractGenericOptionsSelectedValuesType<TProperty>[] });

    public readonly optionsManager: OptionsManager<TProperty, TOptionsItem>;

    constructor(
        optionsService: OptionsService
    ) {
        this.optionsManager = optionsService.getManager(this.apiName, this.property, this.selectedValues, this.getLabel, this.getTooltip);
    }

    public async ngOnInit(): Promise<void> {
        await this.optionsManager.initialize();
    }
}

/**
 * Display the value of a form element with a label that is automatically created from a property in a form template.
 * This may also be a complex object or a collection in which case multiple and nested display elements may be rendered.
 * If you want to display just the value without a label, use {@link RestWorldDisplayComponent} `<rw-display>`.
 * @example
 * <rw-display-element [property]="property" [apiName]="apiName" [value]="value"></rw-display-element>
 */
@Component({
    selector: 'rw-display-element',
    templateUrl: './restworld-display-element/restworld-display-element.component.html',
    styleUrl: './restworld-display-element/restworld-display-element.component.css',
    standalone: true,
    imports: [RestWorldLabelComponent, RestWorldDisplayComponent]
})
export class RestWorldDisplayElementComponent<TProperty extends Property<SimpleValue, string, string>> {
    /**
     * The name of the API to use for the property.
     * @required
     * @remarks This is the name of the API as defined in the `RestWorldClientCollection`.
     */
    public readonly apiName = input.required<string>();
    /**
     * Either the value or the property.value if the value is not set.
     */
    public readonly computedValue = computed(() => this.value() ?? this.property().value as ExtractValueType<TProperty>);
    /**
     * The property to display.
     * @required
     */
    public readonly property = input.required<TProperty>();
    /**
     * The value to display. If not set, the value of the property will be used.
    */
    public value = input<ExtractValueType<TProperty> | ExtractGenericOptionsSelectedValuesType<TProperty>[] | ExtractGenericOptionsSelectedValuesType<TProperty> | Record<string, SimpleValue> | Map<number, Record<string, SimpleValue>>>();

    public get PropertyType() {
        return PropertyType;
    }
}

/**
 * Displays the value of a complex object with multiple properties that is automatically created from the given property.
 * The object can also be nested.
 * @remarks It is advised to use {@link RestWorldDisplayComponent} `<rw-display>` and control the rendered elements with the passed in property
 * instead of using this component directly.
 * @example
 * <rw-display-object [property]="property" [apiName]="apiName" [value]="value"></rw-display-object>
 */
@Component({
    selector: 'rw-display-object',
    templateUrl: './restworld-display-object/restworld-display-object.component.html',
    styleUrl: './restworld-display-object/restworld-display-object.component.css',
    standalone: true,
    imports: [forwardRef(() => RestWorldDisplayTemplateComponent)]
})
export class RestWorldDisplayObjectComponent {
    /**
     * The name of the API to use for the property.
     * @required
     * @remarks This is the name of the API as defined in the `RestWorldClientCollection`.
     */
    public readonly apiName = input.required<string>();
    /**
     * The property to display.
     * @required
     */
    public readonly property = input.required<Property<null, never, never> & {
        _templates: {
            default: Template;
        };
    }>();
    /**
     * The value to display. If not set, the value of the template properties will be used.
    */
    public readonly value = input<Record<string, SimpleValue>>();
}

/**
 * Displays the value of a simple element, like a string, a number or a Date that is automatically created from the given property.
 * @remarks It is advised to use {@link RestWorldDisplayComponent} `<rw-display>` and control the rendered elements with the passed in property
 * instead of using this component directly.
 * @example
 * <rw-display-simple [property]="property" [apiName]="apiName" [value]="value"></rw-display-simple>
 */
@Component({
    selector: 'rw-display-simple',
    templateUrl: './restworld-display-simple/restworld-display-simple.component.html',
    styleUrl: './restworld-display-simple/restworld-display-simple.component.css',
    standalone: true,
    imports: [RestWorldAvatarComponent, PropertyTypeFormatPipe, CheckboxModule, FormsModule]
})
export class RestWorldDisplaySimpleComponent<TProperty extends Property<SimpleValue, string, string>> {
    private static readonly _dateFormat = new Date(3333, 10, 22) // months start at 0 in JS
        .toLocaleDateString()
        .replace("22", "dd")
        .replace("11", "MM")
        .replace("3333", "yyyy")
        .replace("33", "yy");
    private static readonly _timeFormat = new Date(1, 1, 1, 22, 33, 44)
        .toLocaleTimeString()
        .replace("22", "hh")
        .replace("33", "mm")
        .replace("44", "ss");

    /**
     * The name of the API to use for the property.
     * @required
     * @remarks This is the name of the API as defined in the `RestWorldClientCollection`.
     */
    public readonly apiName = input.required<string>();
    /**
     * Either the value or the property.value if the value is not set.
     */
    public readonly computedValue = computed(() => this.value() ?? this.property().value ?? this._defaultValue());
    /**
     * The property to display.
     * @required
     */
    public readonly property = input.required<TProperty>();
    /**
     * The value to display. If not set, the value of the property will be used.
    */
    public readonly value = input<SimpleValue>();

    private readonly _defaultValue = computed(() => {
        const property = this.property();
        if (!property.required || property.options?.minItems === 0)
            return undefined;

        if (property.type === PropertyType.Number)
            return 0;

        return undefined;
    });

    public get PropertyType() {
        return PropertyType;
    }

    public get PropertyWithImage() {
        return PropertyWithImage;
    }

    public get dateFormat(): string {
        return RestWorldDisplaySimpleComponent._dateFormat;
    }

    public get timeFormat() {
        return RestWorldDisplaySimpleComponent._timeFormat;
    }
}

/**
 * Displays the values of a collection of `<rw-display>` elements automatically created from a template.
 * @example
 * <rw-display-template [template]="template" [apiName]="apiName" [value]="value"></rw-display-template>
 */
@Component({
    selector: 'rw-display-template',
    templateUrl: './restworld-display-template/restworld-display-template.component.html',
    styleUrl: './restworld-display-template/restworld-display-template.component.css',
    standalone: true,
    imports: [RestWorldDisplayElementComponent]
})
export class RestWorldDisplayTemplateComponent {
    /**
     * The name of the API to use for the property.
     * @required
     * @remarks This is the name of the API as defined in the `RestWorldClientCollection`.
     */
    public readonly apiName = input.required<string>();
    /**
     * The template to display.
     * @required
     * @remarks This is the template that defines the properties to display.
     */
    public readonly template = input.required<Template>();
    /**
     * The value to display. If not set, the values of the template properties will be used.
    */
    public readonly value = input<Record<string, any>>();
}
