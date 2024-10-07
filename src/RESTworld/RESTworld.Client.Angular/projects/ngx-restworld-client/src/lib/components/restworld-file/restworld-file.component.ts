import { Component, forwardRef, Input, QueryList, ViewChildren } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { FileUpload } from 'primeng/fileupload';

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
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => RestWorldFileComponent),
    multi: true
  }]
})
export class RestWorldFileComponent implements ControlValueAccessor {
  private onChange?: Function;
  @Input()
  /**
   * The file types that the component should accept.
   * This is a comma-separated list of MIME types or file extensions.
   * If not specified, all file types are accepted.
   */
  public accept?: string;

  @Input()
  /**
   * The name of the file to be uploaded.
   */
  public fileName?: string;

  @ViewChildren(FileUpload)
  fileUploads?: QueryList<FileUpload>;

  public disabled = false;
  /**
   * The URI of the file.
   */
  public uri?: string;

  writeValue(obj?: string): void {
    this.uri = obj;
  }
  registerOnChange(fn?: Function): void {
    this.onChange = fn;
  }
  registerOnTouched(): void {
    // not needed for this component, but needed to implement the interface
  }
  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  public fileChanged(event: { files: File[] }): void {
    const file = event.files[0];
    const reader = new FileReader();
    reader.onload = () => {
      this.uri = reader.result as string;
      this.onChange?.(this.uri);
    };

    reader.readAsDataURL(file);
  }
}
