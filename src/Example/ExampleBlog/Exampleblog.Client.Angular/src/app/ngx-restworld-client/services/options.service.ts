import { computed, Injectable, resource, signal, type Signal } from "@angular/core";
import { RestWorldClientCollection } from "./restworld-client-collection";
import { type ExtractGenericOptionsItemType, type ExtractGenericOptionsSelectedValuesType, type Options, type Property, type SimpleValue } from "@wertzui/ngx-hal-client";
import { HttpHeaders } from "@angular/common/http";
import { NgHttpCachingHeaders } from "ng-http-caching";
import { ProblemService } from "./problem.service";

/**
 * This manager can be used to load the options for a property and to manage the selected values.
 * It is normally instantiated by the OptionsService.
 * This manager is normally used in combination with <p-select> and <p-multiSelect> components.
 * It is used internally by the <rw-dropdown> component.
 */
export class OptionsManager<TProperty extends Property<SimpleValue, string, string> & { options: Options<SimpleValue, string, string> }, TOptionsItem extends ExtractGenericOptionsItemType<TProperty> = ExtractGenericOptionsItemType<TProperty>> {
    public readonly getLabel = computed(() => this._getLabel() ?? ((itemOrValue: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>) => this.getDefaultLabel(itemOrValue)));
    public readonly getTooltip = computed(() => this._getTooltip() ?? ((itemOrValue: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>) => this.getDefaultTooltip(itemOrValue)));
    // public readonly items = linkedSignal(() => this.options()?.inline as TOptionsItem[] ?? []);
    public readonly loading = computed(() => this.items.isLoading());
    public readonly options = computed(() => this.property().options);
    public readonly promptField = computed(() => this.options()?.promptField ?? "prompt");
    public readonly valueField = computed(() => this.options()?.valueField ?? "value");

    private readonly _client = computed(() => this._clients.getClient(this.apiName()));
    public readonly selectedItems = computed(() => this.selectedValues()?.map(value => this.getItemByValue(this.items.value(), value as ExtractGenericOptionsSelectedValuesType<TProperty>) ?? []));

    public readonly selectedValues = computed(() => {
        const values = this._selectedValues();

        if (values === null || values === undefined)
            return [];

        return (Array.isArray(values) ? values : [values]) as ExtractGenericOptionsSelectedValuesType<TProperty>[];
    });

    public readonly filter = signal<string | undefined>(undefined);
    private _lastFilter: string | undefined = undefined;
    public readonly items = resource({
        params: () => ({ filter: this.filter(), options: this.options(), selectedValues: this.selectedValues() }),
        loader: async ({ params }) => {
            const options = params.options;
            let filter = params.filter;
            const selectedValues = params.selectedValues;

            // We have inline options, so we don't need to load them from the server.
            if (Array.isArray(options?.inline) && options.inline.length > 0)
                return options.inline as TOptionsItem[];

            // First load: there might be selected items, but the user has not typed in a filter yet.
            // In this case, we want to load the selected items to be able to show them in the dropdown.
            if (filter === undefined && selectedValues.length > 0)
                filter = `${options.valueField} in (${selectedValues})`;

            // Only make the call if the filter has changed.
            // This prevents unnecessary calls when the selected values change but the filter is the same.
            if (filter === this._lastFilter) {
                const currentItems = this.items.value() as TOptionsItem[] | undefined;
                if (currentItems === undefined)
                    return [] as TOptionsItem[];

                return currentItems;
            }

            this._lastFilter = filter;

            const templatedUri = options.link?.href;
            if (!templatedUri) {
                this._problemService.displayToast(undefined, `The property ${this.property().name} does not have options with a link href.`);
                return [] as TOptionsItem[];
            }

            const headersWithCaching = new HttpHeaders({[NgHttpCachingHeaders.ALLOW_CACHE]: "1", [NgHttpCachingHeaders.LIFETIME]: "3000"});
            const response = await this._client().getListByUri<TOptionsItem>(templatedUri, { $filter: filter, $top: 10 }, headersWithCaching);
            this._problemService.checkResponseDisplayErrorsAndThrow(response, undefined, `An error occurred while getting the selected items for the property ${this.property().name}.`);

            const oldItems = this.items.value();
            const newItems = response.body._embedded.items;
            const combinedItems = this.combineCurrentItemsWithSelected(oldItems, selectedValues, newItems);

            return combinedItems;
        }
    });

    constructor(
        private readonly _problemService: ProblemService,
        private readonly _clients: RestWorldClientCollection,
        public readonly apiName: Signal<string>,
        public readonly property: Signal<TProperty>,
        private readonly _selectedValues: Signal<ExtractGenericOptionsSelectedValuesType<TProperty> | ExtractGenericOptionsSelectedValuesType<TProperty>[] | undefined>,
        private readonly _getLabel: Signal<((item: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>) => string | undefined) | undefined>,
        private readonly _getTooltip: Signal<((item: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>) => string | undefined) | undefined>) {
    }

    private static jsonStringifyWithElipsis(value: unknown) {
        const maxLength = 200;
        const end = 10;
        const start = maxLength - end - 2;
        const json = JSON.stringify(value);
        const shortened = json.length > maxLength ? json.substring(0, start) + '…' + json.substring(json.length - end) : json;

        return shortened;
    }

