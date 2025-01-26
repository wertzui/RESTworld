import { CdkDragDrop, DragDropModule } from '@angular/cdk/drag-drop';
import { ChangeDetectorRef, Component, ContentChild, Directive, ElementRef, OnInit, TemplateRef, computed, contentChild, effect, forwardRef, input, linkedSignal, model, output, signal, untracked, viewChild } from '@angular/core';
import { AbstractControl, ControlContainer, FormArray, FormControl, FormGroup, FormGroupDirective, FormsModule, NG_VALUE_ACCESSOR, ReactiveFormsModule, type ControlValueAccessor } from '@angular/forms';
import { FormService, NumberTemplate, Options, OptionsItemDto, ProblemDetails, Property, PropertyType, ResourceOfDto, SimpleValue, Template, TemplateDto } from '@wertzui/ngx-hal-client';
import { MessageService } from 'primeng/api';
import { DropdownChangeEvent } from '../../models/events';
import { PropertyTemplateContext } from '../../models/templating';
import { RestWorldClientCollection } from '../../services/restworld-client-collection';
import { PropertyWithOptions, PropertyWithImage } from '../../models/special-properties'
import { debounce } from '../../util/debounce';
import { MultiSelect } from 'primeng/multiselect';
import { RestWorldLabelComponent } from "../restworld-label/restworld-label.component";
import { ButtonDirective } from "primeng/button";
import { RestWorldValidationErrorsComponent } from "../restworld-validation-errors/restworld-validation-errors.component";
import { Select } from "primeng/select";
import { Tooltip } from "primeng/tooltip";
import { Chip } from 'primeng/chip';
import { DatePicker } from "primeng/datepicker";
import { InputNumber } from "primeng/inputnumber";
import { Checkbox } from "primeng/checkbox";
import { RestWorldImageComponent } from "../restworld-image/restworld-image.component";
import { RestWorldFileComponent } from "../restworld-file/restworld-file.component";
import { InputText } from "primeng/inputtext";
import { JsonPipe, NgTemplateOutlet } from "@angular/common";
import { HalFormsModule } from "../../directives/property.directives";
import { OptionsManager, OptionsService } from "../../services/options.service";
import { toObservable, toSignal } from '@angular/core/rxjs-interop';
import { map, merge, mergeMap } from "rxjs";

/**
 * This helper type converts a Property<X, Y, Z> to an OptionsItemDto<X, Y, Z> and preserves the generic parameters.
 */
export type ExtractGenericOptionsItemType<TProperty> = TProperty extends Property<infer X, infer Y, infer Z> ? OptionsItemDto<X, Y, Z> : never
export type ExtractGenericOptionsSelectedValuesType<TProperty> = TProperty extends Property<infer X, infer Y, infer Z> ? Options<X, Y, Z>["selectedValues"] : never
export type ExtractValueType<TProperty> = TProperty extends Property<infer X, infer Y, infer Z> ? X : never

/**
 * A base class for all input components..
 */
@Directive()
export abstract class RestWorldInputBaseComponent<TProperty extends Property<SimpleValue, string, string> = Property<SimpleValue, string, string>> {
    /**
     * The property to display.
     * @required
     */
    public readonly property = model.required<TProperty>();

    /**
     * Set this to true if the input should use template driven forms instead of the default reactive forms.
     */
    public readonly useTemplateDrivenForms = input(false);

    public readonly model = model<ExtractValueType<TProperty>>();
}

/**
 * A base class for all input components which also feature lazy loading, like dropdowns.
 */
@Directive()
export abstract class RestWorldInputLazyLoadBaseComponent<TProperty extends Property<SimpleValue, string, string> = Property<SimpleValue, string, string>> extends RestWorldInputBaseComponent<TProperty> {
    /**
     * The name of the API to use for the property.
     * @required
     * @remarks This is the name of the API as defined in the `RestWorldClientCollection`.
     */
    public readonly apiName = input.required<string>();
}

/**
 * A form element with a label that is automatically created from a property in a form template.
 * This may also be a complex object or a collection in which case multiple and nested input elements may be rendered.
 * If you want a form element without a label, use {@link RestWorldFormInput} `<rw-input>`.
 * @example
 * <rw-form-element [property]="property" [apiName]="apiName"></rw-form-element>
 */
