import { Injectable } from "@angular/core";
import { FormArray, FormControl, FormGroup, Validators } from '@angular/forms';
import { Property, PropertyType, Template, Templates } from '@wertzui/ngx-hal-client';

@Injectable({
  providedIn: 'root'
})
export class FormService {

  public createFormGroupsFromTemplates(templates: Templates): { [key: string]: FormGroup } {
    const tabs = Object.fromEntries(Object.entries(templates).map(([name, template]) => [
      name,
      this.createFormGroupFromTemplate(template)
    ]));

    return tabs;
  }

  private createFormGroupFromTemplates(templates: Templates, ignoredProperties: string[]): FormGroup {
    const controls = Object.fromEntries(
      Object.entries(templates)
        .filter(([key, ]) => !ignoredProperties.some(p => key === p))
        .map(([name, template]) => [
          name,
          this.createFormGroupFromTemplate(template)
        ]));
    const formGroup = new FormGroup(controls);
    return formGroup;
  }

  public createFormArrayFromTemplates(templates: Templates, ignoredProperties: string[]): FormArray {
    const controls =
      Object.entries(templates)
        .filter(([key, ]) => !ignoredProperties.some(p => key === p))
        .map(([, template]) =>
          this.createFormGroupFromTemplate(template));
    const formArray = new FormArray(controls);
    return formArray;
  }

  public createFormGroupFromTemplate(template: Template): FormGroup {
    const controls = Object.fromEntries(template.properties.map(p => [
      p.name,
      this.createFormControl(p)
    ]));
    const formGroup = new FormGroup(controls);
    return formGroup;
  }

  public createFormControl(property: Property): FormControl | FormGroup | FormArray {
    if (property.type === PropertyType.Object)
      return this.createFormGroupFromTemplate(property._templates['default']);
    if (property.type === PropertyType.Collection)
      return this.createFormArrayFromTemplates(property._templates, ['default']);

    const control = new FormControl(property.value);
    if (property.max)
      control.addValidators(Validators.max(property.max));
    if (property.maxLength)
      control.addValidators(Validators.maxLength(property.maxLength));
    if (property.min)
      control.addValidators(Validators.min(property.min));
    if (property.minLength)
      control.addValidators(Validators.minLength(property.minLength));
    if (property.regex)
      control.addValidators(Validators.pattern(property.regex));
    if (property.required)
      control.addValidators(Validators.required);
    if (property.type === PropertyType.Email)
      control.addValidators(Validators.email);

    return control;
  }
}
