import { Component, forwardRef, QueryList, ViewChildren, input, signal } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { ButtonModule } from "primeng/button";
import { FileUpload, FileUploadModule } from 'primeng/fileupload';
import { SafeUrlPipe } from "../../pipes/safe-url.pipe";

/**
 * Represents a component for uploading files in the RESTworld Angular client.
 * Implements the ControlValueAccessor interface to enable two-way data binding.
 * @example
 * <rw-file
 *  [(ngModel)]="base64EncodedFileUri"
 *  accept="file/text">
 * </rw-file>
 */
@Component({
    selector: 'rw-file',
    templateUrl: './restworld-file.component.html',
    styleUrls: ['./restworld-file.component.css'],
    standalone: true,
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => RestWorldFileComponent),
        multi: true
    }],
    imports: [ButtonModule, FileUploadModule, SafeUrlPipe]
})
export class RestWorldFileComponent implements ControlValueAccessor {
    private onChange?: Function;
    /**
   * The file types that the component should accept.
   * This is a comma-separated list of MIME types or file extensions.
   * If not specified, all file types are accepted.
   */
    public readonly accept = input("", { transform: (v: string) => v ?? "" });

    /**
   * The name of the file to be uploaded.
   */
    public readonly fileName = input("download", { transform: (v: string) => v ?? "download" });

    public readonly disabled = signal(false);
    /**
     * The URI of the file.
     */
    public uri = signal<string | undefined>(undefined);

    writeValue(obj?: string): void {
        this.uri.set(obj);
    }
    registerOnChange(fn?: Function): void {
        this.onChange = fn;
    }
    registerOnTouched(): void {
        // not needed for this component, but needed to implement the interface
    }
    setDisabledState?(isDisabled: boolean): void {
        this.disabled.set(isDisabled);
    }

    public fileChanged(event: { files: File[] }): void {
        const file = event.files[0];
        const reader = new FileReader();
        reader.onload = () => {
            this.uri.set(reader.result as string);
            this.onChange?.(this.uri());
        };

        reader.readAsDataURL(file);
    }

    public deleteFile() {
        this.uri.set(undefined);
        this.onChange?.(this.uri());
    }
}
