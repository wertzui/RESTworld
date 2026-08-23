import { Component } from '@angular/core';
import { FormField } from '@angular/forms/signals';
import { PropertyType } from '@wertzui/ngx-hal-client';
import { InputText } from 'primeng/inputtext';
import { InputNumber } from 'primeng/inputnumber';
import { DatePicker } from 'primeng/datepicker';
import { TriStateCheckbox } from '../../restworld-tri-state-checkbox/restworld-tri-state-checkbox.component';
import { RestWorldImageComponent } from '../../restworld-image/restworld-image.component';
import { RestWorldFileComponent } from '../../restworld-file/restworld-file.component';
import { PropertyWithImage } from '../../../models/special-properties';
import { PropertyAttributes, PropertyInputNumberAttributes } from '../../../directives/property.directives';
import { RestWorldSignalInputBaseComponent } from '../restworld-signal-inputs';

/**
 * A simple input element, like a string, a number or a Date that is automatically created from the given property, using Signal Forms.
 * This is the Signal Forms equivalent of {@link RestWorldInputSimpleComponent} `<rw-input-simple>`.
 * @remarks It is advised to use {@link RestWorldSignalInputComponent} `<rw-signal-input>` and control the rendered inputs with the passed in property
 * instead of using this component directly.
 * @example
 * <rw-signal-input-simple [property]="property" [field]="field"></rw-signal-input-simple>
 */
@Component({
    selector: 'rw-signal-input-simple',
    templateUrl: './restworld-signal-input-simple.component.html',
    styleUrls: ['./restworld-signal-input-simple.component.css'],
    imports: [FormField, InputText, InputNumber, DatePicker, TriStateCheckbox, RestWorldImageComponent, RestWorldFileComponent, PropertyAttributes, PropertyInputNumberAttributes]
})
export class RestWorldSignalInputSimpleComponent extends RestWorldSignalInputBaseComponent {
    private static readonly _dateFormat = new Date(3333, 10, 22) // months start at 0 in JS
        .toLocaleDateString()
        .replace("22", "dd")
        .replace("11", "mm")
        .replace("3333", "yy")
        .replace("33", "y");
    private static readonly _timeFormat = new Date(1, 1, 1, 22, 33, 44)
        .toLocaleTimeString()
        .replace("22", "hh")
        .replace("33", "mm")
        .replace("44", "ss");

    public get PropertyType() {
        return PropertyType;
    }

    public get PropertyWithImage() {
        return PropertyWithImage;
    }

    public get dateFormat(): string {
        return RestWorldSignalInputSimpleComponent._dateFormat;
    }

    public get timeFormat() {
        return RestWorldSignalInputSimpleComponent._timeFormat;
    }
}
