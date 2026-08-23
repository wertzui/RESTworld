import { Component, forwardRef } from '@angular/core';
import { Property, PropertyType, SimpleValue } from '@wertzui/ngx-hal-client';
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
    public get PropertyType() {
        return PropertyType;
    }

    public get PropertyWithOptions() {
        return PropertyWithOptions<SimpleValue, string, string>;
    }
}
