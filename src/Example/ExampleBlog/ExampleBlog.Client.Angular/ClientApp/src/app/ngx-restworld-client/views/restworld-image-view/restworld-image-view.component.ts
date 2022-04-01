import { Component, forwardRef, Input, QueryList, ViewChildren } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CropperPosition, ImageCroppedEvent, ImageCropperComponent, OutputFormat } from 'ngx-image-cropper';
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
  private _dialogHasBeenOpenedBefore = false;

  @Input()
  public alt?: string;
  @Input()
  public accept?: string;
  @Input()
  public fileName?: string;
  @Input()
  public alignImage?: 'center' | 'left';
  @Input()
  public aspectRatio?: number;
  @Input()
  public backgroundColor?: string;
  @Input()
  public canvasRotation?: number;
  @Input()
  public containWithinAspectRatio?: boolean;
  @Input()
  public cropper?: CropperPosition;
  @Input()
  public cropperMaxHeight?: number;
  @Input()
  public cropperMaxWidth?: number;
  @Input()
  public cropperMinHeight?: number;
  @Input()
  public cropperMinWidth?: number;
  @Input()
  public cropperStaticHeight?: number;
  @Input()
  public cropperStaticWidth?: number;
  @Input()
  public format?: OutputFormat;
  @Input()
  public imageQuality?: number;
  @Input()
  public initialStepSize?: number;
  @Input()
  public maintainAspectRatio?: boolean;
  @Input()
  public onlyScaleDown?: boolean;
  @Input()
  public resizeToWidth?: number;
  @Input()
  public resizeToHeight?: number;
  @Input()
  public roundCropper?: boolean;



  @ViewChildren(FileUpload)
  fileUploads?: QueryList<FileUpload>;


  @ViewChildren(ImageCropperComponent)
  imageCroppers?: QueryList<ImageCropperComponent>;


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

    // The image cropper has a bug that stops the image from being visible otherwise.
    // Changing the image or resizing the window would also cause the image to appear.
    if (!this._dialogHasBeenOpenedBefore) {
      this._dialogHasBeenOpenedBefore = true;
      this.imageCroppers?.forEach(i => i.onResize());
    }
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
