import { Component, forwardRef, Input, QueryList, ViewChildren } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { ImageCroppedEvent, OutputFormat } from 'ngx-image-cropper';
import { FileUpload } from 'primeng/fileupload';

@Component({
  selector: 'rw-image',
  templateUrl: './restworld-image-view.component.html',
  styleUrls: ['./restworld-image-view.component.css'],
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => RESTWorldImageViewComponent),
    multi: true
  }]
})
export class RESTWorldImageViewComponent implements ControlValueAccessor {

  private onChange?: Function;

  @Input()
  public alt?: string;
  @Input()
  public accept?: string;
  @Input()
  public fileName?: string;
  @Input()
  maintainAspectRatio = true;
  @Input()
  aspectRatio = 1;
  @Input()
  resizeToWidth = 0;
  @Input()
  resizeToHeight = 0;
  @Input()
  onlyScaleDown = false;
  @Input()
  containWithinAspectRatio = false;
  @Input()
  backgroundColor = "#ffffff";
  @Input()
  public format: OutputFormat = 'png';


  @ViewChildren(FileUpload)
  fileUploads?: QueryList<FileUpload>;


  public disabled = false;
  public uri?: string | null;
  public tempImageFile?: File;
  public displayCropDialog = false;
  public tempCroppedUri?: string | null;

  writeValue(obj?: string | null): void {
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

  public showCropDialog(): void {
    this.displayCropDialog = true;
  }

  public imageChanged(event: { files: File[] }): void {
    this.tempImageFile = event.files[0];
    this.showCropDialog();
  }

  public croppedImageChanged(event: ImageCroppedEvent): void {
    this.tempCroppedUri = event.base64;
  }

  public acceptCroppedImage(): void {
    this.uri = this.tempCroppedUri;
    this.onChange?.(this.uri);
    this.closeCropDialog();
  }

  public closeCropDialog(): void {
    this.fileUploads?.forEach(f => f.clear());
    this.displayCropDialog = false;
  }
}
