import { Template } from "@wertzui/ngx-hal-client";

/**
 * Represents the event that is emitted when the value of a dropdown component changes.
 * @template T The type of the value that is selected in the dropdown.
 */
export interface DropdownChangeEvent<T> {
  originalEvent: Event;
  value: T;
}

/**
 * Event emitted after a successful submit which changed the template.
 */
export interface AfterSubmitOkEvent {
  /**
   * The template before the submit.
   */
  old: Template;
  /**
   * The template after the submit.
   */
  new: Template;
  /**
   * The HTTP status code of the response.
   */
  status: 200;
}

/**
 * Event emitted after a successful submit which created a new resource.
 */
export interface AfterSubmitRedirectEvent {
  /**
   * The Location header of the response if the status is 201.
   */
  location?: string;
  /**
   * The HTTP status code of the response.
   */
  status: 201;
}
