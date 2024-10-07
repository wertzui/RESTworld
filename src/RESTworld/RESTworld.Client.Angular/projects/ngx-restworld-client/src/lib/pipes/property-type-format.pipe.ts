import { Pipe, PipeTransform } from '@angular/core';
import { ClrFormatPipe } from './clr-format.pipe';
import { PropertyType } from '@wertzui/ngx-hal-client';

@Pipe({
  name: 'propertyTypeFormat'
})
/**
 * A pipe that formats the value of a {@link Property} based on its {@link PropertyType}.
 * @example
 * <div>{{ value | propertyTypeFormat: property.type }}</div>
 */
export class PropertyTypeFormatPipe implements PipeTransform {
  private static readonly _dateFormat = new Date(3333, 10, 22) // months start at 0 in JS
    .toLocaleDateString()
    .replace("22", "dd")
    .replace("11", "MM")
    .replace("3333", "yyyy")
    .replace("33", "yy");

  private static readonly _timeFormat = new Date(1, 1, 1, 22, 33, 44)
    .toLocaleTimeString()
    .replace("22", "hh")
    .replace("33", "mm")
    .replace("44", "ss");

  private static readonly _typeToFormatMap = new Map<PropertyType, string>([
    [PropertyType.Hidden, ""],
    [PropertyType.Text, "{0}"],
    [PropertyType.Textarea, "{0}"],
    [PropertyType.Search, "{0}"],
    [PropertyType.Tel, "{0}"],
    [PropertyType.Url, "{0}"],
    [PropertyType.Email, "{0}"],
    [PropertyType.Password, "{0}"],
    [PropertyType.Date, "{0:" + PropertyTypeFormatPipe._dateFormat + "}"],
    [PropertyType.Month, "{0}"],
    [PropertyType.Week, "{0}"],
    [PropertyType.Time, "{0:" + PropertyTypeFormatPipe._timeFormat + "}"],
    [PropertyType.DatetimeLocal, "{0:" + PropertyTypeFormatPipe._dateFormat + " " + PropertyTypeFormatPipe._timeFormat + "}"],
    [PropertyType.Number, "{0}"],
    [PropertyType.Range, "{0}"],
    [PropertyType.Color, "{0}"],
    [PropertyType.Bool, "{0}"],
    [PropertyType.DatetimeOffset, "{0:" + PropertyTypeFormatPipe._dateFormat + " " + PropertyTypeFormatPipe._timeFormat + "}"],
    [PropertyType.Duration, "{0}"],
    [PropertyType.Image, "{0}"],
    [PropertyType.File, "{0}"],
    [PropertyType.Collection, "{0}"],
    [PropertyType.Object, "{0}"],
    [PropertyType.Percent, "{0:P}"],
    [PropertyType.Currency, "{0:N}"],
  ]);


  public constructor(private readonly _clrFormatPipe: ClrFormatPipe) {
  }

  public transform(value: any, type?: PropertyType): string {
    const format = type === undefined ? "{0}" : (PropertyTypeFormatPipe._typeToFormatMap.get(type) ?? "{0}");
    return this._clrFormatPipe.transform(value, format);
  }
}
