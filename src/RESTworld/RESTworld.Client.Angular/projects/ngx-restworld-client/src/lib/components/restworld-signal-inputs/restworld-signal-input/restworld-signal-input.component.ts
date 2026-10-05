import { Component, computed, forwardRef } from '@angular/core';
import type { FieldTree } from '@angular/forms/signals';
import { Options, Property, PropertyType, SimpleValue, Template } from '@wertzui/ngx-hal-client';
import { PropertyWithOptions } from '../../../models/special-properties';
import { RestWorldSignalInputLazyLoadBaseComponent } from '../restworld-signal-inputs';
import { RestWorldSignalInputSimpleComponent } from '../restworld-signal-input-simple/restworld-signal-input-simple.component';
import { RestWorldSignalInputDropdownComponent } from '../restworld-signal-input-dropdown/restworld-signal-input-dropdown.component';
import { RestWorldSignalInputObjectComponent } from '../restworld-signal-input-object/restworld-signal-input-object.component';
import { RestWorldSignalInputCollectionComponent } from '../restworld-signal-input-collection/restworld-signal-input-collection.component';
import { RestWorldSignalValidationErrorsComponent } from '../../restworld-signal-validation-errors/restworld-signal-validation-errors.component';

/**
 * An input that is automatically created from the given property, using Signal Forms.
 * This is the Signal Forms equivalent of {@link RestWorldInputComponent} `<rw-input>`.
 * @example
 * <rw-signal-input [apiName]="apiName" [property]="property" [field]="field"></rw-signal-input>
 */
@Component({
    selector: 'rw-signal-input',
    templateUrl: './restworld-signal-input.component.html',
    styleUrls: ['./restworld-signal-input.component.css'],
    imports: [RestWorldSignalInputDropdownComponent, RestWorldSignalInputSimpleComponent, forwardRef(() => RestWorldSignalInputObjectComponent), forwardRef(() => RestWorldSignalInputCollectionComponent), RestWorldSignalValidationErrorsComponent]
})
export class RestWorldSignalInputComponent<TProperty extends Property<SimpleValue, string, string> = Property<SimpleValue, string, string>> extends RestWorldSignalInputLazyLoadBaseComponent<TProperty> {
    /**
     * The property, narrowed to a property that has options (rendered as a dropdown), or `undefined` if it has none.
     */
    protected readonly dropdownProperty = computed(() => {
        const property: Property<SimpleValue, string, string> = this.property();

        return RestWorldSignalInputComponent.hasOptions(property) ? property : undefined;
    });

    /**
     * The property, narrowed to a property that has a default template (rendered as an object or a collection), or
     * `undefined` if it has none.
     */
    protected readonly templatedProperty = computed(() => {
        const property: Property<SimpleValue, string, string> = this.property();

        return RestWorldSignalInputComponent.hasDefaultTemplate(property) ? property : undefined;
    });

    /**
     * The field, typed with the generic `SimpleValue` that all child components work with.
     * @remarks The base class types `field` as `FieldTree<ExtractValueType<TProperty>>`. This is a conditional type
     * which cannot be resolved while `TProperty` is still generic, so it is not assignable to the
     * `FieldTree<SimpleValue>` that the child components expect. Which concrete value type the field holds depends
     * on the runtime `property().type` and can therefore not be expressed statically.
     */
    protected readonly simpleField = computed(() => this.field() as unknown as FieldTree<SimpleValue>);

    private static hasDefaultTemplate(property: Property<SimpleValue, string, string>): property is Property<SimpleValue, string, string> & { _templates: { default: Template } } {
        return property._templates?.default !== undefined;
    }

    private static hasOptions(property: Property<SimpleValue, string, string>): property is Property<SimpleValue, string, string> & { options: Options<SimpleValue, string, string> } {
        return property.options !== undefined && property.options !== null;
    }

    public get PropertyType() {
        return PropertyType;
    }

    public get PropertyWithOptions() {
        return PropertyWithOptions<SimpleValue, string, string>;
    }
}
