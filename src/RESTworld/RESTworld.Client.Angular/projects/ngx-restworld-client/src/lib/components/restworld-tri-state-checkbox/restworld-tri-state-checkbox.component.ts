/*
# PRIMENG LICENSES

## PRIMENG COMMUNITY VERSIONS LICENSE

The MIT License (MIT)

Copyright (c) 2016-2025 PrimeTek

Permission is hereby granted, free of charge, to any person obtaining a copy
of this software and associated documentation files (the "Software"), to deal
in the Software without restriction, including without limitation the rights
to use, copy, modify, merge, publish, distribute, sublicense, and/or sell
copies of the Software, and to permit persons to whom the Software is
furnished to do so, subject to the following conditions:

The above copyright notice and this permission notice shall be included in
all copies or substantial portions of the Software.

THE SOFTWARE IS PROVIDED "AS IS", WITHOUT WARRANTY OF ANY KIND, EXPRESS OR
IMPLIED, INCLUDING BUT NOT LIMITED TO THE WARRANTIES OF MERCHANTABILITY,
FITNESS FOR A PARTICULAR PURPOSE AND NONINFRINGEMENT. IN NO EVENT SHALL THE
AUTHORS OR COPYRIGHT HOLDERS BE LIABLE FOR ANY CLAIM, DAMAGES OR OTHER
LIABILITY, WHETHER IN AN ACTION OF CONTRACT, TORT OR OTHERWISE, ARISING FROM,
OUT OF OR IN CONNECTION WITH THE SOFTWARE OR THE USE OR OTHER DEALINGS IN
THE SOFTWARE.
*/

import { CommonModule } from '@angular/common';
import {
    AfterContentInit,
    booleanAttribute,
    ChangeDetectionStrategy,
    Component,
    forwardRef,
    inject,
    input,
    numberAttribute,
    output,
    TemplateRef,
    viewChild,
    ViewEncapsulation,
    contentChild,
    model,
    contentChildren,
    computed
} from '@angular/core';
import {
    ControlValueAccessor,
    FormControl,
    NG_VALUE_ACCESSOR,
    NgControl,
} from '@angular/forms';
import { PrimeTemplate, SharedModule } from 'primeng/api';
import { BaseComponent } from 'primeng/basecomponent';
import { CheckboxChangeEvent, CheckboxStyle } from 'primeng/checkbox';

export const TRI_STATE_CHECKBOX_VALUE_ACCESSOR: any = {
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => TriStateCheckbox),
    multi: true,
};
/**
 * Checkbox is an extension to standard checkbox element with theming.
 * @group Components
 */
