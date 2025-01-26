import { Pipe, PipeTransform } from '@angular/core';
// @ts-ignore
import * as clrFormat from 'clr-format';

@Pipe({
    name: 'clrFormat',
    standalone: true
})
/**
 * A pipe that formats a value using a specified format string.
 * Have a look at https://learn.microsoft.com/en-us/dotnet/standard/base-types/formatting-types to see what is supported.
 * @example
 * <div>{{ value | clrFormat: 'yyyy-MM-dd' }}</div>
 */
export class ClrFormatPipe implements PipeTransform {
    private static readonly formatFunction = ClrFormatPipe.getFormatFunction();

    private static getFormatFunction(): (format: string, value: any) => string {
        // Depending if this is bundled with webpack or not, the default export will be different
        return clrFormat.default ?? clrFormat;
    }

    public transform(value: any, format?: string): string {
        if (format === undefined)
            return value;

        return format.includes("{0") ? ClrFormatPipe.formatFunction(format, value) : ClrFormatPipe.formatFunction("{0:" + format + "}", value);
    }
}