    private getItemByValue(items: TOptionsItem[] | undefined, value: ExtractGenericOptionsSelectedValuesType<TProperty>): TOptionsItem | undefined {
        const foundItem = items?.find(item => this.getValue(item) === value);
        if (foundItem)
            return foundItem;

        if (value === null || value === undefined)
            return undefined;

        return { [this.valueField()]: value, [this.promptField()]: "" } as TOptionsItem;
    }

    /**
     * Returns the prompt of the item or value.
     * @param itemOrValue The item or value to get the prompt for.
     * @returns The prompt of the item or value.
     */
    public getPrompt(itemOrValue: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>): string {
        const promptField = this.promptField();
        let item = itemOrValue as TOptionsItem | undefined;

        if (itemOrValue === undefined || itemOrValue === null || !itemOrValue.hasOwnProperty(promptField)) {
            const value = itemOrValue as unknown as ExtractGenericOptionsSelectedValuesType<TProperty>;
            item = this.getItemByValue(this.items.value(), value);
        }

        if (item === undefined || item === null || !item.hasOwnProperty(promptField))
            return "";

        return item[promptField as keyof TOptionsItem] as string ?? "";
    }

    /**
     * Returns the value of the item or value.
     * @param itemOrValue The item or value to get the value for.
     * @returns The value of the item or value.
     */
    public getValue(itemOrValue: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>): ExtractGenericOptionsSelectedValuesType<TProperty> {
        const valueField = this.valueField();
        if (itemOrValue === undefined || itemOrValue === null || !itemOrValue.hasOwnProperty(valueField)) {
            const value = itemOrValue as unknown as ExtractGenericOptionsSelectedValuesType<TProperty>;
            return value;
        }

        const item = itemOrValue as TOptionsItem;
        return item[valueField as keyof TOptionsItem] as ExtractGenericOptionsSelectedValuesType<TProperty>;
    }

    /**
     * Updates the items based on the given filter.
     * @param filter The filter to use when getting the items.
     */
    public async updateItemsFromFilter(filter: string | undefined): Promise<void> {
        this.filter.set(filter);
    }

    private combineCurrentItemsWithSelected(oldItems: TOptionsItem[] | undefined, selectedValues: ExtractGenericOptionsSelectedValuesType<TProperty>[], newItems: TOptionsItem[]): TOptionsItem[] {
        if (!oldItems)
            return newItems;

        // const selectedValuesAsArray = (Array.isArray(selectedValues) ? selectedValues : [selectedValues]) as ExtractGenericOptionsSelectedValuesType<TProperty>[];
        const itemsToKeep = oldItems.filter(i => selectedValues.includes(this.getValue(i)));
        const newValues = newItems.map(i => this.getValue(i));
        const combinedItems = newItems.concat(itemsToKeep.filter(i => !newValues.includes(this.getValue(i))));

        return combinedItems;
    }

    private getDefaultLabel(itemOrValue: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>): string {
        if (itemOrValue === undefined || itemOrValue === null)
            return "";

        let label = this.getPrompt(itemOrValue);
        const property = this.property();
        if (property.cols === undefined || property.cols > 1) {
            const value = this.getValue(itemOrValue);
            if (value?.toString().toUpperCase() !== label.toUpperCase())
                label += ` (${value})`;
        }

        return label;
    }

    private getDefaultTooltip(itemOrValue: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>): string {
        if (itemOrValue === undefined || itemOrValue === null || typeof itemOrValue !== "object")
            return "";

        const promptField = this.promptField();
        const valueField = this.valueField();

        const tooltip = Object.entries(itemOrValue)
            .filter(([key]) => !(key.startsWith('_') || ['createdAt', 'createdBy', 'lastChangedAt', 'lastChangedBy', 'timestamp', promptField, valueField].includes(key)))
            .reduce((prev, [key, value], index) => `${prev}${index === 0 ? '' : '\n'}${key}: ${OptionsManager.jsonStringifyWithElipsis(value)}`, '');

        return tooltip;
    }
}

/**
 * This service provides a manager for properties with options.
 * The manager can be used to load the options for a property and to manage the selected values.
 * The manager is normally used in combination with <p-select> and <p-multiSelect> components.
 * It is used internally by the <rw-dropdown> component.
 */
@Injectable({
    providedIn: 'root'
})
export class OptionsService {
    constructor(
        private readonly _problemService: ProblemService,
        private readonly _clients: RestWorldClientCollection) {
    }

    public getManager<TProperty extends Property<SimpleValue, string, string> & { options: Options<SimpleValue, string, string> }, TOptionsItem extends ExtractGenericOptionsItemType<TProperty> = ExtractGenericOptionsItemType<TProperty>>(
        apiName: Signal<string>,
        property: Signal<TProperty>,
        selectValues: Signal<ExtractGenericOptionsSelectedValuesType<TProperty> | ExtractGenericOptionsSelectedValuesType<TProperty>[] | undefined>,
        getLabel: Signal<((itemOrValue: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>) => string | undefined) | undefined>,
        getTooltip: Signal<((itemOrValue: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>) => string | undefined) | undefined>): OptionsManager<TProperty, TOptionsItem> {
        return new OptionsManager(this._problemService, this._clients, apiName, property, selectValues, getLabel, getTooltip);
    }
}
