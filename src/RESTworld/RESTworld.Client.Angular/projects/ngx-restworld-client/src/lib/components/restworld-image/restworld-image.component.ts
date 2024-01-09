import { AfterViewInit, Component, forwardRef, Input, OnInit, QueryList, ViewChildren } from '@angular/core';
import { ControlValueAccessor, NG_VALUE_ACCESSOR } from '@angular/forms';
import { ImageCroppedEvent, ImageCropperComponent } from 'ngx-image-cropper';
import { FileUpload } from 'primeng/fileupload';
import { Dialog } from 'primeng/dialog'
import { Property } from '@wertzui/ngx-hal-client';
import { RestWorldImage } from '../../models/restworld-image'

/**
 * A component for displaying and editing images with various options such as cropping, resizing, and aspect ratio.
 * Implements ControlValueAccessor to work with Angular forms.
 */
@Component({
  selector: 'rw-image',
  templateUrl: './restworld-image.component.html',
  styleUrls: ['./restworld-image.component.css'],
  providers: [{
    provide: NG_VALUE_ACCESSOR,
    useExisting: forwardRef(() => RestWorldImageComponent),
    multi: true
  }]
})
export class RestWorldImageComponent implements ControlValueAccessor, OnInit, AfterViewInit {

  private onChange?: Function;

  @Input({ required: true })
  public property!: Property & { restWorldImage: RestWorldImage}

  public get alt() {
    return this.property.prompt ?? this.property.name;
  }

  public get accept(): string | undefined {
    return this.property.restWorldImage.accept ?? typeof this.property.placeholder === "string" ? this.property.placeholder as string : "image/*";
  }

  public get fileName() {
    return this.property.name + "." + (this.property.restWorldImage.format ?? ".png");
  }

  public get alignImage() {
    return this.property.restWorldImage.alignImage;
  }

  public get aspectRatio() {
    return this.property.restWorldImage.aspectRatio;
  }

  public get backgroundColor() {
    return this.property.restWorldImage.backgroundColor;
  }
  public set backgroundColor(value: string | undefined) {
    this.property.restWorldImage.backgroundColor = value;
  }

  public get canvasRotation() {
    return this.property.restWorldImage.canvasRotation;
  }

  public get containWithinAspectRatio() {
    return this.property.restWorldImage.containWithinAspectRatio;
  }

  public get cropper() {
    return this.property.restWorldImage.cropper;
  }

  public get cropperMaxHeight() {
    return this.property.restWorldImage.cropperMaxHeight;
  }

  public get cropperMaxWidth() {
    return this.property.restWorldImage.cropperMaxWidth;
  }

  public get cropperMinHeight() {
    return this.property.restWorldImage.cropperMinHeight;
  }

  public get cropperMinWidth() {
    return this.property.restWorldImage.cropperMinWidth;
  }

  public get cropperStaticHeight() {
    return this.property.restWorldImage.cropperStaticHeight;
  }

  public get cropperStaticWidth() {
    return this.property.restWorldImage.cropperStaticWidth;
  }

  public get format() {
    return this.property.restWorldImage.format;
  }

  public get imageQuality() {
    return this.property.restWorldImage.imageQuality;
  }

  public get initialStepSize() {
    return this.property.restWorldImage.initialStepSize;
  }

  public get maintainAspectRatio() {
    return this.property.restWorldImage.maintainAspectRatio;
  }

  public get onlyScaleDown() {
    return this.property.restWorldImage.onlyScaleDown;
  }

  public get resizeToWidth() {
    return this.property.restWorldImage.resizeToWidth;
  }

  public get resizeToHeight() {
    return this.property.restWorldImage.resizeToHeight;
  }

  public get roundCropper() {
    return this.property.restWorldImage.roundCropper;
  }

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
