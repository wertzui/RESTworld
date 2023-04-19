import { AbstractControl, FormGroup } from "@angular/forms";
import { Property, Template } from "@wertzui/ngx-hal-client";

export interface PropertyTemplateContext {
    property: Property,
}

export interface DropdownTemplateContext extends PropertyTemplateContext {
    apiName: string,
}
