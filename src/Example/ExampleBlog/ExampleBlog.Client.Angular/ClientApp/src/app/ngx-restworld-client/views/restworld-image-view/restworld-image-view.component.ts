import { AfterViewInit, Component, forwardRef, Input, OnDestroy, OnInit, QueryList, ViewChildren } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { CropperPosition, ImageCroppedEvent, ImageCropperComponent, OutputFormat } from 'ngx-image-cropper';
import { FileUpload } from 'primeng/fileupload';
import { Dialog } from 'primeng/dialog'
import { async, firstValueFrom, Subscription } from 'rxjs';

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
export class RESTWorldImageViewComponent implements ControlValueAccessor, OnInit, OnDestroy, AfterViewInit {

  private onChange?: Function;
  private _subscriptions?: Subscription[];

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

  @ViewChildren(Dialog)
  dialogs?: QueryList<Dialog>;

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

  ngOnInit(): void {
    // If no background color is set, we set it to white.
    // Otherwise the color picker would show red, the input would be empty and the image cropper would show transparent.
    if (!this.backgroundColor)
      this.backgroundColor = '#ffffff';
  }

  ngOnDestroy(): void {
    this._subscriptions.forEach(s => s.unsubscribe());
  }

  ngAfterViewInit(): void {
    // We need to trigger imageLoadedInView each time, after the opening animation of the dialog has been completed.
    // Otherwise the image cropper initially (and after every window resize) thinks that the image size is 0x0,
    // because the opening animation hast just begun when the image cropper is first shown.
    this.dialogs?.map(d => d.onShow.subscribe(() =>
      this.imageCroppers?.forEach(i => { i.imageLoadedInView(); })
    ));
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
