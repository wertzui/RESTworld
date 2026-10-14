import { CropperPosition, OutputFormat } from 'ngx-image-cropper';

/**
 * If working with a RESTworld backend, you can expect a form-template-property that has an image type to also have a field `restWorldImage` with a value that satisfies this interface.
 */
export interface RestWorldImage {
    accept?: string;
    alignImage: 'left' | 'center';
    aspectRatio: number;
    backgroundColor?: string;
    canvasRotation: number;
    containWithinAspectRatio: boolean;
    cropper?: CropperPosition;
    cropperMaxHeight: number;
    cropperMaxWidth: number;
    cropperMinHeight: number;
    cropperMinWidth: number;
    cropperStaticHeight: number;
    cropperStaticWidth: number;
    format: OutputFormat;
    imageQuality: number;
    initialStepSize: number;
    maintainAspectRatio: boolean;
    onlyScaleDown: boolean;
    resizeToHeight: number;
    resizeToWidth: number;
    roundCropper: boolean;
}
