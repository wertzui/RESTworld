import { Component, ContentChild, ElementRef, TemplateRef, computed, contentChild, effect, input, model, output, signal } from '@angular/core';
import { AbstractControl, FormGroup, ReactiveFormsModule, UntypedFormArray, UntypedFormGroup } from '@angular/forms';
import { FormService, FormsResource, ProblemDetails, PropertyDto, SimpleValue, Template } from '@wertzui/ngx-hal-client';
import { ConfirmationService, MessageService } from 'primeng/api';
import { RestWorldClientCollection } from '../../services/restworld-client-collection';
import { AfterSubmitOkEvent, AfterSubmitRedirectEvent } from '../../models/events';
import { Subscription } from 'rxjs';
import { RestWorldValidationErrorsComponent } from "../restworld-validation-errors/restworld-validation-errors.component";
import { RestWorldInputTemplateComponent } from "../restworld-inputs/restworld-inputs";
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ButtonModule } from "primeng/button";
import { RippleModule } from "primeng/ripple";
import { NgTemplateOutlet } from "@angular/common";
import { ProblemService } from "../../services/problem.service";

/**
 * A form with Save, Reload and Delete buttons.
 * If you do not want buttons, use RestWorldFormTemplateComponent <rw-form-template>.
 * You can also provide your own buttons by passing in a template.
 * @example
 * <rw-form
 *  [template]="template"
 *  apiName="apiName"
 *  rel="rel"
 *  [allowSubmit]="true"
 *  [allowDelete]="true"
 *  [allowReload]="true"
 *  [showSubmit]="true"
 *  [showDelete]="true"
 *  [showReload]="true">
 *  <ng-template #content let-form="form" let-template="template" let-apiName="apiName">
 *    <!-- Custom form content here -->
 *    <!-- This is optional and will replace the default which renders labels and inputs if present -->
 *  </ng-template>
 *  <ng-template #buttons let-form="form" let-template="template" let-apiName="apiName">
 *    <!-- Custom buttons here -->
 *    <!-- This is optional and will replace the default which renders the Save, Reload and Delete buttons if present -->
 *  </ng-template>
 * </rw-form>
 */
@Component({
    selector: 'rw-form',
    templateUrl: './restworld-form.component.html',
    styleUrls: ['./restworld-form.component.css'],
    standalone: true,
    imports: [ReactiveFormsModule, RestWorldValidationErrorsComponent, RestWorldInputTemplateComponent, ProgressSpinnerModule, ButtonModule, RippleModule, NgTemplateOutlet]
})
export class RestWorldFormComponent<TPropertyDtos extends ReadonlyArray<PropertyDto<SimpleValue, string, string>>> {
    /**
     * Emitted after the resource has been deleted.
     */
    public readonly afterDelete = output<void>();
    /**
     * Emitted after the form has been submitted.
     */
    public readonly afterSubmit = output<AfterSubmitOkEvent | AfterSubmitRedirectEvent>();
    /**
     * Determines whether to enable the delete button.
    */
    public readonly allowDelete = input(true);
    /**
     * Determines whether to enable the reload button.
    */
    public readonly allowReload = input(true);
    /**
     * Determines whether to enable the submit button.
    */
    public readonly allowSubmit = input(true);
    /**
     * The name of the API to use.
     */
    public readonly apiName = input.required<string>();
    /**
     * Determines whether the resource can be deleted.
     */
    public readonly canDelete = computed(() =>
        this.allowDelete() &&
        this.template() !== undefined &&
        this.template().target !== undefined &&
        this.template().method == "PUT" &&
        this.formGroup() !== undefined &&
        this.formGroup()?.value.id !== undefined &&
        this.formGroup()?.value.timestamp !== undefined &&
        !this.isLoading());
    /**
     * Determines whether the form can be reloaded.
     */
    public readonly canReload = computed(() =>
        this.allowReload() &&
        this.template() !== undefined &&
        this.template().target !== undefined &&
        this.template().title !== undefined &&
        this.template().properties.some(p => p.name === "id" && p.value !== undefined && p.value !== null && p.value !== 0) &&
        !this.isLoading());
    /**
     * Determines whether the form can be submitted.
     */
    public readonly canSubmit = computed(() =>
        this.allowSubmit() &&
        this.template() !== undefined &&
        this.template().target !== undefined &&
        !this.isLoading() &&
        this.formGroup() !== undefined);
    /**
     * The form group that represents the form.
     */
    public readonly formGroup = computed(() => this._formService.createFormGroupFromTemplate(this.template()));
    public readonly isLoading = signal(false);
    /**
     * The rel of the form.
     */
    public readonly rel = input.required<string>();
    /**
     * Determines whether to show the delete button.
    */
    public readonly showDelete = input(true);
    /**
     * Determines whether to show the reload button.
    */
    public readonly showReload = input(true);
    /**
     * Determines whether to show the submit button.
    */
    public readonly showSubmit = input(true);
    /**
     * The template used to render the form.
     */
    public readonly template = model.required<Template<TPropertyDtos>>();
    /**
     * Emitted when the form value changes.
     */
    public readonly valueChanges = output<any>();

