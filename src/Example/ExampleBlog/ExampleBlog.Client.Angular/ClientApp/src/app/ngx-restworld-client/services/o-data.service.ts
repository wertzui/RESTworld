import { FilterMatchMode, FilterMetadata, LazyLoadEvent } from 'primeng/api';
import { Property, PropertyType, SimpleValue, Template } from '@wertzui/ngx-hal-client';
import { TableLazyLoadEvent } from 'primeng/table';
import { ODataParameters } from '../models/o-data';

/**
 * A static class that provides utility methods for creating OData filters, order by clauses, and parameters from `TableLazyLoadEvent` objects.
 */
export class ODataService {
  /**
   * Creates an OData filter string for a given property and filter metadata.
   * @param property - The property to filter on.
   * @param filter - The filter metadata to use.
   * @returns The OData filter string, or undefined if the filter value is falsy.
   */
  public static createFilterForProperty(property: Property<SimpleValue, string, string>, filter: FilterMetadata): string | undefined {
    if (!filter.value)
      return undefined;

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
   * Creates an OData $filter value for an array of filter metadata objects and a given property.
   * @param property - The property to filter on.
   * @param filters - An array of filter metadata objects.
   * @returns The OData $filter value, or undefined if no filters were provided.
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
   * Creates an OData $filter value from a TableLazyLoadEvent and an array of properties.
   * @param event The TableLazyLoadEvent containing the filter data.
   * @param properties An optional array of properties to filter on.
   * @returns The OData $filter value, or undefined if no filters were applied or no properties were provided.
   */
  public static createFilterFromTableLoadEvent(event: TableLazyLoadEvent, properties?: ReadonlyArray<Property<SimpleValue, string, string>>): string | undefined {
    if (!event.filters || properties === undefined)
      return undefined;

    const filter = Object.entries(event.filters)
      // The type definition is wrong, event.filters has values of type FilterMetadata[] and not FilterMetadata.
      .map(([propertyName, filter]) => ({ property: ODataService.findPropertyByName(properties, propertyName), filters: filter as FilterMetadata[] }))
      .map(f => ODataService.createFilterForPropertyArray(f.property, f.filters))
      .filter(f => !!f)
      .join(' and ');

    if (filter === '')
      return undefined;

    return `(${filter})`;
  }

  /**
   * Creates a OData $orderby value from a TableLazyLoadEvent.
   * @param event The TableLazyLoadEvent to create the $orderby value from.
   * @returns The $orderby value created from the TableLazyLoadEvent.
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
   * Creates OData parameters from a TableLazyLoadEvent and an optional Template.
   * @param event The TableLazyLoadEvent to create OData parameters from.
   * @param template An optional Template to use for creating the OData parameters.
   * @returns An ODataParameters object containing the $filter, $orderby, $top, and $skip parameters.
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
   * Creates a OData $skip value from a TableLazyLoadEvent.
   * @param event The TableLazyLoadEvent to create the $skip value from.
   * @returns The $skip value created from the TableLazyLoadEvent.
   */
  public static createSkipFromTableLoadEvent(event: TableLazyLoadEvent): number | undefined {
    return event.first;
  }

  /**
   * Creates a OData $top value from a TableLazyLoadEvent.
   * @param event The TableLazyLoadEvent to create the $top value from.
   * @returns The $top value created from the TableLazyLoadEvent.
   */
  public static createTopFromTableLoadEvent(event: TableLazyLoadEvent): number | undefined {
    return event.rows === null ? undefined : event.rows;
  }

  private static createComparisonValue(property: Property<SimpleValue, string, string>, value: unknown): string {
    if (value === null || value === undefined)
      return 'null';

    const type = property.type;

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
        return `'${value}'`;
    }
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

  private static findPropertyByName(properties: ReadonlyArray<Property<SimpleValue, string, string>>, propertyName: string): Property<SimpleValue, string, string> {
    const property = properties.find(p => p.name === propertyName);
    if (property === undefined)
      throw new Error(`Cannot find a property with the name ${propertyName} in the properties of the search form template.`);

    return property;
  }
}
