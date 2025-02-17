import { Component, computed, effect, input, untracked, type OnDestroy } from '@angular/core';
import { Template, FormService, type Property, type SimpleValue, type ExtractValueType, type PropertyDto } from "@wertzui/ngx-hal-client";
import type { FilterMetadata } from "primeng/api";
import type { Subscription } from "rxjs";
import { RestWorldInputComponent } from "../restworld-inputs/restworld-inputs";
import { FormGroup, ReactiveFormsModule } from "@angular/forms";

/**
 * This component is used to display a filter element for a table column.
 * It is used internally by the <rw-table> component, but can also be used in the #filter template of a column in the header of a <p-table>.
 */
@Component({
    selector: 'rw-table-column-filter-element',
    imports: [RestWorldInputComponent, ReactiveFormsModule],
    templateUrl: './rest-world-table-column-filter-element.component.html',
    styleUrl: './rest-world-table-column-filter-element.component.css'
})
export class RestWorldTableColumnFilterElementComponent<TProperty extends Property<SimpleValue, string, string>> implements OnDestroy {
    /**
     * The filter constraint to update when the value changes.
     * This is coming from the $context of the #filter template
     */
    public readonly filterConstraint = input.required<FilterMetadata>();
    /**
     * The HAL-Forms property to filter by.
     * This is normally the column.
     */
    public readonly property = input.required<TProperty>();
    /**
     * The name of the API to use when generating dropdowns.
     */
    public readonly apiName = input.required<string>();
    /**
     * The initial value of the filter.
     */
    public readonly value = input.required<ExtractValueType<TProperty> | null>();

    public readonly form = computed<FormGroup>(() => this._formService.createFormGroupFromTemplate(this.template()));

    private readonly template = computed(() => new Template({
        properties: [this.property() as PropertyDto<SimpleValue, string, string>],
    }))

    private _formValueChangesSubscription?: Subscription;

    constructor(private readonly _formService: FormService) {
        effect(() => {
            this._formValueChangesSubscription?.unsubscribe();
            const form = this.form();
            const property = this.property();
            const value = untracked(() => this.value());
            const formControl = form.controls[property.name];
            this._formValueChangesSubscription = formControl.valueChanges.subscribe(this.setFilterValue.bind(this));
            formControl.setValue(value);
        });
    }

    private setFilterValue(value: ExtractValueType<TProperty> | null): void {
        this.filterConstraint().value = value;
    }

    public ngOnDestroy(): void {
        this._formValueChangesSubscription?.unsubscribe();
    }
}
