using HAL.Common.Binary;
using HAL.Common.Converters;
using HAL.Common.Forms;
using System.Text.Json.Serialization;

namespace System.ComponentModel.DataAnnotations
{
    /// <summary>
    /// Use this to align the image in the cropper either to the left or center.
    /// </summary>
    public enum ImageAlignment
    {
        /// <summary>
        /// Align the image to the center (default).
        /// </summary>
        Center,

        /// <summary>
        /// Align the image to the left.
        /// </summary>
        Left
    }

    /// <summary>
    /// Output format (png, jpeg, webp, bmp, ico) (not all browsers support all types, png is always
    /// supported, others are optional)
    /// </summary>
    public enum OutputFormat
    {
        /// <summary>
        /// png
        /// </summary>
        Png,

        /// <summary>
        /// jpeg
        /// </summary>
        Jpeg,

        /// <summary>
        /// bmp
        /// </summary>
        Bmp,

        /// <summary>
        /// webp
        /// </summary>
        Webp,

        /// <summary>
        /// ico
        /// </summary>
        Ico
    }

    /// <summary>
    /// To be able to overwrite the cropper coordinates, you can use this input. Create a new object
    /// of type CropperPosition and assign it to this input. Make sure to create a new object each
    /// time you wish to overwrite the cropper's position and wait for the cropperReady event to
    /// have fired.
    /// </summary>
    public class CropperPosition
    {
        /// <summary>
        /// Creates a new instance of the <see cref="CropperPosition"/> class.
        /// </summary>
        /// <param name="x1">X position of first coordinate (in px)</param>
        /// <param name="y1">Y position of first coordinate (in px)</param>
        /// <param name="x2">X position of second coordinate (in px)</param>
        /// <param name="y2">Y position of second coordinate (in px)</param>
        public CropperPosition(int x1, int y1, int x2, int y2)
        {
            X1 = x1;
            Y1 = y1;
            X2 = x2;
            Y2 = y2;
        }

        /// <summary>
        /// X position of first coordinate (in px)
        /// </summary>
        public int X1 { get; }

        /// <summary>
        /// X position of second coordinate (in px)
        /// </summary>
        public int X2 { get; }

        /// <summary>
        /// Y position of first coordinate (in px)
        /// </summary>
        public int Y1 { get; }

        /// <summary>
        /// Y position of second coordinate (in px)
        /// </summary>
        public int Y2 { get; }
    }

    /// <summary>
    /// Place this attribute on any Property of type <see cref="HalFile"/> to control how the image
    /// cropper is rendered in the frontend.
    /// </summary>
    [JsonConverter(typeof(OnlyDeclaredPropertiesConverter<RestWorldImageAttribute>))]
    public class RestWorldImageAttribute : DataTypeAttribute, IPropertyExtensionData
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="RestWorldImageAttribute"/> class.
        /// </summary>
        public RestWorldImageAttribute()
            : base(DataType.ImageUrl)
        {
        }

        /// <summary>
        /// Controls which files the user can select when opening an image. This should be a valid
        /// Mime-Type or file extension. The default used is "image/*". If this is different than
        /// <see cref="Format"/> then the user can open an image which will be converted to the
        /// format specified in the <see cref="Format"/> property.
        /// </summary>
        public string? Accept { get; set; }

        /// <summary>
        /// Use this to align the image in the cropper either to the left or center.
        /// </summary>
        public ImageAlignment AlignImage { get; set; }

        /// <summary>
        /// The width / height ratio (e.g. 1 / 1 for a square, 4 / 3, 16 / 9 ...)
        /// </summary>
        public double AspectRatio { get; set; }

        /// <summary>
        /// Use this to set a backgroundColor, this is useful if you upload an image of a format
        /// with transparent colors and convert it to 'jpeg' or 'bmp'. The transparent pixels will
        /// then become the set color or the default value. Enter any string representing a CSS
        /// color (https://developer.mozilla.org/en-US/docs/Web/CSS/color_value).
        /// </summary>
        public string? BackgroundColor { get; set; }