@Component({
    selector: 'rw-form-element',
    templateUrl: './restworld-form-element/restworld-form-element.component.html',
    styleUrls: ['./restworld-form-element/restworld-form-element.component.css'],
    viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
    imports: [forwardRef(() => RestWorldInputComponent), RestWorldLabelComponent]
})
export class RestWorldFormElementComponent<TProperty extends Property<SimpleValue, string, string> = Property<SimpleValue, string, string>> extends RestWorldInputLazyLoadBaseComponent<TProperty> {
}

/**
 * A collection that is automatically created from the given property.
 * The collection supports drag & drop to re order the elements and can also be nested.
 * @remarks It is advised to use {@link RestWorldInputComponent} `<rw-input>` and control the rendered inputs with the passed in property
 * instead of using this component directly.
 * @example
 * <rw-input-collection [property]="property" [apiName]="apiName"></rw-input-collection>
 */
@Component({
    selector: 'rw-input-collection',
    templateUrl: './restworld-input-collection/restworld-input-collection.component.html',
    styleUrls: ['./restworld-input-collection/restworld-input-collection.component.css'],
    viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
    imports: [forwardRef(() => RestWorldInputTemplateComponent), DragDropModule, ButtonDirective, ReactiveFormsModule]
})
export class RestWorldInputCollectionComponent<TProperty extends Property<SimpleValue, string, string> & { _templates: { default: Template } } = Property<SimpleValue, string, string> & { _templates: { default: Template } }> extends RestWorldInputLazyLoadBaseComponent<TProperty> {
    public readonly defaultTemplate = computed(() => this.property()._templates.default);
    public readonly innerFormArray = computed(() => (this._controlContainer.control as FormGroup<any>)?.controls[this.property().name] as FormArray<FormGroup<any>>);
    public readonly templates = computed(() => this.getCollectionEntryTemplates(this.property()));

    @ContentChild('inputCollection', { static: false })
    public inputCollectionRef?: TemplateRef<unknown>;

    constructor(
        private readonly _formService: FormService,
        private readonly _controlContainer: ControlContainer,
    ) {
        super();
    }

    private getCollectionEntryTemplates(property?: TProperty): NumberTemplate[] {
        if (!property)
            return [];

        return Object.entries(property._templates)
            .filter(([key, value]) => Number.isInteger(Number.parseInt(key)) && Number.isInteger(Number.parseInt(value?.title ?? "")))
            .map(([, value]) => new NumberTemplate(value as TemplateDto));
    }

    public addNewItemToCollection(): void {
        const templates = this.templates();
        const defaultTemplate = this.defaultTemplate();

        const maxIndex = Math.max(...Object.keys(templates)
            .map(key => Number.parseInt(key))
            .filter(key => Number.isSafeInteger(key)));
        const nextIndex = maxIndex < 0 ? 0 : maxIndex + 1;

        const copiedTemplateDto = JSON.parse(JSON.stringify(defaultTemplate)) as TemplateDto;
        copiedTemplateDto.title = nextIndex.toString();
        const copiedTemplate = new Template(copiedTemplateDto);

        this.property.update((property) => { property._templates[nextIndex] = copiedTemplate; return { ...property }; });
        this.innerFormArray().push(this._formService.createFormGroupFromTemplate(this.defaultTemplate()));
    }

    public collectionItemDropped($event: CdkDragDrop<{}>) {
        const previousIndex = $event.previousIndex;
        const currentIndex = $event.currentIndex;
        const movementDirection = currentIndex > previousIndex ? 1 : -1;

        // Move in FormArray
        // We do not need to move the item in the _templates object
        const innerFormArray = this.innerFormArray();
        const movedControl = innerFormArray.at(previousIndex);
        for (let i = previousIndex; i * movementDirection < currentIndex * movementDirection; i = i + movementDirection) {
            innerFormArray.setControl(i, innerFormArray.at(i + movementDirection));
        }
        innerFormArray.setControl(currentIndex, movedControl);
    }

    public deleteItemFromCollection(template: NumberTemplate): void {
        const title = template.title;
        if (title === undefined)
            throw new Error("Cannot delete a template without a title.");

        this.property.update((property) => { delete property._templates[title]; return { ...property }; });
        this.innerFormArray().removeAt(title);
    }
}

