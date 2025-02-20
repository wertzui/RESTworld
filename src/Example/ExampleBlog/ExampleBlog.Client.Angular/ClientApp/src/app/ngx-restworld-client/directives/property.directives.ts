import { Directive, Host, Inject, input, Optional, Self, SkipSelf, ElementRef, type OnChanges, Renderer2, type SimpleChanges, effect, forwardRef, NgModule, computed } from "@angular/core";
import { FormControlName, NG_ASYNC_VALIDATORS, NG_VALIDATORS, NG_VALUE_ACCESSOR, type AsyncValidator, type AsyncValidatorFn, ControlContainer, type ControlValueAccessor, type Validator, type ValidatorFn, DefaultValueAccessor, CheckboxControlValueAccessor, SelectControlValueAccessor, RangeValueAccessor, NumberValueAccessor, NgControl, NgControlStatus, ReactiveFormsModule } from "@angular/forms";
import type { Property, SimpleValue } from "@wertzui/ngx-hal-client";
import { InputNumber } from "primeng/inputnumber";
import { MultiSelect } from "primeng/multiselect";
import { Select } from "primeng/select";

/**
 * This directive sets the attributes of a p-inputNumber element based on the property.
 * It sets the mode, minFractionDigits and maxFractionDigits attributes.
 * Thses are based on the properties step value.
 */
@Directive({
    selector: "p-inputNumber[formControlProperty], p-inputNumber[propertyAttributes]",
})
export class PropertyInputNumberAttributes<TProperty extends Property<SimpleValue, string, string>> {
    public readonly formControlProperty = input<TProperty>();
    public readonly propertyAttributes = input<TProperty>();

    constructor(
        @Self() inputNumber: InputNumber
    ) {
        // We need to set it to the maximum value in the constructor, because for some reason lowering it in the effect works, but raising it does not.
        inputNumber.maxFractionDigits = 100;

        effect(() => {
            const property = this.formControlProperty() ?? this.propertyAttributes();
            if (!property) {
                console.error("Cannot create an instance of the PropertyInputNumberAttributes directive without a property.");
                return;
            }

            const element = inputNumber;
            if (!element) {
                console.error(`Cannot create an instance of the PropertyInputNumberAttributes directive for the property "${property.name}" without a p-inputNumber element.`);
                return;
            }

            element.showButtons = !property.readOnly;
            element.minFractionDigits = 0;
            if (property.step)
                element.step = property.step;
            if (!property.step || property.step < 0) {
                element.mode = "decimal";
            }
            else if (property.step && property.step >= 1) {
                element.maxFractionDigits = PropertyInputNumberAttributes.countFractionalDigits(property.value);
            }
        });
    }

    private static countFractionalDigits(value: unknown): number {
        if (value === null || value === undefined || typeof value !== "number")
            return 0;

        let count = 0;
        while (!Number.isInteger(value) && count < 100) { // Prevent infinite loop due to precision issues
            value *= 10;
            count++;
        }

        return count;
    }
}

/**
 * This directive sets the attributes of a p-select or p-multiSelect element based on the property.
 * It sets the optionLabel, optionValue, readonly, filterBy, filter, showClear, required, filterPlaceholder, appendTo, display, selectionLimit, showToggleAll attributes.
 * You need to set either the property or the propertyAttributes input.
 * If both are set, the property input will be used.
 * The propertyAttributes can be used if you want to set the attributes of an input element based on a property without binding the property to a form control using the propertyFormControl directive.
 * @example
 * <p-select [formControlProperty]="property" />
 * <p-select [propertyAttributes]="property" />
 * <p-multiSelect [formControlProperty]="property" />
 * <p-multiSelect [propertyAttributes]="property" />
 */
@Directive({
    selector: "p-select[formControlProperty], p-select[propertyAttributes], p-multiSelect[formControlProperty], p-multiSelect[propertyAttributes]",
})
export class PropertySelectAttributes<TProperty extends Property<SimpleValue, string, string>> {
    /**
     * The property to display.
     */
    public readonly formControlProperty = input<TProperty>();
    public readonly propertyAttributes = input<TProperty>();

