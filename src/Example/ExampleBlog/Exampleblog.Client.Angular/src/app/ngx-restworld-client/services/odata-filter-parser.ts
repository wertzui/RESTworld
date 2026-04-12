import { defaultParser } from "@odata/parser";
import type { FilterMetadata } from "primeng/api";
import { OdataVisitor } from "./odata-visitor";
import type { Property, SimpleValue } from "@wertzui/ngx-hal-client";


/**
 * This class is used to parse an OData $filter string into a Record of filter constraints.
 */
export class ODataFilterParser {

    public static parseFilter(filter: string | undefined, properties: Partial<Record<string, Property<SimpleValue, string, string>>>): Partial<Record<string, FilterMetadata[]>> {
        if (!filter)
            return {};

        // The parser has a bug where it does not correctly parse cast expressions, so we need to replace them with the first argument
        // See https://github.com/Soontao/odata-v4-parser/issues/283
        filter = ODataFilterParser.replaceCastExpressions(filter);
        const ast = defaultParser.filter(filter);
        const visitor = new OdataVisitor();
        const filters = visitor.parse(ast);

        /// OData needs enum values in pascal case, but JSON and therefor HAL-Forms needs them in camel case
        ODataFilterParser.MakeEnumValuesCamelCase(filters, properties);

        return Object.fromEntries(filters);
    }

    private static MakeEnumValuesCamelCase(filters: Map<string, FilterMetadata[]>, properties: Partial<Record<string, Property<SimpleValue, string, string>>>) {
        for (const [key, value] of filters) {
            for (const filter of value) {
                const options = properties[key]?.options;
                if (options && !options.link && typeof filter.value === "string") {
                    filter.value = filter.value.charAt(0).toLowerCase() + filter.value.slice(1);
                }
            }
        }
    }

    private static replaceCastExpressions(input: string): string {
        // Regular expression to match cast(x, y) and extract x
        const regex = /cast\(([^,]+),\s*[^)]+\)/g;

        // Replace matches with the first captured group (x)
        const result = input.replace(regex, (match, x) => x.trim());

        return result;
    }
}
