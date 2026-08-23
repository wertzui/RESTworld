import { Component, computed, input } from '@angular/core';
import type { FieldTree } from '@angular/forms/signals';
import { Property, PropertyType, SimpleValue } from '@wertzui/ngx-hal-client';
import { MessageModule } from 'primeng/message';

/**
 * Displays Signal Forms validation errors for a single property.
 * This is the Signal Forms equivalent of {@link RestWorldValidationErrorsComponent} `<rw-validation-errors>`.
 * @example
 * <rw-signal-validation-errors [field]="form.name" [property]="property"></rw-signal-validation-errors>
 */
@Component({
    selector: 'rw-signal-validation-errors',
    templateUrl: './restworld-signal-validation-errors.component.html',
    styleUrls: ['./restworld-signal-validation-errors.component.css'],
    imports: [MessageModule]
})
export class RestWorldSignalValidationErrorsComponent<TValue = unknown> {
    /**
     * The `FieldTree` to display the validation errors for.
     * @required
     */
    public readonly field = input.required<FieldTree<TValue>>();

    /**
     * The property to display the validation errors for.
     * Used to render a human-readable field name in the default error messages.
     */
    public readonly property = input<Property<SimpleValue, string, string>>();

    public readonly name = computed(() => this.property()?.name ?? null);
    public readonly prompt = computed(() => this.property()?.prompt ?? this.name() ?? null);

    private readonly _state = computed(() => this.field()());
    public readonly errors = computed(() => this._state().errors());
    public readonly showErrors = computed(() => this._state().touched() && this._state().invalid());

    public get PropertyType() {
        return PropertyType;
    }
}
