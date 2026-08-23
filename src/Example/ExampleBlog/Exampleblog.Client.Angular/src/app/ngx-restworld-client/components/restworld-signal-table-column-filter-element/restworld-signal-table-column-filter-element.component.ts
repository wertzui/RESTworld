import { Component, Injector, computed, effect, forwardRef, inject, input, runInInjectionContext, signal, untracked } from '@angular/core';
import type { FieldTree } from '@angular/forms/signals';
import { Template, SignalFormService, type Property, type SimpleValue, type ExtractValueType, type PropertyDto, type SignalForm } from "@wertzui/ngx-hal-client";
import type { FilterMetadata } from "primeng/api";
import { RestWorldSignalInputComponent } from "../restworld-signal-inputs/restworld-signal-input/restworld-signal-input.component";

/**
 * This component is used to display a filter element for a table column, using Signal Forms.
 * This is the Signal Forms equivalent of {@link RestWorldTableColumnFilterElementComponent} `<rw-table-column-filter-element>`.
 * It is used internally by the `<rw-signal-table>` component, but can also be used in the #filter template of a column in the header of a `<p-table>`.
 * @remarks `RestWorldSignalInputComponent` is wrapped in `forwardRef()` because `<rw-signal-input-collection>`
 * now nests `<rw-signal-table>` (in bound mode), which closes an import cycle back to this component:
 * rw-signal-table -> rw-signal-table-column-filter-element -> rw-signal-input -> rw-signal-input-collection ->
 * rw-signal-table.
 */
@Component({
    selector: 'rw-signal-table-column-filter-element',
    imports: [forwardRef(() => RestWorldSignalInputComponent)],
    templateUrl: './restworld-signal-table-column-filter-element.component.html',
    styleUrl: './restworld-signal-table-column-filter-element.component.css'
})
export class RestWorldSignalTableColumnFilterElementComponent<TProperty extends Property<SimpleValue, string, string>> {
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

    /**
     * The signal form (model signal + field tree) created from a single-property template built from
     * {@link property}. Rebuilt whenever `property()` changes.
     * @remarks `form()` (used internally by {@link SignalFormService.createSignalFormFromTemplate}) internally
     * calls both `inject()` and `effect()`, so this must be a plain `signal()` populated from a constructor
     * `effect()` wrapped in both `untracked()` (to escape this effect's own reactive context, satisfying the
     * effect()-inside-effect assertion) and `runInInjectionContext()` (for the `inject()` call) - see
     * `RestWorldSignalFormComponent`'s `signalForm` for the same pattern with more detail.
     */
    private readonly _signalForm = signal<SignalForm<Record<string, unknown>> | undefined>(undefined);

    /**
     * The `FieldTree` for the property, to bind to `[formField]` via `<rw-signal-input>`.
     */
    public readonly field = computed(() => {
        const signalForm = this._signalForm();
        const property = this.property();
        if (!signalForm)
            return undefined;

        return (signalForm.form as unknown as Record<string, FieldTree<ExtractValueType<TProperty>>>)[property.name];
    });

    private readonly _injector = inject(Injector);

    constructor(private readonly _signalFormService: SignalFormService) {
        // (Re-)build the signal form whenever the property changes, seeding it with the current filter value.
        effect(() => {
            const property = this.property();
            const value = untracked(() => this.value());

            const template = new Template({
                properties: [property as PropertyDto<SimpleValue, string, string>],
            });

            // Only override the model with `value` when it is set. When it is `null` (no filter entered yet),
            // we deliberately omit the override so `createSignalFormFromTemplate` falls back to its own
            // property-type-aware default (via `SignalFormService`'s internal `getSimplePropertyValue`),
            // which correctly defaults text-like properties to `''` instead of `null`. Native `<input>`
            // elements bound to Signal Forms treat a `null` model value as "number-like" (see
            // `getNativeControlValue` in `@angular/forms/signals`), which would otherwise cause every
            // keystroke in a text filter to be rejected as a parse error and the value would never update.
            const signalForm = untracked(() => runInInjectionContext(this._injector, () => value !== null
                ? this._signalFormService.createSignalFormFromTemplate(template, { [property.name]: value })
                : this._signalFormService.createSignalFormFromTemplate(template)));
            this._signalForm.set(signalForm as SignalForm<Record<string, unknown>>);
        });

        // Push the model's value to the PrimeNG filter constraint whenever it changes.
        effect(() => {
            const signalForm = this._signalForm();
            const property = this.property();
            if (!signalForm)
                return;

            const currentValue = signalForm.model()[property.name] as ExtractValueType<TProperty> | null;
            this.filterConstraint().value = currentValue;
        });
    }
}
