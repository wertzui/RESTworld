import { Component, forwardRef, Input, QueryList, ViewChildren } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { FileUpload } from 'primeng/fileupload';

@Component({
  selector: 'rw-file',
  templateUrl: './restworld-file-view.component.html',
  styleUrls: ['./restworld-file-view.component.css'],
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => RESTWorldFileViewComponent),
    multi: true
  }]
})
export class RESTWorldFileViewComponent implements ControlValueAccessor {
  private onChange?: Function;
  @Input()
  public accept?: string;

  @Input()
  public fileName?: string;

  @ViewChildren(FileUpload)
  fileUploads?: QueryList<FileUpload>;

  public disabled = false;
  public uri?: string;

  constructor() { }

  writeValue(obj: any): void {
    this.uri = obj;
  }
  registerOnChange(fn: any): void {
    this.onChange = fn;
  }
  registerOnTouched(fn: any): void {
  }
  setDisabledState?(isDisabled: boolean): void {
    this.disabled = isDisabled;
  }

  public fileChanged(event: { files: File[] }): void {
    const file = event.files[0];
    const reader = new FileReader();
    reader.onload = e => {
      this.uri = reader.result as string;
      this.onChange?.(this.uri);
    };

    reader.readAsDataURL(file);
  }
}
