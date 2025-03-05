import { AfterViewInit, Component, forwardRef, OnInit, QueryList, ViewChildren, input, viewChildren, signal, computed, model, effect } from '@angular/core';
import { ControlValueAccessor, FormsModule, NG_VALUE_ACCESSOR } from '@angular/forms';
import { ImageCroppedEvent, ImageCropperComponent } from 'ngx-image-cropper';
import { FileUpload, FileUploadModule } from 'primeng/fileupload';
import { Dialog, DialogModule } from 'primeng/dialog'
import { Property } from '@wertzui/ngx-hal-client';
import { RestWorldImage } from '../../models/restworld-image'
import { SafeUrlPipe } from '../../pipes/safe-url.pipe';
import { ButtonModule } from "primeng/button";
import { ColorPickerModule } from "primeng/colorpicker";

/**
 * A component for displaying and editing images with various options such as cropping, resizing, and aspect ratio.
 * Implements ControlValueAccessor to work with Angular forms.
 */
@Component({
    selector: 'rw-image',
    templateUrl: './restworld-image.component.html',
    styleUrls: ['./restworld-image.component.css'],
    standalone: true,
    providers: [{
        provide: NG_VALUE_ACCESSOR,
        useExisting: forwardRef(() => RestWorldImageComponent),
        multi: true
    }],
    imports: [SafeUrlPipe, ButtonModule, ImageCropperComponent, FileUploadModule, DialogModule, ColorPickerModule, FormsModule]
})
export class RestWorldImageComponent implements ControlValueAccessor {
    public readonly property = input.required<Property & {
        restWorldImage: RestWorldImage;
    }>();
    public readonly accept = computed(() => {
        const property = this.property();
        return property.restWorldImage.accept ?? typeof property.placeholder === "string" ? property.placeholder as string : "image/*"
    });
    public readonly alignImage = computed(() => this.property().restWorldImage.alignImage ?? "center");
    public readonly alt = computed(() => {
        const property = this.property();
        return property.prompt ?? property.name;
    });
    public readonly aspectRatio = computed(() => this.property().restWorldImage.aspectRatio ?? 1);
    // If no background color is set, we set it to white.
    // Otherwise the color picker would show red, the input would be empty and the image cropper would show transparent.
    public readonly backgroundColor = model("#ffffff");
    public readonly canvasRotation = computed(() => this.property().restWorldImage.canvasRotation);
    public readonly containWithinAspectRatio = computed(() => this.property().restWorldImage.containWithinAspectRatio);
    public readonly cropper = computed(() => this.property().restWorldImage.cropper);
    public readonly cropperMaxHeight = computed(() => this.property().restWorldImage.cropperMaxHeight);
    public readonly cropperMaxWidth = computed(() => this.property().restWorldImage.cropperMaxWidth);
    public readonly cropperMinHeight = computed(() => this.property().restWorldImage.cropperMinHeight);
    public readonly cropperMinWidth = computed(() => this.property().restWorldImage.cropperMinWidth);
    public readonly cropperStaticHeight = computed(() => this.property().restWorldImage.cropperStaticHeight);
    public readonly cropperStaticWidth = computed(() => this.property().restWorldImage.cropperStaticWidth);
    public readonly dialogs = viewChildren(Dialog);
    public readonly disabled = signal(false);
    public readonly fileUploads = viewChildren(FileUpload);
    public readonly filename = computed(() => this.property().name + "." + (this.property().restWorldImage.format ?? ".png"));
    public readonly format = computed(() => this.property().restWorldImage.format);
    public readonly imageCroppers = viewChildren(ImageCropperComponent);
    public readonly imageQuality = computed(() => this.property().restWorldImage.imageQuality);
    public readonly initialStepSize = computed(() => this.property().restWorldImage.initialStepSize);
    public readonly maintainAspectRatio = computed(() => this.property().restWorldImage.maintainAspectRatio);
    public readonly onlyScaleDown = computed(() => this.property().restWorldImage.onlyScaleDown);
    public readonly resizeToHeight = computed(() => this.property().restWorldImage.resizeToHeight);
    public readonly resizeToWidth = computed(() => this.property().restWorldImage.resizeToWidth);
    public readonly roundCropper = computed(() => this.property().restWorldImage.roundCropper);

    public displayCropDialog = model(false);
    private tempCroppedUri?: string;
    public readonly tempImageFile = signal<File | undefined>(undefined);
    public readonly uri = signal<string | undefined>(undefined);

    private onChange?: Function;

    constructor() {
        effect(() => this.property().restWorldImage.backgroundColor = this.backgroundColor());
        effect(() => this.backgroundColor.set(this.property().restWorldImage.backgroundColor ?? "#ffffff"));

        // We need to trigger imageLoadedInView each time, after the opening animation of the dialog has been completed.
        // Otherwise the image cropper initially (and after every window resize) thinks that the image size is 0x0,
        // because the opening animation hast just begun when the image cropper is first shown.
        effect(() => this.dialogs()?.map(d => d.onShow.subscribe(() =>
            this.imageCroppers()?.forEach(i => { i.imageLoadedInView(); })
        )));
    }

    public acceptCroppedImage(): void {
        this.uri.set(this.tempCroppedUri);
        this.onChange?.(this.uri());
        this.closeCropDialog();
    }

    public closeCropDialog(): void {
        this.fileUploads()?.forEach(f => f.clear());
        this.displayCropDialog.set(false);
    }

    public croppedImageChanged(event: ImageCroppedEvent): void {
        this.tempCroppedUri = event.base64 ?? undefined;
    }

    public deleteImage() {
        this.uri.set(undefined);
        this.onChange?.(this.uri());
    }

    public imageChanged(event: { files: File[] }): void {
        this.tempImageFile.set(event.files[0]);
        this.showCropDialog();
    }

    public registerOnChange(fn?: Function): void {
        this.onChange = fn;
    }

    public registerOnTouched(): void {
        // not needed for this component, but needed to implement the interface
    }

    public setDisabledState?(isDisabled: boolean): void {
        this.disabled.set(isDisabled);
    }

    public showCropDialog(): void {
        this.displayCropDialog.set(true);
    }

    public writeValue(obj?: string): void {
        this.uri.set(obj);
    }
}
