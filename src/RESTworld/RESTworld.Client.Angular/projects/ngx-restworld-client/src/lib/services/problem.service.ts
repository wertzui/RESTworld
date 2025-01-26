import type { HttpResponse } from "@angular/common/http";
import { Injectable } from "@angular/core";
import { UntypedFormArray, UntypedFormGroup, type AbstractControl, type FormGroup } from "@angular/forms";
import { ProblemDetails } from "@wertzui/ngx-hal-client";
import { MessageService } from "primeng/api";

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
     * @param nativeElement Any parent element of the <rw-validation-errors>. If not provided, the document will be used.
     */
    public static scrollToFirstValidationError(nativeElement?: HTMLElement): void {
        const enclosingElement = nativeElement ?? document;
        setTimeout(() => {
            const validationErrorElements = enclosingElement.querySelectorAll('rw-validation-errors>val-errors>div')
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
    public displayToast(problemDetails: ProblemDetails, defaultDetail: string = "Error", defaultTitle: string = "Error"): void {
        const summary = problemDetails.title ?? defaultTitle;
        const detail = problemDetails.detail ?? defaultDetail;

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
            for (const [key, errorsForKey] of Object.entries(problemDetails["errors"] as {})) {
                const path = key.split(/\.|\[/).map(e => e.replace("]", ""));
                // The path might start with a $, indicating the root.
                if (path.length > 0 && path[0] === "$")
                    path.shift();
                const formControl = path.reduce<AbstractControl | undefined>(ProblemService.getSubControl, formGroup);
                if (formControl) {
                    formControl.setErrors({ ...formControl.errors, ...{ remote: errorsForKey } });
                    formControl.markAsTouched();
                }
            }
        }

        ProblemService.scrollToFirstValidationError(nativeElement);
    }
}