@Component({
    selector: 'p-tri-state-checkbox, p-tri-state-checkBox, p-tri-state-check-box, rw-tri-state-checkbox, rw-tri-state-checkBox, rw-tri-state-check-box',
    standalone: true,
    imports: [CommonModule, SharedModule],
    template: `
        <div [style]="style()" [class]="styleClass()" [ngClass]="containerClass()" [attr.data-p-highlight]="model() === true" [attr.data-p-checked]="model() === true" [attr.data-p-disabled]="disabled()" [title]="model()">
            <input
                #input
                [attr.id]="inputId()"
                type="checkbox"
                [value]="model()"
                [attr.name]="name()"
                [checked]="model()"
                [attr.tabindex]="tabindex()"
                [disabled]="disabled()"
                [readonly]="readonly()"
                [attr.required]="required()"
                [attr.aria-labelledby]="ariaLabelledBy()"
                [attr.aria-label]="ariaLabel()"
                [style]="inputStyle()"
                [class]="inputClass()"
                [ngClass]="{ 'p-checkbox-input': true }"
                (focus)="onInputFocus($event)"
                (blur)="onInputBlur($event)"
                (change)="handleChange($event)"
            />
            <div class="p-checkbox-box">
              @if (!checkboxIconTemplate() && !_checkboxIconTemplate) {
                @if (model() === true) {
                  @if (checkboxIcon()) {
                        <span class="p-checkbox-icon" [ngClass]="checkboxIcon()" [attr.data-pc-section]="'icon'"></span>
                  }
                  @else {
                        <span class="p-checkbox-icon pi pi-check" [attr.data-pc-section]="'icon'"></span>
                  }
                }
                @else if (model() === null) {
                    <span class="p-checkbox-icon pi pi-minus" [attr.data-pc-section]="'icon'"></span>
                }
              }
                <ng-template *ngTemplateOutlet="checkboxIconTemplate() ?? _checkboxIconTemplate ?? null; context: { model: model, class: 'p-checkbox-icon' }"></ng-template>
            </div>
        </div>
    `,
    providers: [TRI_STATE_CHECKBOX_VALUE_ACCESSOR, CheckboxStyle],
    changeDetection: ChangeDetectionStrategy.OnPush,
    encapsulation: ViewEncapsulation.None,
})
export class TriStateCheckbox
    extends BaseComponent
    implements AfterContentInit, ControlValueAccessor {
    /**
     * Value of the checkbox.
     * @group Props
     */
    readonly value = input<any>();
    /**
     * Name of the checkbox group.
     * @group Props
     */
    readonly name = input<string>();
    /**
     * When present, it specifies that the element should be disabled.
     * @group Props
     */
    readonly disabled = model<boolean | undefined>(undefined);
    /**
     * Establishes relationships between the component and label(s) where its value should be one or more element IDs.
     * @group Props
     */
    readonly ariaLabelledBy = input<string>();
    /**
     * Used to define a string that labels the input element.
     * @group Props
     */
    readonly ariaLabel = input<string>();
    /**
     * Index of the element in tabbing order.
     * @group Props
     */
    readonly tabindex = input<number, unknown>(undefined, { transform: numberAttribute });
    /**
     * Identifier of the focus input to match a label defined for the component.
     * @group Props
     */
    readonly inputId = input<string>();
    /**
     * Inline style of the component.
     * @group Props
     */
    readonly style = input<{
        [klass: string]: any;
    } | null>();
    /**
     * Inline style of the input element.
     * @group Props
     */
    readonly inputStyle = input<{
        [klass: string]: any;
    } | null>();
    /**
     * Style class of the component.
     * @group Props
     */
    readonly styleClass = input<string>("");
    /**
     * Style class of the input element.
     * @group Props
     */
    readonly inputClass = input<string>("");
    /**
     * Defines the size of the component.
     * @group Props
     */
    readonly size = input<'large' | 'small'>();
    /**
     * Form control value.
     * @group Props
     */
    readonly formControl = input<FormControl>();
    /**
     * Icon class of the checkbox icon.
     * @group Props
     */
    readonly checkboxIcon = input<string | undefined>();
    /**
     * When present, it specifies that the component cannot be edited.
     * @group Props
     */
    readonly readonly = input<boolean, unknown>(undefined, { transform: booleanAttribute });
    /**
     * When present, it specifies that checkbox must be checked before submitting the form.
     * @group Props
     */
    readonly required = input<boolean, unknown>(undefined, { transform: booleanAttribute });
    /**
     * When present, it specifies that the component should automatically get focus on load.
     * @group Props
     */
    readonly autofocus = input<boolean, unknown>(undefined, { transform: booleanAttribute });

    readonly model = model<boolean | null>(null);
    /**
     * Specifies the input variant of the component.
     * @group Props
     */
    variant = input<'filled' | 'outlined'>('outlined');
    /**
     * Callback to invoke on value change.
     * @param {CheckboxChangeEvent} event - Custom value change event.
     * @group Emits
     */
    onChange = output<CheckboxChangeEvent>();
    /**
     * Callback to invoke when the receives focus.
     * @param {Event} event - Browser event.
     * @group Emits
     */
    onFocus = output<Event>();
    /**
     * Callback to invoke when the loses focus.
     * @param {Event} event - Browser event.
     * @group Emits
     */
    onBlur = output<Event>();

    inputViewChild = viewChild.required<HTMLInputElement>('input');

    readonly containerClass = computed(() => ({
        'p-checkbox p-component': true,
        'p-checkbox-checked p-highlight': this.model(),
        'p-disabled': this.disabled(),
        'p-variant-filled':
            this.variant() === 'filled' ||
            this.config.inputStyle() === 'filled' ||
            this.config.inputVariant() === 'filled',
        'p-checkbox-sm p-inputfield-sm': this.size() === 'small',
        'p-checkbox-lg p-inputfield-lg': this.size() === 'large',
    }));

    /**
     * The template of the checkbox icon.
     * @group Templates
     */
    readonly checkboxIconTemplate = contentChild<TemplateRef<any>>('checkboxicon', { descendants: false });

    readonly templates = contentChildren(PrimeTemplate);

    _checkboxIconTemplate: TemplateRef<any> | undefined;

    onModelChange: Function = () => { };

    onModelTouched: Function = () => { };

    focused: boolean = false;

    _componentStyle = inject(CheckboxStyle);

    ngAfterContentInit() {
        this.templates().forEach((item) => {
            switch (item.getType()) {
                case 'icon':
                    this._checkboxIconTemplate = item.template;
                    break;
                case 'checkboxicon':
                    this._checkboxIconTemplate = item.template;
                    break;
            }
        });
    }

    updateModel(event: Event) {
        let newModelValue;

        /*
         * When `formControlName` or `formControl` is used - `writeValue` is not called after control changes.
         * Otherwise it is causing multiple references to the actual value: there is one array reference inside the component and another one in the control value.
         * `selfControl` is the source of truth of references, it is made to avoid reference loss.
         * */
        const selfControl = this.injector.get<NgControl | null>(NgControl, null, {
            optional: true,
            self: true,
        });

        const currentModelValue =
            selfControl && !this.formControl() ? selfControl.value : this.model;

        newModelValue = this.getNextValue(this.model());
        this.model.set(newModelValue);
        this.onModelChange(newModelValue);

        this.onChange.emit({ checked: newModelValue, originalEvent: event });
    }

    private getNextValue(currentValue: boolean | null): boolean | null {
        switch (currentValue) {
            case true:
                return false;
            case false:
                return this.required() ? true : null;
            default:
                return true;
        }
    }

    handleChange(event: Event) {
        if (!this.readonly()) {
            this.updateModel(event);
        }
    }

    onInputFocus(event: Event) {
        this.focused = true;
        this.onFocus.emit(event);
    }

    onInputBlur(event: Event) {
        this.focused = false;
        this.onBlur.emit(event);
        this.onModelTouched();
    }

    focus() {
        this.inputViewChild().focus();
    }

    writeValue(model: any): void {
        this.model.set(model);
        this.cd.markForCheck();
    }

    registerOnChange(fn: Function): void {
        this.onModelChange = fn;
    }

    registerOnTouched(fn: Function): void {
        this.onModelTouched = fn;
    }

    setDisabledState(val: boolean): void {
        setTimeout(() => {
            this.disabled.set(val);
            this.cd.markForCheck();
        });
    }
}
