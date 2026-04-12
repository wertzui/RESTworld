import { PrimitiveTypeEnum } from "@odata/metadata";
import { type Token, TokenType, createTraverser, type EqualsExpressionToken, type LeftRightExpressionToken, type LiteralToken, type AndExpressionToken, type OrExpressionToken } from "@odata/parser";
import type { SimpleValue } from "@wertzui/ngx-hal-client";
import { type FilterMetadata, FilterMatchMode } from "primeng/api";

interface MethodCallExpressionToken extends Token {
    type: TokenType.MethodCallExpression;
    value: {
        method: string;
        parameters: Token[];
    };
}

interface ODataIdentifierToken extends Token {
    type: TokenType.ODataIdentifier;
    value: {
        name: string;
    };
}

/**
 * This visitor turns a parsed OData $filter query string represented as a {@link Token} into a map of property names to filter metadata.
 * The filter metadata is used by the PrimeNG table component to filter the rows of
 * a table based on the values of the columns.
 */
export class OdataVisitor {
    private _filters: Map<string, FilterMetadata[]> = new Map();
    private operator: "and" | "or" | undefined;
    private propertyName: string | undefined;
    private value: SimpleValue | null | undefined;

    private static correctFilterMatchMode(matchMode: string, literalValue: SimpleValue | SimpleValue[] | null): string {
        if (literalValue instanceof Date) {
            switch (matchMode) {
                case FilterMatchMode.EQUALS:
                    return FilterMatchMode.DATE_IS;
                case FilterMatchMode.NOT_EQUALS:
                    return FilterMatchMode.DATE_IS_NOT;
                case FilterMatchMode.LESS_THAN:
                    return FilterMatchMode.DATE_BEFORE;
                case FilterMatchMode.LESS_THAN_OR_EQUAL_TO:
                    return FilterMatchMode.DATE_BEFORE;
                case FilterMatchMode.GREATER_THAN:
                    return FilterMatchMode.DATE_AFTER;
                case FilterMatchMode.GREATER_THAN_OR_EQUAL_TO:
                    return FilterMatchMode.DATE_AFTER;
            }
        }

        return matchMode;
    }

    private static parseLiteralValue(node: LiteralToken): SimpleValue | null {
        const type = node.value as PrimitiveTypeEnum;
        switch (type) {
            case PrimitiveTypeEnum.Binary:
            case PrimitiveTypeEnum.Boolean:
                return node.raw === "true";
            case PrimitiveTypeEnum.Byte:
            case PrimitiveTypeEnum.Int16:
            case PrimitiveTypeEnum.Int32:
            case PrimitiveTypeEnum.Int64:
            case PrimitiveTypeEnum.SByte:
                return Number.parseInt(node.raw);
            case PrimitiveTypeEnum.Date:
            case PrimitiveTypeEnum.DateTime:
            case PrimitiveTypeEnum.DateTimeOffset:
            case PrimitiveTypeEnum.Duration:
            case PrimitiveTypeEnum.TimeOfDay:
                return new Date(node.raw);
            case PrimitiveTypeEnum.Decimal:
            case PrimitiveTypeEnum.Double:
            case PrimitiveTypeEnum.Single:
                return Number.parseFloat(node.raw);
            default:
                switch (node.raw) {
                    case "null":
                        return null;
                    default:
                        return node.raw.startsWith("'") ? node.raw.substring(1, node.raw.length - 1) : node.raw;
                }
        }
    }

    private static parseMethodCallExpression(node: Token): string {
        switch (node.value.method) {
            case 'startswith':
                return FilterMatchMode.STARTS_WITH;
            case 'contains':
                return FilterMatchMode.CONTAINS;
            case 'not contains':
                return FilterMatchMode.NOT_CONTAINS;
            case 'endswith':
                return FilterMatchMode.ENDS_WITH;
            default:
                throw Error(`Unknown method call ${node.value.method}`);
        }
    }

