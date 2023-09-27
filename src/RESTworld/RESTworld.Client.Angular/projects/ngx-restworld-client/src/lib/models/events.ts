/**
 * Represents the event that is emitted when the value of a dropdown component changes.
 * @template T The type of the value that is selected in the dropdown.
 */
export interface DropdownChangeEvent<T> {
  originalEvent: Event;
  value: T;
}
