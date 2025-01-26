import { Component, ElementRef, ViewChildren, computed, input } from '@angular/core';
import { AbstractControl, ControlContainer, FormGroup, FormGroupDirective } from '@angular/forms';
import { Property, PropertyType, SimpleValue } from '@wertzui/ngx-hal-client';
import { ValidationErrorDirective, ValidationErrorsComponent } from 'ngx-valdemort';
import { MessageModule } from 'primeng/message';

/**
 * Displays validation errors either for one property or for a whole form.
 * If a property is given, only the validation errors for this property will be displayed.
 * If a form is given, all validation errors for all properties will be displayed.
 * @example
 * <rw-validation-errors [property]="property"></rw-validation-errors>
 * <rw-validation-errors [form]="form"></rw-validation-errors>
 */
@Component({
    selector: 'rw-validation-errors',
    templateUrl: './restworld-validation-errors.component.html',
    styleUrls: ['./restworld-validation-errors.component.css'],
    standalone: true,
    viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }],
    imports: [ValidationErrorsComponent, ValidationErrorDirective, MessageModule]
})
export class RestWorldValidationErrorsComponent<T extends { [K in keyof T]: AbstractControl<any, any>; }> {
    /**
     * The form to display the validation errors for.
     * Either set this or the property input.
     */
    public readonly form = input<FormGroup<T>>();
    public readonly name = computed(() => this.property()?.name ?? null);
    public readonly prompt = computed(() => this.property()?.prompt ?? this.name() ?? null);
    /**
     * The property to display the validation errors for.
     * Either set this or the form input.
     */
    public readonly property = input<Property<SimpleValue, string, string>>();

    public get PropertyType() {
        return PropertyType;
    }
}
