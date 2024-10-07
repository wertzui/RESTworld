import { Component, ElementRef, Input, ViewChildren } from '@angular/core';
import { AbstractControl, ControlContainer, FormGroup, FormGroupDirective } from '@angular/forms';
import { Property, PropertyType, SimpleValue } from '@wertzui/ngx-hal-client';
import { ValidationErrorsComponent } from 'ngx-valdemort';

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
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }]
})
export class RestWorldValidationErrorsComponent<T extends { [K in keyof T]: AbstractControl<any, any>; }> {
  @Input()
  property?: Property<SimpleValue, string, string>;

  @Input()
  form?: FormGroup<T>

  @ViewChildren(ValidationErrorsComponent)
  validationErrorsComponents!: ValidationErrorsComponent[];

  public get PropertyType() {
    return PropertyType;
  }
}