/**
 * A form input element that is automatically created from a property in a form template.
 * This may also be a complex object or a collection in which case multiple and nested input elements may be rendered.
 * If you also want a label, use {@link RestWorldFormElement} `<rw-form-element>`.
 * You can also use one of the different RestWorldInput... `<rw-input-...>` elements to render a specific input,
 * but it is advised to control the rendered input through the passed in property.
 * @example
 * <rw-input [property]="property" [apiName]="apiName"></rw-input>
 */
@Component({
    selector: 'rw-input',
    templateUrl: './restworld-input/restworld-input.component.html',
    styleUrls: ['./restworld-input/restworld-input.component.css'],
    standalone: true,
    viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
    imports: [forwardRef(() => RestWorldInputDropdownComponent), forwardRef(() => RestWorldInputObjectComponent), forwardRef(() => RestWorldInputSimpleComponent), RestWorldInputCollectionComponent, RestWorldValidationErrorsComponent, FormsModule]
})
export class RestWorldInputComponent<TProperty extends Property<SimpleValue, string, string> = Property<SimpleValue, string, string>> extends RestWorldInputLazyLoadBaseComponent<TProperty> {
    public get PropertyType() {
        return PropertyType;
    }

    public get PropertyWithOptions() {
        return PropertyWithOptions<SimpleValue, string, string>;
    }
}

/**
 * A dropdown that is automatically created from the given property.
 * The dropdown supports searching through a RESTWorld list endpoint on the backend if the `link` of the options is set.
 * Otherwise the dropdown will use the `inline` of the options.
 * @remarks It is advised to use {@link RestWorldInputComponent} `<rw-input>` and control the rendered inputs with the passed in property
 * instead of using this component directly.
 * @example
 * <rw-input-dropdown [property]="property" [apiName]="apiName"></rw-input-dropdown>
 */
@Component({
    selector: 'rw-input-dropdown',
    templateUrl: './restworld-input-dropdown/restworld-input-dropdown.component.html',
    styleUrls: ['./restworld-input-dropdown/restworld-input-dropdown.component.css'],
    standalone: true,
    viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
    imports: [Select, ReactiveFormsModule, MultiSelect, Tooltip, Chip, NgTemplateOutlet, HalFormsModule, FormsModule]
})
export class RestWorldInputDropdownComponent<TProperty extends Property<SimpleValue, string, string> & { options: Options<SimpleValue, string, string> }, TOptionsItem extends ExtractGenericOptionsItemType<TProperty> = ExtractGenericOptionsItemType<TProperty>> extends RestWorldInputLazyLoadBaseComponent<TProperty> implements OnInit {
    /**
     * A flag that indicates if the search should be case sensitive.
     * The default is false.
     */
    public readonly caseSensitive = input(false);
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

    public readonly inputOptionsMultipleRef = contentChild<TemplateRef<PropertyTemplateContext>>("inputOptionsMultiple");
    public readonly inputOptionsSingleRef = contentChild<TemplateRef<PropertyTemplateContext>>("inputOptionsSingle");
    public readonly multiSelect = viewChild<MultiSelect>(MultiSelect);
    /**
     * An event that is emitted when the selected value changes.
     */
    public onChange = output<DropdownChangeEvent<TOptionsItem>>();
    public onOptionsFiltered = debounce(this.onOptionsFilteredInternal, 500);
    public readonly optionsManager: OptionsManager<TProperty, TOptionsItem>;

    private readonly _formControl = computed(() => {
        const formGroup = this._controlContainer.control as FormGroup<any>;
        return formGroup.controls[this.property().name] as AbstractControl<ExtractGenericOptionsSelectedValuesType<TProperty> | ExtractGenericOptionsSelectedValuesType<TProperty>[]>;
    });

    private readonly _valueChangesSignal = toSignal(
        merge(
            // Get the initial value when 'control' changes
            toObservable(this._formControl).pipe(map((ctl) => ctl.value)),
            // Get the new value when 'control.value' changes
            toObservable(this._formControl).pipe(mergeMap((ctl) => ctl.valueChanges))
        )
    );

    constructor(
        private readonly _controlContainer: ControlContainer,
        optionsService: OptionsService,
    ) {
        super();
        //const valueChangesSignal = toSignal(this._formControl.valueChanges);
        this.optionsManager = optionsService.getManager(this.apiName, this.property, this._valueChangesSignal, this.getLabel, this.getTooltip);
    }

