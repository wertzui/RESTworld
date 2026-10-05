import { AbstractControl, FormGroup } from "@angular/forms";
import { Property, Template } from "@wertzui/ngx-hal-client";

/**
 * The template context for a property.
 */
export interface PropertyTemplateContext {
    property: Property,
}

/**
 * The template context for a property that is a dropdown.
 */
export interface DropdownTemplateContext extends PropertyTemplateContext {
    apiName: string,
}
