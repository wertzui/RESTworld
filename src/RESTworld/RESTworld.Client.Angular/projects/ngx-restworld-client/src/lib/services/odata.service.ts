import { ODataParameters } from "../models/o-data";
import { ODataFilterParser } from "./odata-filter-parser";
import { Injectable } from "@angular/core";
import { ActivatedRoute } from "@angular/router";
import { type FilterToken } from "@odata/parser";
import { Property, PropertyType, SimpleValue, Template } from "@wertzui/ngx-hal-client";
import { FilterMatchMode, FilterMetadata, TranslationKeys } from "primeng/api";
import { TableLazyLoadEvent } from "primeng/table";

/**
 * A static class that provides utility methods for creating OData filters, order by clauses, and parameters from {@link TableLazyLoadEvent} objects.
 */
@Injectable({
    providedIn: 'root',
})
export class ODataService {
    /**
     * Creates an OData filter string for a given property and filter metadata.
     * @param property - The property to filter on.
     * @param filter - The filter metadata to use.
     * @returns The OData filter string, or undefined if the filter value is falsy.
     */
    public static createFilterForProperty(property: Property<SimpleValue, string, string>, filter: FilterMetadata): string | undefined {
        if (filter.matchMode == TranslationKeys.NO_FILTER)
            return undefined;

        // Enums are handled differently
        if (property.options && !property.options.link)
            return ODataService.createFilterForEnum(property, filter);

        const oDataOperator = ODataService.createODataOperator(
            filter.matchMode,
        );
        const comparisonValue = ODataService.createComparisonValue(property, filter.value);

        switch (oDataOperator) {
            case 'contains':
            case 'not contains':
            case 'startswith':
            case 'endswith':
                return `${oDataOperator}(${property.name}, ${comparisonValue})`;
            default:
                return `${property.name} ${oDataOperator} ${comparisonValue}`;
        }
    }

    /**
     * Creates an OData `$filter` value for an array of {@link FilterMetadata} objects and a given {@link Property}.
     * @param property - The {@link Property} to filter on.
     * @param filters - An array of {@link FilterMetadata} objects.
     * @returns The OData `$filter` value, or undefined if no filters were provided.
     */
    public static createFilterForPropertyArray(property: Property<SimpleValue, string, string>, filters: FilterMetadata[]): string | undefined {
        const filter = filters
            .map(f => ODataService.createFilterForProperty(property, f))
            .filter(f => !!f)
            .join(` ${filters[0].operator} `);

        if (filter === '')
            return undefined;

        return `(${filter})`;
    }

    /**
     * Creates an OData `$filter` value from a {@link TableLazyLoadEvent} and an array of {@link Property Properties}.
     * @param event The {@link TableLazyLoadEvent} containing the filter data.
     * @param properties An optional array of {@link Property Properties} to filter on.
     * @returns The OData $`$filter` value, or undefined if no filters were applied or no properties were provided.
     */
    public static createFilterFromTableLoadEvent(event: TableLazyLoadEvent, properties?: ReadonlyArray<Property<SimpleValue, string, string>>): string | undefined {
        const eventFilters = event.filters;
        if (eventFilters === undefined || properties === undefined)
            return undefined;

        const filter = properties
            .map(property => ({ property, filters: eventFilters[property.name] as (FilterMetadata[] | undefined) }))
            .filter(f => f.filters !== undefined)
            .map(f => ODataService.createFilterForPropertyArray(f.property, f.filters!))
            .filter(f => !!f)
            .join(' and ');

        if (filter === '')
            return undefined;

        return `(${filter})`;
    }

    /**
     * Creates a map of property names to filter metadata from an OData $filter string.
     * @param filter The OData filter string to parse.
     * @returns A record of property names to filter metadata.
     */
    public static createFilterMetadataFromODataFilter(filter: string | undefined, properties: Partial<Record<string, Property<SimpleValue, string, string>>>): Partial<Record<string, FilterMetadata[]>> {
        const filters = ODataFilterParser.parseFilter(filter, properties);

        return filters;
    }

