import { Component, forwardRef } from '@angular/core';
import { RestWorldLabelComponent } from '../../restworld-label/restworld-label.component';
import { RestWorldSignalInputComponent } from '../restworld-signal-input/restworld-signal-input.component';
import { RestWorldSignalInputLazyLoadBaseComponent } from '../restworld-signal-inputs';

/**
 * A form element with a label that is automatically created from a property in a form template, using Signal Forms.
 * This is the Signal Forms equivalent of {@link RestWorldFormElementComponent} `<rw-form-element>`.
 * If you want a form element without a label, use {@link RestWorldSignalInputComponent} `<rw-signal-input>`.
 * @example
 * <rw-signal-form-element [property]="property" [apiName]="apiName" [field]="field"></rw-signal-form-element>
 * @remarks `RestWorldSignalInputComponent` is wrapped in `forwardRef()` because it closes an import cycle:
 * rw-signal-form-element -> rw-signal-input -> rw-signal-input-object -> rw-signal-input-template ->
 * rw-signal-form-element.
 */
@Component({
    selector: 'rw-signal-form-element',
    templateUrl: './restworld-signal-form-element.component.html',
    styleUrls: ['./restworld-signal-form-element.component.css'],
    imports: [forwardRef(() => RestWorldSignalInputComponent), RestWorldLabelComponent]
})
export class RestWorldSignalFormElementComponent extends RestWorldSignalInputLazyLoadBaseComponent {
}
