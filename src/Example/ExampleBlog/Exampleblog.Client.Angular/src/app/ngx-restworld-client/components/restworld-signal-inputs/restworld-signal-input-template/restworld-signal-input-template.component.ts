import { Component, forwardRef, input } from '@angular/core';
import type { FieldTree } from '@angular/forms/signals';
import { NumberTemplate, Template } from '@wertzui/ngx-hal-client';
import { getChildFieldTree } from '../../../util/field-tree';
import { RestWorldSignalFormElementComponent } from '../restworld-signal-form-element/restworld-signal-form-element.component';

/**
 * Renders a `<rw-signal-form-element>` for every property of the given template, using Signal Forms.
 * Does not have any buttons on its own.
 * If you want buttons, use {@link RestWorldSignalFormComponent} `<rw-signal-form>`.
 * This is the Signal Forms equivalent of {@link RestWorldInputTemplateComponent} `<rw-input-template>`.
 * @example
 * <rw-signal-input-template [template]="template" [apiName]="apiName" [field]="field"></rw-signal-input-template>
 * @remarks `RestWorldSignalFormElementComponent` is wrapped in `forwardRef()` because it closes an import cycle:
 * rw-signal-input-template -> rw-signal-form-element -> rw-signal-input -> rw-signal-input-object ->
 * rw-signal-input-template.
 */
@Component({
    selector: 'rw-signal-input-template',
    templateUrl: './restworld-signal-input-template.component.html',
    styleUrls: ['./restworld-signal-input-template.component.css'],
    imports: [forwardRef(() => RestWorldSignalFormElementComponent)]
})
export class RestWorldSignalInputTemplateComponent<TModel extends Record<string, unknown> = Record<string, unknown>> {
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
    public readonly template = input.required<Template | NumberTemplate>();
    /**
     * The `FieldTree` for the model that the template describes.
     * @required
     */
    public readonly field = input.required<FieldTree<TModel>>();

    protected fieldFor(propertyName: string) {
        return getChildFieldTree(this.field() as unknown as FieldTree<Record<string, unknown>>, propertyName);
    }
}
