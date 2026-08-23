import { Component, computed, contentChild, input, output, TemplateRef, viewChild } from '@angular/core';
import { ExtractGenericOptionsItemType, ExtractGenericOptionsSelectedValuesType, Options, Property, SimpleValue } from '@wertzui/ngx-hal-client';
import { Select, type SelectChangeEvent } from 'primeng/select';
import { MultiSelect } from 'primeng/multiselect';
import { Tooltip } from 'primeng/tooltip';
import { Chip } from 'primeng/chip';
import { FormField } from '@angular/forms/signals';
import { NgTemplateOutlet } from '@angular/common';
import { DropdownChangeEvent } from '../../../models/events';
import { PropertyTemplateContext } from '../../../models/templating';
import { debounce } from '../../../util/debounce';
import { OptionsManager, OptionsService } from '../../../services/options.service';
import { PropertySelectAttributes, PropertyAttributes } from '../../../directives/property.directives';
import { RestWorldSignalInputLazyLoadBaseComponent } from '../restworld-signal-inputs';

/**
 * A dropdown that is automatically created from the given property, using Signal Forms.
 * The dropdown supports searching through a RESTWorld list endpoint on the backend if the `link` of the options is set.
 * Otherwise the dropdown will use the `inline` of the options.
 * This is the Signal Forms equivalent of {@link RestWorldInputDropdownComponent} `<rw-input-dropdown>`.
 * @remarks It is advised to use {@link RestWorldSignalInputComponent} `<rw-signal-input>` and control the rendered inputs with the passed in property
 * instead of using this component directly.
 * @example
 * <rw-signal-input-dropdown [property]="property" [apiName]="apiName" [field]="field"></rw-signal-input-dropdown>
 */
@Component({
    selector: 'rw-signal-input-dropdown',
    templateUrl: './restworld-signal-input-dropdown.component.html',
    styleUrls: ['./restworld-signal-input-dropdown.component.css'],
    imports: [Select, MultiSelect, Tooltip, Chip, FormField, PropertySelectAttributes, PropertyAttributes, NgTemplateOutlet]
})
export class RestWorldSignalInputDropdownComponent<TProperty extends Property<SimpleValue, string, string> & { options: Options<SimpleValue, string, string> }, TOptionsItem extends ExtractGenericOptionsItemType<TProperty> = ExtractGenericOptionsItemType<TProperty>> extends RestWorldSignalInputLazyLoadBaseComponent<TProperty> {
    /**
     * A flag that indicates if the search should be case sensitive.
     * The default is false.
     */
    public readonly caseSensitive = input(false);
    /**
     * A function that returns the label for the given item.
     * The default returns the prompt and optionally the value in brackets.
     * The value in brackets will only be displayed if the `cols` field of the property is undefined or greater than 1.
     * Overwrite this function to change the label.
     * @param item The item to get the label for.
     */
    public readonly getLabel = input<(itemOrValue: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>) => string | undefined>();
    /**
     * A function that returns the tooltip for the given item.
     * The default returns all properties of the item except the ones that start with an underscore or the ones that are in the list of default properties to exclude.
     * The default properties to exclude are: createdAt, createdBy, lastChangedAt, lastChangedBy, timestamp, promptField, valueField.
     * Overwrite this function to change the tooltip.
     * @param item The item to get the label for.
     */
    public readonly getTooltip = input<(itemOrValue: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>) => string | undefined>();

    public readonly inputOptionsMultipleRef = contentChild<TemplateRef<PropertyTemplateContext>>("inputOptionsMultiple");
    public readonly inputOptionsSingleRef = contentChild<TemplateRef<PropertyTemplateContext>>("inputOptionsSingle");
    public readonly multiSelect = viewChild<MultiSelect>(MultiSelect);
    /**
     * An event that is emitted when the selected value changes.
     */
    public onChange = output<DropdownChangeEvent<TOptionsItem>>();
    public onOptionsFiltered = debounce(this.onOptionsFilteredInternal, 500);
    public readonly optionsManager: OptionsManager<TProperty, TOptionsItem>;

    // Unlike Reactive Forms, a Signal Forms `FieldTree`'s `value` is already a reactive signal, so it can be
    // used directly as the "current value" signal without an RxJS observable bridge.
    private readonly _value = computed(() => this.field()().value() as ExtractGenericOptionsSelectedValuesType<TProperty> | ExtractGenericOptionsSelectedValuesType<TProperty>[] | undefined);

    constructor(
        optionsService: OptionsService,
    ) {
        super();
        this.optionsManager = optionsService.getManager(this.apiName, this.property, this._value, this.getLabel, this.getTooltip);
    }

    public onOptionsChanged(event: SelectChangeEvent) {
        this.onChange.emit({ originalEvent: event.originalEvent!, value: event.value });
    }

    public async onOptionsFilteredInternal(event: { originalEvent: Event; filter: string | null }) {
        const options = this.optionsManager.options();
        const currentItems = this.optionsManager.items.value();

        if (!(event.filter) || event.filter === '')
            return;

        if (event.originalEvent.type === "input") {
            const inputEvent = (event.originalEvent as InputEvent);
            if (inputEvent.inputType === "insertFromPaste") {
                // If the user pasted in multiple ids as comma separated list, we want to get them all and set them as the selected value.

                var values = event.filter
                    .split(",")
                    .filter(v => v !== '')
                    .map(v => v.trim())
                    .map(v => {
                        const n = Number.parseFloat(v);
                        return Number.isNaN(n) ? this.makeUpperIfCaseInsensitive(v.toUpperCase(), false) : n;
                    });

                if (!values || values.length === 0)
                    return;

                const allAreNumbers = values.every(v => typeof v === "number" && !isNaN(v));
                const filter = allAreNumbers
                    ? `${options.valueField} in (${values.join(',')})`
                    : `contains(${this.makeUpperIfCaseInsensitive(options.promptField, true)}, '${values.join("', '")}')`;

                if ((options?.link?.href))
                    await this.optionsManager.updateItemsFromFilter(filter);

                if (currentItems) {
                    const selectedValues = currentItems
                        .map(i => this.optionsManager.getValue(i))
                        .filter(v => values.includes(v as unknown as string | number));
                    this.field()().value.set(selectedValues as ExtractGenericOptionsSelectedValuesType<TProperty>[] as any);
                    this.multiSelect()?.resetFilter();
                }
            }
            else {
                // This is the normal case where the user types in a filter.

                let filter = `contains(${this.makeUpperIfCaseInsensitive(options.promptField, true)}, '${this.makeUpperIfCaseInsensitive(event.filter, false)}')`;
                if (options.valueField?.toLowerCase() === 'id' && !Number.isNaN(Number.parseInt(event.filter)))
                    filter = `(${options.valueField} eq ${event.filter})  or (${filter})`;

                if ((options?.link?.href))
                    await this.optionsManager.updateItemsFromFilter(filter);
            }
        }
    }

    private makeUpperIfCaseInsensitive(filter: string | null | undefined, isOData: boolean): string | null | undefined {
        if (this.caseSensitive() || typeof filter !== "string")
            return filter;

        if (isOData)
            return `toupper(${filter})`;

        return filter.toUpperCase();
    }
}