    public async ngOnInit(): Promise<void> {
        await this.optionsManager.initialize();
    }

    public onOptionsChanged(event: DropdownChangeEvent<TOptionsItem>) {
        this.onChange.emit(event);
    }

    public async onOptionsFilteredInternal(event: { originalEvent: Event; filter: string | null }) {
        const options = this.optionsManager.options();
        const currentItems = this.optionsManager.items();

        if (!(event.filter) || event.filter === '')
            return;

        if (event.originalEvent.type === "input") {
            const inputEvent = (event.originalEvent as InputEvent);
            if (inputEvent.inputType === "insertFromPaste") {
                // If the user pasted in multiple ids as comma separated list, we want to get them all and set them as the selected value.

                var values = event.filter
                    .split(",")
                    .filter(v => v !== '')
                    .map(v => v.trim())
                    .map(v => {
                        const n = Number.parseFloat(v);
                        return Number.isNaN(n) ? this.makeUpperIfCaseInsensitive(v.toUpperCase(), false) : n;
                    });

                if (!values || values.length === 0)
                    return;

                const allAreNumbers = values.every(v => typeof v === "number" && !isNaN(v));
                const filter = allAreNumbers
                    ? `${options.valueField} in (${values.join(',')})`
                    : `contains(${this.makeUpperIfCaseInsensitive(options.promptField, true)}, '${values.join("', '")}')`;

                if ((options?.link?.href))
                    await this.optionsManager.updateItemsFromFilter(filter);

                if (currentItems) {
                    const selectedValues = currentItems
                        .map(i => this.optionsManager.getValue(i))
                        .filter(v => values.includes(v as unknown as string | number));
                    this._formControl().setValue(selectedValues);
                    this.multiSelect()?.resetFilter();
                }
            }
            else {
                // This is the normal case where the user types in a filter.

                let filter = `contains(${this.makeUpperIfCaseInsensitive(options.promptField, true)}, '${this.makeUpperIfCaseInsensitive(event.filter, false)}')`;
                if (options.valueField?.toLowerCase() === 'id' && !Number.isNaN(Number.parseInt(event.filter)))
                    filter = `(${options.valueField} eq ${event.filter})  or (${filter})`;

                if ((options?.link?.href))
                    await this.optionsManager.updateItemsFromFilter(filter);
            }
        }
    }

    private makeUpperIfCaseInsensitive(filter: string | null | undefined, isOData: boolean): string | null | undefined {
        if (this.caseSensitive() || typeof filter !== "string")
            return filter;

        if (isOData)
            return `toupper(${filter})`;

        return filter.toUpperCase();
    }
}

/**
 * A complex object with multiple properties that is automatically created from the given property.
 * The object can also be nested.
 * @remarks It is advised to use {@link RestWorldInputComponent} `<rw-input>` and control the rendered inputs with the passed in property
 * instead of using this component directly.
 * @example
 * <rw-input-object [property]="property" [apiName]="apiName"></rw-input-object>
 */
@Component({
    selector: 'rw-input-object',
    templateUrl: './restworld-input-object/restworld-input-object.component.html',
    styleUrls: ['./restworld-input-object/restworld-input-object.component.css'],
    standalone: true,
    viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
    imports: [forwardRef(() => RestWorldInputTemplateComponent), ReactiveFormsModule, NgTemplateOutlet]
})
export class RestWorldInputObjectComponent<TProperty extends Property<SimpleValue, string, string> & { _templates: { default: Template } } = Property<SimpleValue, string, string> & { _templates: { default: Template } }> extends RestWorldInputLazyLoadBaseComponent<TProperty> {
    public inputObjectRef = contentChild<TemplateRef<unknown>>("inputObject");

    constructor(
        private readonly _controlContainer: ControlContainer
    ) {
        super();
    }

    public readonly innerFormGroup = computed(() => {
        const formGroup = this._controlContainer.control as FormGroup<any>;
        return formGroup.controls[this.property().name] as FormGroup<any>;
    });
}

/**
 * A simple input element, like a string, a number or a Date that is automatically created from the given property.
 * @remarks It is advised to use {@link RestWorldInputComponent} `<rw-input>` and control the rendered inputs with the passed in property
 * instead of using this component directly.
 * @example
 * <rw-input-simple [property]="property" [apiName]="apiName"></rw-input-simple>
 */
