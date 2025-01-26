import { computed, effect, Injectable, linkedSignal, signal, type Signal } from "@angular/core";
import { RestWorldClientCollection } from "./restworld-client-collection";
import { ProblemDetails, type ExtractGenericOptionsItemType, type ExtractGenericOptionsSelectedValuesType, type Options, type Property, type ResourceOfDto, type SimpleValue } from "@wertzui/ngx-hal-client";
import { MessageService } from "primeng/api";
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
    public readonly items = linkedSignal(() => this.options()?.inline as TOptionsItem[] ?? []);
    public readonly loading = signal(false);
    public readonly options = computed(() => this.property().options);
    public readonly promptField = computed(() => this.options()?.promptField ?? "prompt");
    public readonly valueField = computed(() => this.options()?.valueField ?? "value");

    private readonly _client = computed(() => this._clients.getClient(this.apiName()));
    public readonly selectedItems = computed(() => this.selectedValues()?.map(value => this.getItemByValue(this.items(), value as ExtractGenericOptionsSelectedValuesType<TProperty>) ?? []));

    constructor(
        private readonly _problemService: ProblemService,
        private readonly _clients: RestWorldClientCollection,
        public readonly apiName: Signal<string>,
        public readonly property: Signal<TProperty>,
        public readonly selectedValues: Signal<ExtractGenericOptionsSelectedValuesType<TProperty> | ExtractGenericOptionsSelectedValuesType<TProperty>[] | undefined>,
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

    private getItemByValue(items: TOptionsItem[], value: ExtractGenericOptionsSelectedValuesType<TProperty>): TOptionsItem | undefined {
        const foundItem = items.find(item => this.getValue(item) === value);
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
            item = this.getItemByValue(this.items(), value);
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
     * Initializes the options manager.
     * This will load the initial options for the property.
     * Call it once in the ngOnInit method of your component.
     */
    public async initialize(): Promise<void> {
        const options = this.options();
        if (!options.link?.href)
            return;

        const selectedValues = this.selectedValues() ?? options.selectedValues;

        const filter = selectedValues === null || selectedValues === undefined ?
            undefined :
            `${options.valueField} in (${selectedValues})`;

        await this.updateItemsFromFilter(filter);
    }

    /**
     * Updates the items based on the given filter.
     * @param filter The filter to use when getting the items.
     */
    public async updateItemsFromFilter(filter: string | undefined): Promise<void> {
        this.loading.set(true);
        try {
            const options = this.options();
            const templatedUri = options.link?.href;
            if (!templatedUri)
                this._problemService.displayToastAndThrow(undefined, `The property ${this.property().name} does not have options with a link href.`);

            const headersWithCaching = new HttpHeaders({[NgHttpCachingHeaders.ALLOW_CACHE]: "1", [NgHttpCachingHeaders.LIFETIME]: "3000"});
            const response = await this._client().getListByUri<TOptionsItem>(templatedUri, { $filter: filter, $top: 10 }, headersWithCaching);
            this._problemService.checkResponseDisplayErrorsAndThrow(response, undefined, `An error occurred while getting the selected items for the property ${this.property().name}.`);

            const items = response.body._embedded.items;
            const newItems = this.combineCurrentItemsWithSelected(items);
            this.items.set(newItems);
        }
        finally {
            this.loading.set(false);
        }
    }

    private combineCurrentItemsWithSelected(items: TOptionsItem[]): TOptionsItem[] {
        const oldInline = this.items();
        if (!oldInline)
            return items;

        const selectedValues = this.selectedValues();
        const selectedValuesAsArray = (Array.isArray(selectedValues) ? selectedValues : [selectedValues]) as ExtractGenericOptionsSelectedValuesType<TProperty>[];
        const itemsToKeep = oldInline.filter(i => selectedValuesAsArray.includes(this.getValue(i)));
        const newValues = items.map(i => this.getValue(i));
        const newItems = items.concat(itemsToKeep.filter(i => !newValues.includes(this.getValue(i))));

        return newItems;
    }

    private getDefaultLabel(itemOrValue: TOptionsItem | ExtractGenericOptionsSelectedValuesType<TProperty>): string {
        if (itemOrValue === undefined || itemOrValue === null)
            return "";

        let label = this.getPrompt(itemOrValue);
        const property = this.property();
        if (property.cols === undefined || property.cols > 1) {
            const value = this.getValue(itemOrValue);
            if (typeof value !== "string" || (value !== "" && (value as string).toUpperCase() !== label.toUpperCase()))
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
