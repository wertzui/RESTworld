import { Component, computed, input } from '@angular/core';
import type { FieldTree, ValidationError } from '@angular/forms/signals';
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

    /**
     * The human-readable messages for all current errors.
     */
    public readonly messages = computed(() => this.errors().map(error => this.toMessage(error)));

    public get PropertyType() {
        return PropertyType;
    }

    /**
     * Checks if the error carries the given detail (e.g. `min` for a `min` error).
     * @remarks The errors are typed as the generic `ValidationError`, the details are only present on the specific
     * error classes (and on custom errors of the same kind), so they are checked at runtime.
     */
    private static hasDetail<TKey extends string>(error: ValidationError, key: TKey): error is ValidationError & Record<TKey, unknown> {
        return key in error;
    }

    private toMessage(error: ValidationError): string {
        const prompt = this.prompt();

        switch (error.kind) {
            case 'required':
                return `The field '${prompt}' is required.`;
            case 'email':
                return `'${prompt}' must be a valid email address.`;
            case 'min':
                if (RestWorldSignalValidationErrorsComponent.hasDetail(error, 'min'))
                    return `'${prompt}' must be greater than or equal to ${String(error.min)}.`;
                break;
            case 'max':
                if (RestWorldSignalValidationErrorsComponent.hasDetail(error, 'max'))
                    return `'${prompt}' must be smaller than or equal to ${String(error.max)}.`;
                break;
            case 'minLength':
                if (RestWorldSignalValidationErrorsComponent.hasDetail(error, 'minLength'))
                    return `The length of '${prompt}' must be equal or greater than or equal to ${String(error.minLength)}.`;
                break;
            case 'maxLength':
                if (RestWorldSignalValidationErrorsComponent.hasDetail(error, 'maxLength'))
                    return `The length of '${prompt}' must be shorter than or equal to ${String(error.maxLength)}.`;
                break;
            case 'pattern':
                if (RestWorldSignalValidationErrorsComponent.hasDetail(error, 'pattern'))
                    return `The value for '${prompt}' does not match the pattern ${String(error.pattern)}.`;
                break;
        }

        return error.message ?? error.kind;
    }
}