    constructor(
        @Self() @Optional() select?: Select,
        @Self() @Optional() multiSelect?: MultiSelect
    ) {
        effect(() => {
            const property = this.formControlProperty() ?? this.propertyAttributes();
            if (!property) {
                console.error("Cannot create an instance of the PropertySelectControl directive without a property.");
                return;
            }

            const options = property.options;
            if (!options) {
                console.error(`Cannot create an instance of the PropertySelectControl directive for the property "${property.name}" without options.`);
                return;
            }

            const element = select ?? multiSelect;
            if (!element) {
                console.error(`Cannot create an instance of the PropertySelectControl directive for the property "${property.name}" without a p-select or p-multiSelect element.`);
                return;
            }

            const promptField = options.promptField ?? "prompt";
            const valueField = options.valueField ?? "value";
            element.optionLabel = promptField;
            element.optionValue = valueField;
            element.readonly = property.readOnly;
            element.filterBy = promptField + "," + valueField;
            element.filter = true;
            element.showClear = !property.required || (options.minItems ?? 0) <= 0;
            element.appendTo = "body";

            if (element instanceof Select) {
                element.required = property.required || (options.minItems ?? 0) > 0;
                element.filterPlaceholder = options.link?.href ? "search for more results" : ""
            }
            else if (element instanceof MultiSelect) {
                // It is missspelled in the primeng component using an uppercase H
                element.filterPlaceHolder = options.link?.href ? "search for more results" : ""
                element.display = "chip";
                element.selectionLimit = options.maxItems;
                element.showToggleAll = true;
            }
        });
    }

}

/**
 * This directive sets the attributes of an input element based on the property.
 * It sets the id, name, placeholder, type, max, min, step, required, readOnly, cols, rows, maxLength, minLength attributes.
 * It also adds the p-disabled class if the property is readOnly.
 * You need to set either the property or the propertyAttributes input.
 * If both are set, the property input will be used.
 * The propertyAttributes can be used if you want to set the attributes of an input element based on a property without binding the property to a form control using the propertyFormControl directive.
 * @example
 * <input [formControlProperty]="property" />
 * <input [propertyAttributes]="property" />
 */
@Directive({
    selector: "[formControlProperty],[propertyAttributes]",
})
export class PropertyAttributes<TProperty extends Property<SimpleValue, string, string>> {
    public readonly formControlProperty = input<TProperty>();
    public readonly propertyAttributes = input<TProperty>();

    constructor(
        private readonly elementRef: ElementRef,
        private readonly renderer: Renderer2
    ) {
        effect(() => {
            const property = this.formControlProperty() ?? this.propertyAttributes();
            if (!property) {
                return;
            }

            const nativeElement = this.elementRef.nativeElement;
            if (!(nativeElement instanceof HTMLElement))
                return;

            this.renderer.setAttribute(nativeElement, "id", property.name);
            this.renderer.setAttribute(nativeElement, "name", property.name);
            if (property.placeholder)
                this.renderer.setAttribute(nativeElement, "placeholder", property.placeholder as string);
            if (property.type)
                this.renderer.setAttribute(nativeElement, "type", property.type);
            if (property.max)
                this.renderer.setAttribute(nativeElement, "max", property.max.toString());
            if (property.min)
                this.renderer.setAttribute(nativeElement, "min", property.min.toString());
            if (property.step)
                this.renderer.setAttribute(nativeElement, "step", property.step.toString());
            if (property.required)
                this.renderer.setAttribute(nativeElement, "required", "true");
            if (property.readOnly)
                this.renderer.addClass(nativeElement, "p-disabled");
            if (property.cols)
                this.renderer.setAttribute(nativeElement, "cols", property.cols.toString());
            if (property.rows)
                this.renderer.setAttribute(nativeElement, "rows", property.rows.toString());
            if (property.maxLength)
                this.renderer.setAttribute(nativeElement, "maxlength", property.maxLength.toString());
            if (property.minLength)
                this.renderer.setAttribute(nativeElement, "minlength", property.minLength.toString());
        });
    }
}

/**
 * This directive binds a property to a form control.
 * It sets the name of the form control to the name of the property and acts just like [formControlName]="myProperty().name".
 * @example
 * <input [formControlProperty]="property" />
 */
