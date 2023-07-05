import { FilterMatchMode, FilterMetadata, LazyLoadEvent } from 'primeng/api';
import { Property, PropertyType, Template } from '@wertzui/ngx-hal-client';

export class ODataService {
  public static createFilterForProperty(property: Property, filter: FilterMetadata): string | undefined {
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

  public static createFilterForPropertyArray(property: Property, filters: FilterMetadata[]): string | undefined {
    const filter = filters
      .map(f => ODataService.createFilterForProperty(property, f))
      .filter(f => !!f)
      .join(` ${filters[0].operator} `);

    if (filter === '')
      return undefined;

    return `(${filter})`;
  }

  public static createFilterFromTableLoadEvent(event: LazyLoadEvent, properties?: Property[]): string | undefined {
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

  public static createOrderByFromTableLoadEvent(event: LazyLoadEvent): string | undefined {
    if (event.sortField) {
      const order = !event.sortOrder || event.sortOrder > 0 ? 'asc' : 'desc';
      return `${event.sortField} ${order}`;
    }

    return undefined;
  }

  public static createParametersFromTableLoadEvent(event: LazyLoadEvent, template?: Template) {
    const oDataParameters = {
      $filter: ODataService.createFilterFromTableLoadEvent(event, template?.properties),
      $orderby: ODataService.createOrderByFromTableLoadEvent(event),
      $top: ODataService.createTopFromTableLoadEvent(event),
      $skip: ODataService.createSkipFromTableLoadEvent(event)
    };

    return oDataParameters;
  }

  public static createSkipFromTableLoadEvent(event: LazyLoadEvent): number | undefined {
    return event.first;
  }

  public static createTopFromTableLoadEvent(event: LazyLoadEvent): number | undefined {
    return event.rows;
  }

  private static createComparisonValue(property: Property, value: unknown): string {
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

  private static findPropertyByName(properties: Property[], propertyName: string): Property {
    const property = properties.find(p => p.name === propertyName);
    if (property === undefined)
      throw new Error(`Cannot find a property with the name ${propertyName} in the properties of the search form template.`);

    return property;
  }
}
