import { Component, Input } from '@angular/core';
import { ControlContainer, FormGroupDirective } from '@angular/forms';
import { Property, PropertyType, SimpleValue } from '@wertzui/ngx-hal-client';

/**
 * A label that is generated from the given property.
 * If you also want an input element, you can either use {@link RestWorldFormElement} `<rw-form-element>` to have a label and an input rendered,
 * or use {@link RestWorldInput} `<rw-input>` which will just render the input element, so you can freely place this label.
 * @example
 * <rw-label [property]="property"></rw-label>
 */
@Component({
  selector: 'rw-label',
  templateUrl: './restworld-label.component.html',
  styleUrls: ['./restworld-label.component.css'],
  viewProviders: [{ provide: ControlContainer, useExisting: FormGroupDirective }]
})
export class RestWorldLabelComponent<TProperty extends Property<SimpleValue, string, string>> {
  /**
   * The property to be displayed.
   * @required
   **/
  @Input({ required: true })
  property!: TProperty;

  public get PropertyType() {
    return PropertyType;
  }
}