@Directive({
    selector: "[formControlProperty]:not([useTemplateDrivenForms=true])",
    providers: [
        {
            provide: NgControl,
            useExisting: forwardRef(() => FormControlProperty),
        },
    ],
})
export class FormControlProperty<TProperty extends Property<SimpleValue, string, string>> extends FormControlName implements OnChanges {
    public readonly formControlProperty = input.required<TProperty>();

    private _propertyAdded = false;

    constructor(
        @Optional() @Host() @SkipSelf() parent: ControlContainer,
        @Optional() @Self() @Inject(NG_VALIDATORS) validators: (Validator | ValidatorFn)[],
        @Optional() @Self() @Inject(NG_ASYNC_VALIDATORS) asyncValidators: (AsyncValidator | AsyncValidatorFn)[],
        @Optional() @Self() @Inject(NG_VALUE_ACCESSOR) valueAccessors: ControlValueAccessor[],
    ) {
        super(parent, validators, asyncValidators, valueAccessors, null);

        effect(() => {
            const property = this.formControlProperty();
            if (!property) {
                return;
            }

            this.name = property.name;
        });
    }

    public override ngOnChanges(changes: SimpleChanges): void {
        // We cannot purely rely on the effect, because FormControlName used ngOnChanges which runs before any effects.
        // FormControlName reads this.name in ngOnChanges, so we need to set it here to make sure it is set before FormControlName reads it.
        if (!this._propertyAdded) {
            this.name = this.formControlProperty().name;
            this._propertyAdded = true;
        }

        super.ngOnChanges(changes);
    }
}

@Directive({
    selector: "input:not([type=checkbox])[formControlProperty], textarea[formControlProperty], select[formControlProperty]",
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => DefaultPropertyValueAccessor),
            multi: true,
        },
    ],
})
export class DefaultPropertyValueAccessor extends DefaultValueAccessor {
}

@Directive({
    selector: "input[type=checkbox][formControlProperty]",
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => CheckboxPropertyValueAccessor),
            multi: true,
        },
    ],
})
export class CheckboxPropertyValueAccessor extends CheckboxControlValueAccessor {
}


@Directive({
    selector: "select:not([multiple])[formControlProperty]",
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => SelectPropertyValueAccessor),
            multi: true,
        },
    ],
})
export class SelectPropertyValueAccessor extends SelectControlValueAccessor {
}

@Directive({
    selector: "input[type=range][formControlProperty]",
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => RangePropertyValueAccessor),
            multi: true,
        },
    ],
})
export class RangePropertyValueAccessor extends RangeValueAccessor {
}

@Directive({
    selector: "input[type=number][formControlProperty]",
    providers: [
        {
            provide: NG_VALUE_ACCESSOR,
            useExisting: forwardRef(() => NumberPropertyValueAccessor),
            multi: true,
        },
    ],
})
export class NumberPropertyValueAccessor extends NumberValueAccessor {
}

export const ngControlStatusHost = {
    "[class.ng-untouched]": "isUntouched",
    "[class.ng-touched]": "isTouched",
    "[class.ng-pristine]": "isPristine",
    "[class.ng-dirty]": "isDirty",
    "[class.ng-valid]": "isValid",
    "[class.ng-invalid]": "isInvalid",
    "[class.ng-pending]": "isPending",
};

@Directive({
    selector: "[formControlProperty]",
    host: ngControlStatusHost
})
export class PropertyControlStatus extends NgControlStatus {
    constructor(@Self() cd: NgControl) {
        super(cd);
    }
}

@NgModule({
    imports: [FormControlProperty, DefaultPropertyValueAccessor, CheckboxPropertyValueAccessor, SelectPropertyValueAccessor, RangePropertyValueAccessor, NumberPropertyValueAccessor, PropertyControlStatus, PropertySelectAttributes, PropertyAttributes, PropertyInputNumberAttributes, ReactiveFormsModule],
    exports: [FormControlProperty, DefaultPropertyValueAccessor, CheckboxPropertyValueAccessor, SelectPropertyValueAccessor, RangePropertyValueAccessor, NumberPropertyValueAccessor, PropertyControlStatus, PropertySelectAttributes, PropertyAttributes, PropertyInputNumberAttributes, ReactiveFormsModule],
})
export class HalFormsModule { }