@Component({
    selector: 'rw-input-simple',
    templateUrl: './restworld-input-simple/restworld-input-simple.component.html',
    styleUrls: ['./restworld-input-simple/restworld-input-simple.component.css'],
    standalone: true,
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => RestWorldInputSimpleComponent),
        multi: true
    }],
    viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
    imports: [DatePicker, InputNumber, Checkbox, RestWorldImageComponent, RestWorldFileComponent, InputText, HalFormsModule, FormsModule]
})
export class RestWorldInputSimpleComponent<TProperty extends Property<SimpleValue, string, string> = Property<SimpleValue, string, string>> extends RestWorldInputBaseComponent<TProperty> implements ControlValueAccessor {
    private static readonly _dateFormat = new Date(3333, 10, 22) // months start at 0 in JS
        .toLocaleDateString()
        .replace("22", "dd")
        .replace("11", "mm")
        .replace("3333", "yy")
        .replace("33", "y");
    private static readonly _timeFormat = new Date(1, 1, 1, 22, 33, 44)
        .toLocaleTimeString()
        .replace("22", "hh")
        .replace("33", "mm")
        .replace("44", "ss");

    private readonly _inputChild = viewChild<ElementRef<HTMLInputElement | HTMLTextAreaElement>>("inputElement");
    private readonly _controlChild = viewChild<ControlValueAccessor>(NG_VALUE_ACCESSOR);


    constructor(
    ) {
        super();
    }
    writeValue(obj: any): void {
        const controlChild = this._controlChild();
        const inputChild = this._inputChild();
        if (controlChild !== undefined)
            controlChild.writeValue(obj);
        else if (inputChild !== undefined)
            inputChild.nativeElement.value = obj;
    }
    registerOnChange(fn: any): void {
        const controlChild = this._controlChild();
        const inputChild = this._inputChild();
        if (controlChild !== undefined)
            controlChild.registerOnChange(fn);
        else if (inputChild !== undefined)
            inputChild.nativeElement.oninput = (event) => fn((event.target as HTMLInputElement | HTMLTextAreaElement).value);
    }
    registerOnTouched(fn: any): void {
        const controlChild = this._controlChild();
        const inputChild = this._inputChild();
        if (controlChild !== undefined)
            controlChild.registerOnTouched(fn);
        else if (inputChild !== undefined)
            inputChild.nativeElement.onblur = (event) => fn();
    }
    setDisabledState?(isDisabled: boolean): void {
        const controlChild = this._controlChild();
        const inputChild = this._inputChild();
        if (controlChild !== undefined && controlChild.setDisabledState !== undefined)
            controlChild.setDisabledState(isDisabled);
        else if (inputChild !== undefined) {
            if (isDisabled)
                inputChild.nativeElement.setAttribute("disabled", "disabled");
            else
                inputChild.nativeElement.removeAttribute("disabled");
        }
    }

    public get PropertyType() {
        return PropertyType;
    }

    public get PropertyWithImage() {
        return PropertyWithImage;
    }

    public get dateFormat(): string {
        return RestWorldInputSimpleComponent._dateFormat;
    }

    // public readonly formControl = computed(() => {
    //     const formGroup = this._controlContainer.control as FormGroup<any>;
    //     return formGroup.controls[this.property().name] as FormControl<SimpleValue | SimpleValue[]>;
    // });

    public get timeFormat() {
        return RestWorldInputSimpleComponent._timeFormat;
    }
}

/**
 * A collection of `<rw-form>` elements automatically created from a template.
 * Does not have any buttons on its own.
 * If you want buttons, use {@link RestWorldForm} `<rw-form>`.
 * @example
 * <rw-form-collection [template]="template" [apiName]="apiName"></rw-form-collection>
 */
@Component({
    selector: 'rw-input-template',
    templateUrl: './restworld-input-template/restworld-input-template.component.html',
    styleUrls: ['./restworld-input-template/restworld-input-template.component.css'],
    standalone: true,
    viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
    imports: [RestWorldFormElementComponent]
})
export class RestWorldInputTemplateComponent<T extends { [K in keyof T]: AbstractControl<any, any>; }> {
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
}
