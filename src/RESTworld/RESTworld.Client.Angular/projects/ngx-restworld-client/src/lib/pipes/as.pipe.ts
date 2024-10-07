import { Pipe, PipeTransform } from '@angular/core';

/**
 * Cast super type into type using generics
 * Return Type obtained by optional @param type OR assignment type.
 * @example
 * <div *ngFor="let item of items | as: Item">
 */
@Pipe({ name: 'as' })
export class AsPipe implements PipeTransform {
  /**
   * Cast (S: SuperType) into (T: Type) using @Generics.
   * @param value (S: SuperType) obtained from input type.
   * @optional @param type (T CastingType)
   * type?: { new (): T }
   * type?: new () => T
   */
  transform<S, T extends S>(value: S, type: new (...args: any[]) => T): T {
    return value as T;
  }
}
