import { Component, forwardRef, Input, QueryList, ViewChildren } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { FileUpload } from 'primeng/fileupload';

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
  public accept?: string;

  @Input()
  public fileName?: string;

  @ViewChildren(FileUpload)
  fileUploads?: QueryList<FileUpload>;

  public disabled = false;
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
