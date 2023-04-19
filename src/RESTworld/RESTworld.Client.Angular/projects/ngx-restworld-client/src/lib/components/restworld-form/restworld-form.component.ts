import { Component, ContentChild, EventEmitter, Input, OnInit, Output, TemplateRef } from '@angular/core';
import { AbstractControl, FormGroup, UntypedFormArray, UntypedFormGroup } from '@angular/forms';
import { Router } from '@angular/router';
import { FormsResource, Template } from '@wertzui/ngx-hal-client';
import { ConfirmationService, MessageService } from 'primeng/api';
import { ProblemDetails } from '../../models/problem-details';
import { FormService } from '../../services/form.service';
import { RestWorldClient } from '../../services/restworld-client';
import { RestWorldClientCollection } from '../../services/restworld-client-collection';

/**
 * A form with Save, Reload and Delete buttons.
 * If you do not want buttons, use RestWorldFormTemplateComponent <rw-form-template>.
 * You can also provide your own buttons by passing in a template.
 */
@Component({
  selector: 'rw-form',
  templateUrl: './restworld-form.component.html',
  styleUrls: ['./restworld-form.component.css']
})
export class RestWorldFormComponent implements OnInit{
  @Input()
  template!: Template;

  @Input()
  apiName!: string;

  @Input()
  rel!: string;

  @Input()
  allowSubmit = true;

  @Input()
  allowDelete = true;

  @Input()
  allowReload = true;

  @Input()
  showSubmit = true;

  @Input()
  showDelete = true;

  @Input()
  showReload = true;

  @Output()
  afterDelete = new EventEmitter<void>();

  @Output()
  afterSubmit = new EventEmitter<{old: Template, new: Template}>();

  @ContentChild('buttons', { static: false })
  buttonsRef?: TemplateRef<unknown>;

  private _isLoading = false;
  public get isLoading(): boolean {
    return this._isLoading;
  }

  private _formGroup?: FormGroup<{timestamp: AbstractControl<string>}>;
  public get formGroup(): FormGroup<{timestamp: AbstractControl<string>}> | undefined {
    return this._formGroup;
  }

  public get canSubmit() : boolean {
    return this.allowSubmit &&
      this.template.target !== undefined &&
      !this.isLoading;
  }

  public get canDelete(): boolean {
    return this.allowDelete &&
      this.template.target !== undefined &&
      this.template.method == "PUT" &&
      !this.isLoading;
  }

  public get canReload(): boolean {
    return this.allowReload &&
      this.template.target !== undefined &&
      this.template.title !== undefined &&
      this.template.properties.some(p => p.name === "id" && p.value !== undefined && p.value !== null && p.value !== 0) &&
      !this.isLoading;
  }

  constructor(
    private readonly _clients: RestWorldClientCollection,
    private readonly _confirmationService: ConfirmationService,
    private readonly _messageService: MessageService,
    private readonly _formService: FormService) {
  }

  ngOnInit(): void {
    this._formGroup = this._formService.createFormGroupFromTemplate(this.template);
  }

  public async reload(): Promise<void> {
    if (!this.canReload)
      return;

    this._isLoading = true;

    try
    {
      const response = await this.getClient().getForm(this.template.target!);
      if (!response.ok || ProblemDetails.isProblemDetails(response.body) || !response.body) {
        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'Error while loading the resource from the API.', data: response, sticky: true });
      }
      else {
        this.template = response.body.getTemplateByTitle(this.template.title!);
        this._formGroup = this._formService.createFormGroupFromTemplate(this.template);
      }
    }
    catch (e: unknown) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: `An unknown error occurred. ${JSON.stringify(e)}`, sticky: true });
      console.log(e);
    }

    this._isLoading = false;
  }

  public showDeleteConfirmatioModal() {
    this._confirmationService.confirm({
      message: 'Do you really want to delete this resource?',
      header: 'Confirm delete',
      icon: 'far fa-trash-alt',
      accept: () => this.delete()
    });
  }

  public async submit() {
    if(!this.canSubmit)
      return;

    this._isLoading = true;

    try {
      const response = await this.getClient().submit(this.template, this.formGroup!.value);

      if (!response.ok) {
        let summary = 'Error';
        let detail = 'Error while saving the resource.';
        if (ProblemDetails.isProblemDetails(response.body)) {
          const problemDetails = response.body as ProblemDetails;
          summary = problemDetails.title || summary;
          detail = problemDetails.detail || detail;
          // display validation errors
          if (problemDetails['errors'] as {}) {
            for (const [key, errorsForKey] of Object.entries(problemDetails['errors'] as {})) {
              const path = key.split(/\.|\[/).map(e => e.replace("]", ""));
              // The path might start with a $, indicating the root.
              if (path.length > 0 && path[0] === '$')
                path.shift();
              const formControl = path.reduce<AbstractControl>(RestWorldFormComponent.getSubControl, this.formGroup!);
              formControl.setErrors({ ...formControl.errors, ...{ remote: errorsForKey } });
              formControl.markAsTouched();
            }
          }
        }

        this._messageService.add({ severity: 'error', summary: summary, detail: detail, data: response, sticky: true });
      }
      else {
        const templateBeforeSubmit = this.template;
        const responseResource = (response.body as FormsResource);
        const templateAfterSubmit = responseResource.getTemplateByTitle(this.template.title!);

        this._messageService.add({ severity: 'success', summary: 'Saved', detail: 'The resource has been saved.' });

        this.afterSubmit.emit({old: templateBeforeSubmit, new: templateAfterSubmit});
      }
    }
    catch (e: unknown) {
      this._messageService.add({ severity: 'error', summary: 'Error', detail: `An unknown error occurred. ${JSON.stringify(e)}`, sticky: true });
      console.log(e);
    }

    this._isLoading = false;
  }

  public async delete(): Promise<void> {
    if (!this.canDelete)
      return;

    if (this.formGroup === undefined)
      throw new Error("formGroup cannot be undefined.");

    await this.getClient().deleteByTemplateAndForm(this.template, this.formGroup);
    this._messageService.add({ severity: 'success', summary: 'Deleted', detail: 'The resource has been deleted.' })

    this.afterDelete.emit();
  }

  private static getSubControl(control: AbstractControl, pathElement: string): AbstractControl {
    if (pathElement === "")
      return control;

    if (control instanceof UntypedFormGroup)
      return control.controls[pathElement];

    if (control instanceof UntypedFormArray) {
      const index = Number.parseInt(pathElement);
      if (Number.isInteger(index))
        return control.controls[index];
    }

    return control;
  }

  private getClient(): RestWorldClient {
    return this._clients.getClient(this.apiName);
  }
}
