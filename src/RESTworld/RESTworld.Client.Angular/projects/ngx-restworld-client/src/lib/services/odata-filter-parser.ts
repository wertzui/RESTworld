import { defaultParser } from "@odata/parser";
import type { FilterMetadata } from "primeng/api";
import { OdataVisitor } from "./odata-visitor";


/**
 * This class is used to parse an OData $filter string into a Record of filter constraints.
 */
export class ODataFilterParser {

    public static parseFilter(filter: string | undefined): Partial<Record<string, FilterMetadata[]>> {
        if (!filter)
            return {};

        // The parser has a bug where it does not correctly parse cast expressions, so we need to replace them with the first argument
        // See https://github.com/Soontao/odata-v4-parser/issues/283
        filter = ODataFilterParser.replaceCastExpressions(filter);
        const ast = defaultParser.filter(filter);
        const visitor = new OdataVisitor();
        const filters = visitor.parse(ast);

        return Object.fromEntries(filters);
    }

    private static replaceCastExpressions(input: string): string {
        // Regular expression to match cast(x, y) and extract x
        const regex = /cast\(([^,]+),\s*[^)]+\)/g;

        // Replace matches with the first captured group (x)
        const result = input.replace(regex, (match, x) => x.trim());

        return result;
    }
}
