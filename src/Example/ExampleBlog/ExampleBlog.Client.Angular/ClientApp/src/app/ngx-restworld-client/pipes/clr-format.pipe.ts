import { Pipe, PipeTransform } from '@angular/core';
// @ts-ignore
import * as clrFormat from 'clr-format';

@Pipe({
  name: 'clrFormat'
})
export class ClrFormatPipe implements PipeTransform {

  constructor() {
  }

  transform(value: any, format?: string): string {
    if (format === undefined)
      return value;

    return format.includes("{0") ? clrFormat(format, value) : clrFormat("{0:" + format + "}", value);
  }
}
