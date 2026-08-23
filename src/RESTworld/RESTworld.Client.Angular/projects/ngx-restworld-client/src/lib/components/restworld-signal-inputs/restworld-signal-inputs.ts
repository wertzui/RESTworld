import { Directive, input, model } from '@angular/core';
import type { FieldTree } from '@angular/forms/signals';
import { Property, SimpleValue } from '@wertzui/ngx-hal-client';
import { ExtractValueType } from '../restworld-inputs/restworld-inputs';

/**
 * A base class for all Signal Forms based input components.
 * @remarks
 * Signal Forms has no equivalent of the `ControlContainer` dependency injection mechanism that Reactive Forms
 * input components use to find their ancestor form. Instead, the `FieldTree` for the bound property must be
 * passed down explicitly through the `field` input.
 */
@Directive()
export abstract class RestWorldSignalInputBaseComponent<TProperty extends Property<SimpleValue, string, string> = Property<SimpleValue, string, string>> {
    /**
     * The property to display.
     * @required
     */
    public readonly property = input.required<TProperty>();

    /**
     * The `FieldTree` node for the property to bind to.
     * @required
     */
    public readonly field = input.required<FieldTree<ExtractValueType<TProperty>>>();
}

/**
 * A base class for all Signal Forms based input components which also feature lazy loading, like dropdowns.
 */
@Directive()
export abstract class RestWorldSignalInputLazyLoadBaseComponent<TProperty extends Property<SimpleValue, string, string> = Property<SimpleValue, string, string>> extends RestWorldSignalInputBaseComponent<TProperty> {
    /**
     * The name of the API to use for the property.
     * @required
     * @remarks This is the name of the API as defined in the `RestWorldClientCollection`.
     */
    public readonly apiName = input.required<string>();
}