    /**
     * Creates a OData `$orderby` value from a {@link TableLazyLoadEvent}.
     * @param event The {@link TableLazyLoadEvent} to create the `$orderby` value from.
     * @returns The `$orderby` value created from the {@link TableLazyLoadEvent}.
     */
    public static createOrderByFromTableLoadEvent(event: TableLazyLoadEvent): string | undefined {
        if (event.multiSortMeta && event.multiSortMeta.length > 0) {
            return event.multiSortMeta
                .map(m => `${m.field} ${m.order > 0 ? 'asc' : 'desc'}`)
                .join(', ');
        }

        if (event.sortField) {
            const order = !event.sortOrder || event.sortOrder > 0 ? 'asc' : 'desc';
            return `${event.sortField} ${order}`;
        }

        return undefined;
    }

    /**
     * Creates an {@link ODataParameters} object from a given {@link ActivatedRoute}.
     * @param route The {@link ActivatedRoute} to create the {@link ODataParameters} from.
     * @param prefix An optional prefix to use for the query parameter keys.
     * @returns The {@link ODataParameters} object created from the {@link ActivatedRoute}.
     */
    public static createParametersFromRoute(route: ActivatedRoute, prefix?: string): ODataParameters {
        const snapshot = route.snapshot;

        let oDataParameters = {} as ODataParameters;
        const filter = snapshot.queryParamMap.get(`${prefix}$filter`);
        const orderBy = snapshot.queryParamMap.get(`${prefix}$orderby`);
        const top = snapshot.queryParamMap.get(`${prefix}$top`);
        const skip = snapshot.queryParamMap.get(`${prefix}$skip`);

        if (filter)
            oDataParameters.$filter = filter;
        if (orderBy)
            oDataParameters.$orderby = orderBy;
        if (top) {
            const topNumber = Number.parseInt(top);
            if (Number.isInteger(topNumber))
                oDataParameters.$top = topNumber;
        }
        if (skip) {
            const skipNumber = Number.parseInt(skip);
            if (Number.isInteger(skipNumber))
                oDataParameters.$skip = skipNumber;
        }

        return oDataParameters;
    }

    /**
     * Creates {@link ODataParameters} from a {@link TableLazyLoadEvent} and an optional {@link Template}.
     * @param event The {@link TableLazyLoadEvent} to create OData parameters from.
     * @param template An optional {@link Template} to use for creating the OData parameters.
     * @returns An {@link ODataParameters} object containing the `$filter`, `$orderby`, `$top`, and `$skip` parameters.
     */
    public static createParametersFromTableLoadEvent(event: TableLazyLoadEvent, template?: Template): ODataParameters {
        const oDataParameters = {
            $filter: ODataService.createFilterFromTableLoadEvent(event, template?.properties),
            $orderby: ODataService.createOrderByFromTableLoadEvent(event),
            $top: ODataService.createTopFromTableLoadEvent(event),
            $skip: ODataService.createSkipFromTableLoadEvent(event)
        };

        return oDataParameters;
    }

    /**
     * Creates a OData `$skip` value from a {@link TableLazyLoadEvent}.
     * @param event The {@link TableLazyLoadEvent} to create the `$skip` value from.
     * @returns The `$skip` value created from the {@link TableLazyLoadEvent}.
     */
    public static createSkipFromTableLoadEvent(event: TableLazyLoadEvent): number | undefined {
        return event.first;
    }

    /**
     * Creates a OData `$top` value from a {@link TableLazyLoadEvent}.
     * @param event The {@link TableLazyLoadEvent} to create the `$top` value from.
     * @returns The `$top` value created from the {@link TableLazyLoadEvent}.
     */
    public static createTopFromTableLoadEvent(event: TableLazyLoadEvent): number | undefined {
        return event.rows === null ? undefined : event.rows;
    }

