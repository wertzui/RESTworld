import type { HttpResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { UntypedFormArray, UntypedFormGroup, type AbstractControl, type FormGroup } from "@angular/forms";
import type { FieldTree, ValidationError } from "@angular/forms/signals";
import { ProblemDetails } from "@wertzui/ngx-hal-client";
import { MessageService } from "primeng/api";
import { getFieldTreeAtPath } from "../util/field-tree";

/**
 * This service is responsible for displaying problems to the user.
 */
@Injectable({
    providedIn: "root"
})
export class ProblemService {
    constructor(private readonly _messageService: MessageService) {
    }

    /**
     * Scrolls to the first validation error.
     * @param nativeElement Any parent element of the <rw-validation-errors> or <rw-signal-validation-errors>. If not provided, the document will be used.
     * @remarks Matches both the Reactive Forms `<rw-validation-errors>` (which renders ngx-valdemort's
     * `<val-errors><div>...`) and the Signal Forms `<rw-signal-validation-errors>` (which renders its own
     * `<div class="rw-signal-validation-errors"><p-message>...`) DOM structures, so this works for forms built
     * with either `<rw-form>`/`<rw-validation-errors>` or `<rw-signal-form>`/`<rw-signal-validation-errors>`.
     */
    public static scrollToFirstValidationError(nativeElement?: HTMLElement): void {
        const enclosingElement = nativeElement ?? document;
        setTimeout(() => {
            const validationErrorElements = enclosingElement.querySelectorAll('rw-validation-errors>val-errors>div, rw-signal-validation-errors>div.rw-signal-validation-errors>p-message');
            if (validationErrorElements.length === 0)
                return;

            const firstError = validationErrorElements[0];
            firstError.scrollIntoView({ behavior: 'smooth', block: 'center' });
        },
            100);
    }

    private static getSubControl(control: AbstractControl | undefined, pathElement: string): AbstractControl | undefined {
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

    /**
     * Adds errors to a control's existing errors.
     * @param control The control to add errors to.
     * @param newErrors The new errors to add.
     */
    public addErrors(control: AbstractControl, newErrors: unknown): void {
        if (!newErrors)
            return;

        // Get existing remote errors or initialize an empty array
        const existingErrors = control.errors?.remote;

        // Combine the errors
        let combinedErrors;
        if (!existingErrors) {
            // No existing errors, just use the new ones
            combinedErrors = [newErrors];
        } else if (Array.isArray(existingErrors)) {
            // Existing errors are already an array, add to it
            combinedErrors = [...existingErrors, newErrors];
        } else {
            // Existing errors are not an array, combine with new errors
            combinedErrors = [existingErrors, newErrors];
        }

        // Set the combined errors
        control.setErrors({ ...control.errors, remote: combinedErrors });
    }

    /**
     * Checks the response and displays errors if necessary.
     * @param response The response to check.
     * @param formGroup The form group to display validation errors in.
     * @param defaultDetail The default detail to display if the problem details do not contain a detail.
     * @param defaultTitle The default title to display if the problem details do not contain a title.
     * @returns True if the response is ok, false otherwise.
     */
    public checkResponseAndDisplayErrors<T>(response: HttpResponse<T | ProblemDetails>, formGroup?: FormGroup, defaultDetail: string = "Error", defaultTitle: string = "Error"): response is HttpResponse<T> & { body: T} {
        if (response.ok)
            return true;

        if (ProblemDetails.isProblemDetails(response.body)) {
            if (formGroup)
                this.displayValidationErrors(response.body, formGroup);

            this.displayToast(response.body, defaultDetail, defaultTitle);
        }
        else {
            this._messageService.add({ severity: "error", summary: defaultTitle, detail: defaultDetail, sticky: true });
        }

        return false;
    }

    /**
     * Checks the response and displays errors if necessary.
     * Throws an error if the response is not ok.
     * @param response The response to check.
     * @param formGroup The form group to display validation errors in.
     * @param defaultDetail The default detail to display if the problem details do not contain a detail.
     * @param defaultTitle The default title to display if the problem details do not contain a title.
     * @returns True if the response is ok, false otherwise.
     */
    public checkResponseDisplayErrorsAndThrow<T>(response: HttpResponse<T | ProblemDetails>, formGroup?: FormGroup, defaultDetail: string = "Error", defaultTitle: string = "Error"): asserts response is HttpResponse<T> & { body: T} {
        if (response.ok)
            return;

        if (ProblemDetails.isProblemDetails(response.body)) {
            if (formGroup)
                this.displayValidationErrors(response.body, formGroup);

            this.displayToastAndThrow(response.body, defaultDetail, defaultTitle);
        }
        else {
            this.displayToastAndThrow(undefined, defaultDetail, defaultTitle);
        }
    }

    /**
     * Displays a toast message with the given problem details.
     * @param problemDetails The problem details to display.
     * @param defaultDetail The default detail to display if the problem details do not contain a detail.
     * @param defaultTitle The default title to display if the problem details do not contain a title.
     */
    public displayToast(problemDetails?: ProblemDetails, defaultDetail: string = "Error", defaultTitle: string = "Error"): void {
        const summary = problemDetails?.title ?? defaultTitle;
        const detail = problemDetails?.detail ?? defaultDetail;

        this._messageService.add({ severity: "error", summary: summary, detail: detail, sticky: true });
    }

    /**
     * Displays a toast message with the given problem details and throws an error.
     * @param problemDetails The problem details to display.
     * @param defaultDetail The default detail to display if the problem details do not contain a detail.
     * @param defaultTitle The default title to display if the problem details do not contain a title.
     */
    public displayToastAndThrow(problemDetails?: ProblemDetails, defaultDetail: string = "Error", defaultTitle: string = "Error"): never {
        const summary = problemDetails?.title ?? defaultTitle;
        const detail = problemDetails?.detail ?? defaultDetail;

        this._messageService.add({ severity: "error", summary: summary, detail: detail, sticky: true });
        throw new Error(`${summary}: ${detail}`);
    }

    /**
     * Displays validation errors in the form.
     * @param problemDetails The problem details containing the validation errors.
     * @param formGroup The form group to display the errors in.
     * @param nativeElement Any parent element of the <rw-validation-errors>. If not provided, the document will be used.
     */
    public displayValidationErrors(problemDetails: ProblemDetails, formGroup: FormGroup, nativeElement?: HTMLElement): void {
        // display validation errors
        if (problemDetails["errors"] as {}) {
            // Add the problem detail to the form group errors
            this.addErrors(formGroup, problemDetails.detail);

            for (const [key, errorsForKey] of Object.entries(problemDetails["errors"] as {})) {
                const path = key.split(/\.|\[/).map(e => e.replace("]", ""));
                // The path might start with a $, indicating the root.
                if (path.length > 0 && path[0] === "$")
                    path.shift();
                const formControl = path.reduce<AbstractControl | undefined>(ProblemService.getSubControl, formGroup);

                if (formControl) {
                    // Add the errors to the form control
                    this.addErrors(formControl, errorsForKey);
                    formControl.markAsTouched();
                }

                // Add the field-specific errors to the form group as well
                this.addErrors(formGroup, errorsForKey);
            }

            formGroup.markAsTouched();
        }

        ProblemService.scrollToFirstValidationError(nativeElement);
    }

    /**
     * Converts the validation errors contained in a `ProblemDetails` response into the `TreeValidationResult`
     * shape expected by Signal Forms' `submit()` action callback.
     * @remarks
     * This is the Signal Forms equivalent of {@link displayValidationErrors}. Rather than mutating
     * `AbstractControl`s directly, it returns an array of `ValidationError.WithOptionalFieldTree` which
     * `submit()` will automatically distribute to the correct fields (via Signal Forms' internal
     * `setSubmissionErrors`), attaching them to the resolved field when the path can be resolved, and
     * falling back to the root field (making them available as whole-form errors) otherwise.
     * @param problemDetails The problem details containing the validation errors.
     * @param rootField The root `FieldTree` of the form to resolve field-specific error paths against.
     * @returns An array of `ValidationError.WithOptionalFieldTree`, one for the overall problem detail (if any)
     * plus one for each field-specific error. Returns an empty array if there are no errors.
     */
    public problemDetailsToSignalFormValidationErrors(problemDetails: ProblemDetails, rootField: FieldTree<Record<string, unknown>>): ValidationError.WithOptionalFieldTree[] {
        const errors: ValidationError.WithOptionalFieldTree[] = [];

        if (problemDetails.detail)
            errors.push({ kind: 'remote', message: problemDetails.detail });

        if (problemDetails["errors"] as {}) {
            for (const [key, errorsForKey] of Object.entries(problemDetails["errors"] as {})) {
                const message = Array.isArray(errorsForKey) ? errorsForKey.join(' ') : String(errorsForKey);
                const fieldTree = getFieldTreeAtPath(rootField, key);

                errors.push({ kind: 'remote', message, fieldTree: fieldTree as FieldTree<unknown> });
            }
        }

        return errors;
    }
}