    /***
     * Parses an OData $filter query string and returns a map of property names to filter metadata.
     * @param node The root node of the OData $filter query string.
     * @param logToConsole If true, logs the type and raw value of each token to the console.
     * @returns A map of property names to filter metadata.
     */
    public parse(node: Token, logToConsole?: boolean): Map<string, FilterMetadata[]> {
        if (logToConsole) {
            const visitLogAll = createTraverser(
                Object.fromEntries(
                    Object.entries(TokenType)
                        .map(([key, value]) => ([key, (node: Token) => console.log(`${node.type}: ${node.raw}`)] as [string, (node: Token) => void])
                        )
                ), true);
            visitLogAll(node);
        }

        const visit = createTraverser({
            EqualsExpression: (node: Token) => this.CreateFilter(this.propertyName, this.value, FilterMatchMode.EQUALS),
            NotEqualsExpression: (node: Token) => this.CreateFilter(this.propertyName, this.value, FilterMatchMode.NOT_EQUALS),
            GreaterThanExpression: (node: Token) => this.CreateFilter(this.propertyName, this.value, FilterMatchMode.GREATER_THAN),
            GreaterOrEqualsExpression: (node: Token) => this.CreateFilter(this.propertyName, this.value, FilterMatchMode.GREATER_THAN_OR_EQUAL_TO),
            LesserThanExpression: (node: Token) => this.CreateFilter(this.propertyName, this.value, FilterMatchMode.LESS_THAN),
            LesserOrEqualsExpression: (node: Token) => this.CreateFilter(this.propertyName, this.value, FilterMatchMode.LESS_THAN_OR_EQUAL_TO),
            HasExpression: (node: Token) => this.CreateFilter(this.propertyName, [this.value], FilterMatchMode.EQUALS),
            ODataIdentifier: (node: Token) => this.propertyName = node.value.name,
            Literal: (node: Token) => this.value = OdataVisitor.parseLiteralValue(node as LiteralToken),
            AndExpression: (node: Token) => this.operator = "and",
            OrExpression: (node: Token) => this.operator = "or",
            BoolParenExpression: (node: Token) => this.SetOperatorForFilters(),
            MethodCallExpression: (node: Token) => this.visitMethodCallExpression(node as MethodCallExpressionToken, visit),
        }, true);

        visit(node);

        return this._filters;
    }

    private CreateFilter(propertyName: string | undefined, value: SimpleValue | SimpleValue[] | null, matchMode: string) {
        if (propertyName === undefined)
            throw Error(`Cannot add a filter for the value ${value} and the match mode ${matchMode} without a property name`);

        matchMode = OdataVisitor.correctFilterMatchMode(matchMode, value);
        const filter = this._filters.get(propertyName);
        if (filter === undefined) {
            this._filters.set(propertyName, [{ value, operator: "and", matchMode: matchMode }]);
        }
        else if (filter.length === 1 && Array.isArray(filter[0].value) && Array.isArray(value)) {
            filter[0].value.push(...value); // Flags enum
        }
        else {
            filter.push({ value, operator: "and", matchMode: matchMode });
        }
    }

    private SetOperatorForFilters() {
        // In this case we are in the outermost expression which is just parantheses around the complete $filter value
        if (this.propertyName === undefined)
            return;

        // If we have not found an operator, or it is "and", we can just stay with the default created with the filter.
        if (this.operator === undefined || this.operator === "and")
            return;

        const filter = this._filters.get(this.propertyName);
        if (filter === undefined)
            return;

        // The operator is "or", so we need to set the operator for all filters to "or"
        for (const f of filter)
            f.operator = this.operator;
    }

    // The visitor does not correctly implement the MethodCallExpressionToken, so we need to do this manually
    private visitMethodCallExpression(node: MethodCallExpressionToken, visit: (node: Token) => void) {
        for (const parameter of node.value.parameters)
            visit(parameter);
        const matchMode = OdataVisitor.parseMethodCallExpression(node);
        this.CreateFilter(this.propertyName, this.value, matchMode);
    }
}