        /// <summary>
        /// Rotate the canvas (1 = 90deg, 2 = 180deg...)
        /// </summary>
        public int CanvasRotation { get; set; }

        /// <summary>
        /// When set to true, padding will be added around the image to make it fit to the aspect ratio
        /// </summary>
        public bool ContainWithinAspectRatio { get; set; }

        /// <summary>
        /// To be able to overwrite the cropper coordinates, you can use this input. Create a new
        /// object of type CropperPosition and assign it to this input. Make sure to create a new
        /// object each time you wish to overwrite the cropper's position and wait for the
        /// cropperReady event to have fired.
        /// </summary>
        public CropperPosition? Cropper { get; private set; }

        /// <summary>
        /// This array is used to fill in the <see cref="Cropper"/> property through an Attribute
        /// declaration, because custom objects cannot be set through attributes. Position of the
        /// elements are: [ X1, Y1, X2, Y2 ].
        /// </summary>
        [JsonIgnore]
        public int[]? CropperArray
        {
            get => Cropper is null ? null : new[] { Cropper.X1, Cropper.Y1, Cropper.X2, Cropper.Y2 };
            set
            {
                if (value is null)
                {
                    Cropper = null;
                    return;
                }
                if (value.Length != 4)
                    throw new ArgumentOutOfRangeException(nameof(value), $"The value must have 4 entires. Only {value.Length} where provided.");

                Cropper = new CropperPosition(value[0], value[1], value[2], value[3]);
            }
        }

        /// <summary>
        /// The cropper cannot be made bigger than this number of pixels in height (in px)
        /// </summary>
        public int CropperMaxHeight { get; set; }

        /// <summary>
        /// The cropper cannot be made bigger than this number of pixels in width (in px)
        /// </summary>
        public int CropperMaxWidth { get; set; }

        /// <summary>
        /// The cropper cannot be made smaller than this number of pixels in height (relative to
        /// original image's size) (in px) (will be ignored if maintainAspectRatio is set)
        /// </summary>
        public int CropperMinHeight { get; set; }

        /// <summary>
        /// The cropper cannot be made smaller than this number of pixels in width (relative to
        /// original image's size) (in px)
        /// </summary>
        public int CropperMinWidth { get; set; }

        /// <summary>
        /// Set Cropper Height and disable resize (in px)
        /// </summary>
        public int CropperStaticHeight { get; set; }

        /// <summary>
        /// Set Cropper Width and disable resize (in px)
        /// </summary>
        public int CropperStaticWidth { get; set; }

        /// <summary>
        /// Output format (png, jpeg, webp, bmp, ico) (not all browsers support all types, png is
        /// always supported, others are optional)
        /// </summary>
        public OutputFormat Format { get; set; }

        /// <summary>
        /// This only applies when using jpeg or webp as output format. Entering a number between 0
        /// and 100 will determine the quality of the output image.
        /// </summary>
        public int ImageQuality { get; set; }

        /// <summary>
        /// The initial step size in pixels when moving the cropper using the keyboard. Step size
        /// can then be changed by using the numpad when the cropper is focused
        /// </summary>
        public int InitialStepSize { get; set; }

        /// <summary>
        /// Keep width and height of cropped image equal according to the aspectRatio
        /// </summary>
        public bool MaintainAspectRatio { get; set; }

        /// <summary>
        /// When the resizeToWidth or resizeToHeight is set, enabling this option will make sure
        /// smaller images are not scaled up
        /// </summary>
        public bool OnlyScaleDown { get; set; }

        /// <summary>
        /// Cropped image will be resized to at most this height (in px)
        /// </summary>
        public int ResizeToHeight { get; set; }

        /// <summary>
        /// Cropped image will be resized to at most this width (in px)
        /// </summary>
        public int ResizeToWidth { get; set; }

        /// <summary>
        /// Set this to true for a round cropper. Resulting image will still be square, use
        /// border-radius: 100% on resulting image to show it as round.
        /// </summary>
        public bool RoundCropper { get; set; }
    }
}