    private static createComparisonValue(property: Property<SimpleValue, string, string>, value: unknown, isEnum?: boolean): string {
        if (value === null || value === undefined)
            return 'null';

        const type = ODataService.getPropertyType(property, value);

        switch (type) {
            case PropertyType.Date:
                return `cast(${(value as Date).toISOString()}, Edm.DateOnly)`;
            case PropertyType.DatetimeLocal:
                return `cast(${(value as Date).toISOString()}, Edm.DateTime)`;
            case PropertyType.DatetimeOffset:
                return `cast(${(value as Date).toISOString()}, Edm.DateTimeOffset)`;
            case PropertyType.Time:
                return `cast(${(value as Date).toISOString()}, Edm.TimeOnly)`;
            case PropertyType.Duration:
                return `cast(${(value as Date).toISOString()}, Edm.TimeSpan)`;
            case PropertyType.Bool:
            case PropertyType.Number:
            case PropertyType.Currency:
            case PropertyType.Month:
                return '' + value;
            case PropertyType.Percent:
                return '' + ((value as number) / 100);
            default:
                return `'${isEnum && typeof value === "string" ? value.charAt(0).toUpperCase() + value.slice(1) : value}'`;
        }
    }

    private static createFilterForEnum(property: Property<SimpleValue, string, string>, filter: FilterMetadata): string | undefined {
        if (filter.matchMode == TranslationKeys.NO_FILTER)
            return undefined;

        const options = property.options;
        if (options === undefined)
            throw Error(`Property ${property.name} has no options`);

        const maxItems = options.maxItems ?? Number.MAX_SAFE_INTEGER;
        const oDataOperator = ODataService.createODataOperator(
            filter.matchMode,
        );

        // Normal enum
        if (maxItems === 1) {
            const comparisonValue = ODataService.createComparisonValue(property, filter.value, true);
            return `${property.name} ${oDataOperator} ${comparisonValue}`;
        }

        // Flags enum
        if (filter.value === null || filter.value === undefined)
            return undefined;

        const values = Array.isArray(filter.value) ? filter.value : [filter.value];
        const comparisonValues = values.map(v => ODataService.createComparisonValue(property, v, true));

        const filters = comparisonValues.map(v => `${property.name} has ${v}`);
        const concatenatedFilters = filters.join(' and ');

        return `(${concatenatedFilters})`;
    }

    private static createODataOperator(matchMode?: string): string {
        switch (matchMode) {
            case FilterMatchMode.STARTS_WITH:
                return 'startswith';
            case FilterMatchMode.CONTAINS:
                return 'contains';
            case FilterMatchMode.NOT_CONTAINS:
                return 'not contains';
            case FilterMatchMode.ENDS_WITH:
                return 'endswith';
            case FilterMatchMode.EQUALS:
                return 'eq';
            case FilterMatchMode.NOT_EQUALS:
                return 'ne';
            case FilterMatchMode.IN:
                return 'in';
            case FilterMatchMode.LESS_THAN:
                return 'lt';
            case FilterMatchMode.LESS_THAN_OR_EQUAL_TO:
                return 'le';
            case FilterMatchMode.GREATER_THAN:
                return 'gt';
            case FilterMatchMode.GREATER_THAN_OR_EQUAL_TO:
                return 'ge';
            case FilterMatchMode.IS:
                return 'eq';
            case FilterMatchMode.IS_NOT:
                return 'ne';
            case FilterMatchMode.BEFORE:
                return 'lt';
            case FilterMatchMode.AFTER:
                return 'gt';
            case FilterMatchMode.DATE_AFTER:
                return 'ge';
            case FilterMatchMode.DATE_BEFORE:
                return 'lt';
            case FilterMatchMode.DATE_IS:
                return 'eq';
            case FilterMatchMode.DATE_IS_NOT:
                return 'ne';
            default:
                throw Error(`Unknown matchMode ${matchMode}`);
        }
    }

    private static getPropertyType(property: Property<SimpleValue, string, string>, value: unknown): PropertyType {
        if (property.options) {
            if (typeof value === "string" ||
                Array.isArray(value) && value.every(v => typeof v === "string") ||
                property.options.inline?.some(o => property.options!.valueField !== undefined && typeof (o as any)[property.options!.valueField] === "string"))
                return PropertyType.Text;

            return PropertyType.Number;
        }

        return property.type!;
    }
}