    /**
     * A reference to a template that can be used to render custom buttons for the form.
    */
    public readonly buttonsRef = contentChild<TemplateRef<unknown>>('buttons');
    /**
     * A reference to a template that can be used to render custom content inside the <form> element instead of the default form.
    */
    public readonly contentRef = contentChild<TemplateRef<unknown>>('content');

    private readonly _client = computed(() => this._clients.getClient(this.apiName()));

    private _formValueChangesSubscription?: Subscription;

    constructor(
        private readonly _clients: RestWorldClientCollection,
        private readonly _confirmationService: ConfirmationService,
        private readonly _messageService: MessageService,
        private readonly _formService: FormService,
        private readonly _elementRef: ElementRef<HTMLElement>,
        private readonly _problemService: ProblemService) {
            // Update the form value changes subscription to always track the current form group.
            effect(() => {
                this._formValueChangesSubscription?.unsubscribe();
                const formGroup = this.formGroup();
                this._formValueChangesSubscription = formGroup?.valueChanges.subscribe(newValue => this.valueChanges.emit(newValue));
                this.valueChanges.emit(formGroup?.value);
            });
    }

    public async delete(): Promise<void> {
        if (!this.canDelete())
            return;

        const formGroup = this.formGroup();
        if (formGroup === undefined)
            throw new Error("formGroup cannot be undefined.");

        const template = this.template();
        if (template === undefined)
            throw new Error("template cannot be undefined.");

        // canDelete already checks that the timestamp is present, so the cast is safe.
        const result = await this._client().deleteByTemplateAndForm(template, formGroup as unknown as FormGroup<{ timestamp: AbstractControl<string> }>);
        if (this._problemService.checkResponseAndDisplayErrors(result, formGroup)) {
            this._messageService.add({ severity: 'success', summary: 'Deleted', detail: 'The resource has been deleted.' })
            this.afterDelete.emit();
        }
    }

    public async reload(): Promise<void> {
        const canReload = this.canReload();
        const template = this.template();
        if (!canReload || template === undefined)
            return;

        this.isLoading.set(true);

        try {
            const response = await this._client().getForm(template.target!);
            if (this._problemService.checkResponseAndDisplayErrors(response, this.formGroup())) {
                this.template.set(response.body!.getTemplateByTitle(template.title!) as Template<TPropertyDtos>);
            }
        }
        catch (e: unknown) {
            this._messageService.add({ severity: 'error', summary: 'Error', detail: `An unknown error occurred. ${JSON.stringify(e)}`, sticky: true });
            console.log(e);
        }

        this.isLoading.set(false);
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
        const formGroup = this.formGroup();
        const template = this.template();

        if (formGroup !== undefined) {
            formGroup.markAllAsTouched();

            if (!formGroup.valid) {
                this._messageService.add({
                    severity: 'error',
                    summary: 'Error',
                    detail: 'Please correct the errors before submitting.',
                });

                ProblemService.scrollToFirstValidationError(this._elementRef.nativeElement);
                return;
            }
        }

        if (!this.canSubmit() || formGroup === undefined || template === undefined)
            return;

        this.isLoading.set(true);

        try {
            const response = await this._client().submit(template, formGroup.value);

            if (!this._problemService.checkResponseAndDisplayErrors(response, formGroup, "Error while saving the resource")) {
            }
            else if (response.status == 201) {
                if (!response.headers.has('Location')) {
                    this._messageService.add({ severity: 'error', summary: 'Error', detail: 'The server returned a 201 Created response, but did not return a Location header.', data: response, sticky: true });
                    return;
                }

                this._messageService.add({ severity: 'success', summary: 'Created', detail: 'The resource has been created.' });

                var createdAtUri = response.headers.get('Location')!;
                this.afterSubmit.emit({ location: createdAtUri, status: 201 });
            }
            else {
                const responseResource = (response.body as FormsResource);
                const newTemplate = responseResource.getTemplateByTitle(template.title!) as Template<TPropertyDtos>;
                this.template.set(newTemplate);

                this._messageService.add({ severity: 'success', summary: 'Saved', detail: 'The resource has been saved.' });

                this.afterSubmit.emit({ old: template, new: newTemplate, status: 200 });
            }
        }
        catch (e: unknown) {
            this._messageService.add({ severity: 'error', summary: 'Error', detail: `An unknown error occurred. ${JSON.stringify(e)}`, sticky: true });
            console.log(e);
        }

        this.isLoading.set(false);
    }
}
