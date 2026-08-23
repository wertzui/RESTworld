import { Component, ContentChild, ElementRef, Injector, TemplateRef, computed, contentChild, effect, inject, input, model, output, runInInjectionContext, signal, untracked } from '@angular/core';
import { submit, type FieldTree } from '@angular/forms/signals';
import { FormsResource, ProblemDetails, PropertyDto, SignalForm, SimpleValue, SignalFormService, Template } from '@wertzui/ngx-hal-client';
import { ConfirmationService, MessageService } from 'primeng/api';
import { RestWorldClientCollection } from '../../services/restworld-client-collection';
import { AfterSubmitOkEvent, AfterSubmitRedirectEvent } from '../../models/events';
import { RestWorldSignalInputTemplateComponent } from '../restworld-signal-inputs/restworld-signal-input-template/restworld-signal-input-template.component';
import { RestWorldSignalValidationErrorsComponent } from '../restworld-signal-validation-errors/restworld-signal-validation-errors.component';
import { ProgressSpinnerModule } from 'primeng/progressspinner';
import { ButtonModule } from 'primeng/button';
import { RippleModule } from 'primeng/ripple';
import { NgTemplateOutlet } from '@angular/common';
import { ProblemService } from '../../services/problem.service';

/**
 * A form with Save, Reload and Delete buttons, using Signal Forms.
 * This is the Signal Forms equivalent of {@link RestWorldFormComponent} `<rw-form>`.
 * @remarks
 * Flat templates as well as nested `Object` and `Collection` properties are supported (see
 * {@link RestWorldSignalInputObjectComponent} `<rw-signal-input-object>` and
 * {@link RestWorldSignalInputCollectionComponent} `<rw-signal-input-collection>`).
 * If you do not want buttons, use {@link RestWorldSignalInputTemplateComponent} `<rw-signal-input-template>`.
 * @example
 * <rw-signal-form
 *  apiName="apiName"
 *  rel="rel"
 *  [(template)]="template">
 * </rw-signal-form>
 */
@Component({
    selector: 'rw-signal-form',
    templateUrl: './restworld-signal-form.component.html',
    styleUrls: ['./restworld-signal-form.component.css'],
    imports: [RestWorldSignalInputTemplateComponent, RestWorldSignalValidationErrorsComponent, ProgressSpinnerModule, ButtonModule, RippleModule, NgTemplateOutlet]
})
export class RestWorldSignalFormComponent<TPropertyDtos extends ReadonlyArray<PropertyDto<SimpleValue, string, string>> = ReadonlyArray<PropertyDto<SimpleValue, string, string>>> {
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
     * The signal form (model signal + field tree) created from the current template.
     * @remarks `form()` (used internally by {@link SignalFormService.createSignalFormFromTemplate}) internally calls
     * `effect()`, which cannot be called from a `computed()` derivation (NG0602). It is therefore (re-)created in a
     * constructor `effect()` that reacts to `template()` changes and written into this plain signal instead.
     */
    public readonly signalForm = signal<SignalForm<Record<string, unknown>>>(undefined!);
    /**
     * The field tree that represents the form.
     */
    public readonly form = computed(() => this.signalForm().form as FieldTree<Record<string, unknown>>);
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
     * Determines whether the resource can be deleted.
     */
    public readonly canDelete = computed(() =>
        this.allowDelete() &&
        this.template() !== undefined &&
        this.template().target !== undefined &&
        this.template().method == "PUT" &&
        this.signalForm().model()["id"] !== undefined &&
        this.signalForm().model()["timestamp"] !== undefined &&
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
        !this.isLoading());

    /**
     * A reference to a template that can be used to render custom buttons for the form.
    */
    public readonly buttonsRef = contentChild<TemplateRef<unknown>>('buttons');
    /**
     * A reference to a template that can be used to render content before the default buttons.
    */
    public readonly beforeButtonsRef = contentChild<TemplateRef<unknown>>('beforeButtons');
    /**
     * A reference to a template that can be used to render content after the default buttons.
    */
    public readonly afterButtonsRef = contentChild<TemplateRef<unknown>>('afterButtons');
    /**
     * A reference to a template that can be used to render custom content inside the <form> element instead of the default form.
    */
    public readonly contentRef = contentChild<TemplateRef<unknown>>('content');

    private readonly _client = computed(() => this._clients.getClient(this.apiName()));
    private readonly _injector = inject(Injector);
    constructor(
        private readonly _clients: RestWorldClientCollection,
        private readonly _confirmationService: ConfirmationService,
        private readonly _messageService: MessageService,
        private readonly _signalFormService: SignalFormService,
        private readonly _elementRef: ElementRef<HTMLElement>,
        private readonly _problemService: ProblemService) {
            // Rebuild the signal form whenever the template changes. This must be a constructor effect (not a
            // computed()) because `form()` internally calls `effect()`, which is not allowed inside computed().
            // `form()` also calls `inject()` internally, so the creation must explicitly run inside an injection
            // context via `runInInjectionContext()`. Additionally, `form()`'s internal `effect()` call asserts it
            // is not itself running inside a reactive context, so the creation must also be `untracked()` to escape
            // this effect's own reactive context (the effect callback itself is a reactive/tracked context).
            effect(() => {
                const template = this.template();
                const signalForm = untracked(() => runInInjectionContext(this._injector, () => this._signalFormService.createSignalFormFromTemplate(template)));
                this.signalForm.set(signalForm as SignalForm<Record<string, unknown>>);
            });
    }

    public async delete(): Promise<void> {
        if (!this.canDelete())
            return;

        const template = this.template();
        const timestamp = this.signalForm().model()["timestamp"] as string;

        const result = await this._client().deleteByUrl(template.target!, timestamp);
        if (result.ok) {
            this._messageService.add({ severity: 'success', summary: 'Deleted', detail: 'The resource has been deleted.' })
            this.afterDelete.emit();
        }
        else if (ProblemDetails.isProblemDetails(result.body)) {
            this._problemService.displayToast(result.body, "Error while deleting the resource");
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
            if (response.ok) {
                this.template.set((response.body as FormsResource).getTemplateByTitle(template.title!) as Template<TPropertyDtos>);
            }
            else if (ProblemDetails.isProblemDetails(response.body)) {
                this._problemService.displayToast(response.body, "Error while reloading the resource");
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
        const template = this.template();
        const signalForm = this.signalForm();

        if (!this.canSubmit())
            return;

        await submit(signalForm.form, async () => {
            this.isLoading.set(true);

            try {
                const response = await this._client().submit(template, signalForm.model());

                if (!response.ok) {
                    if (ProblemDetails.isProblemDetails(response.body)) {
                        this._problemService.displayToast(response.body, "Error while saving the resource");

                        return this._problemService.problemDetailsToSignalFormValidationErrors(response.body, signalForm.form as FieldTree<Record<string, unknown>>);
                    }
                }
                else if (response.status == 201) {
                    if (!response.headers.has('Location')) {
                        this._messageService.add({ severity: 'error', summary: 'Error', detail: 'The server returned a 201 Created response, but did not return a Location header.', data: response, sticky: true });
                        return;
                    }

                    this._messageService.add({ severity: 'success', summary: 'Created', detail: 'The resource has been created.' });

                    const createdAtUri = response.headers.get('Location')!;
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
            finally {
                this.isLoading.set(false);
            }

            return undefined;
        });

        if (this.form()().invalid()) {
            this._messageService.add({
                severity: 'error',
                summary: 'Error',
                detail: 'Please correct the errors before submitting.',
            });

            ProblemService.scrollToFirstValidationError(this._elementRef.nativeElement);
        }
    }
}
