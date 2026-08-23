import { Component, TemplateRef, contentChild, forwardRef } from '@angular/core';
import type { FieldTree } from '@angular/forms/signals';
import { Property, SimpleValue, Template } from '@wertzui/ngx-hal-client';
import { NgTemplateOutlet } from '@angular/common';
import { RestWorldSignalInputLazyLoadBaseComponent } from '../restworld-signal-inputs';
import { RestWorldSignalInputTemplateComponent } from '../restworld-signal-input-template/restworld-signal-input-template.component';

/**
 * A complex object with multiple properties that is automatically created from the given property, using Signal Forms.
 * The object can also be nested.
 * This is the Signal Forms equivalent of {@link RestWorldInputObjectComponent} `<rw-input-object>`.
 * @remarks It is advised to use {@link RestWorldSignalInputComponent} `<rw-signal-input>` and control the rendered
 * input with the passed in property instead of using this component directly.
 * @example
 * <rw-signal-input-object [property]="property" [apiName]="apiName" [field]="field"></rw-signal-input-object>
 */
@Component({
    selector: 'rw-signal-input-object',
    templateUrl: './restworld-signal-input-object.component.html',
    styleUrls: ['./restworld-signal-input-object.component.css'],
    imports: [forwardRef(() => RestWorldSignalInputTemplateComponent), NgTemplateOutlet]
})
export class RestWorldSignalInputObjectComponent<TProperty extends Property<SimpleValue, string, string> & { _templates: { default: Template } } = Property<SimpleValue, string, string> & { _templates: { default: Template } }> extends RestWorldSignalInputLazyLoadBaseComponent<TProperty> {
    /**
     * A reference to a template that can be used to render custom content instead of the default nested
     * `<rw-signal-input-template>`.
     */
    public readonly inputObjectRef = contentChild<TemplateRef<unknown>>('inputObject');

    /**
     * The `FieldTree` for the nested object, re-typed as a `Record<string, unknown>` model so it can be passed
     * down to {@link RestWorldSignalInputTemplateComponent} `<rw-signal-input-template>`.
     * @remarks The base class types `field` as `FieldTree<ExtractValueType<TProperty>>`, which resolves to the
     * wide `SimpleValue` union for an `Object` property (the generic parameter does not model nested object
     * shapes). This getter narrows it to the shape `rw-signal-input-template` actually expects.
     */
    protected get innerField(): FieldTree<Record<string, unknown>> {
        return this.field() as unknown as FieldTree<Record<string, unknown>>;
    }
}
