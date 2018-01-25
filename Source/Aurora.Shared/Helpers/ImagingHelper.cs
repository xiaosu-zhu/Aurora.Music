using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Numerics;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Threading.Tasks;
using Windows.Foundation;
using Windows.Graphics.Imaging;
using Windows.Storage;
using Windows.Storage.Streams;
using Windows.System.UserProfile;
using Windows.UI;
using Windows.UI.ViewManagement;
using Windows.UI.Xaml.Media.Imaging;

namespace Aurora.Shared.Helpers
{
    public static class ImagingHelper
    {
        public static async Task<BitmapImage> ResizedImage(StorageFile ImageFile, float ratio)
        {
            IRandomAccessStream inputstream = await ImageFile.OpenReadAsync();
            BitmapImage sourceImage = new BitmapImage();
            sourceImage.SetSource(inputstream);

            var origHeight = sourceImage.PixelHeight;
            var origWidth = sourceImage.PixelWidth;
            var newHeight = (int)(origHeight * ratio);
            var newWidth = (int)(origWidth * ratio);

            sourceImage.DecodePixelWidth = newWidth;
            sourceImage.DecodePixelHeight = newHeight;

            return sourceImage;
        }

        public static async Task<WriteableBitmap> GetPictureAsync(StorageFile ImageFile)
        {

            using (IRandomAccessStream stream = await ImageFile.OpenAsync(FileAccessMode.Read))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(stream);
                WriteableBitmap bmp = new WriteableBitmap((int)decoder.PixelWidth, (int)decoder.PixelHeight);
                await bmp.SetSourceAsync(stream);
                return bmp;
            }
        }

        public static async Task SaveBitmapToFileAsync(WriteableBitmap image, string userId)
        {
            StorageFolder pictureFolder = await ApplicationData.Current.LocalFolder.CreateFolderAsync("ProfilePictures", CreationCollisionOption.OpenIfExists);
            var file = await pictureFolder.CreateFileAsync(userId + ".jpg", CreationCollisionOption.ReplaceExisting);

            using (var stream = await file.OpenStreamForWriteAsync())
            {
                BitmapEncoder encoder = await BitmapEncoder.CreateAsync(BitmapEncoder.PngEncoderId, stream.AsRandomAccessStream());
                var pixelStream = image.PixelBuffer.AsStream();
                byte[] pixels = new byte[image.PixelBuffer.Length];

                await pixelStream.ReadAsync(pixels, 0, pixels.Length);

                encoder.SetPixelData(BitmapPixelFormat.Bgra8, BitmapAlphaMode.Ignore, (uint)image.PixelWidth, (uint)image.PixelHeight, 96, 96, pixels);

                await encoder.FlushAsync();
            }
        }

        public static async Task CropandScaleAsync(StorageFile source, StorageFile dest, Point startPoint, Size size, double m_scaleFactor)
        {
            uint startPointX = (uint)Math.Floor(startPoint.X);
            uint startPointY = (uint)Math.Floor(startPoint.Y);
            uint height = (uint)Math.Floor(size.Height);
            uint width = (uint)Math.Floor(size.Width);
            using (IRandomAccessStream sourceStream = await source.OpenReadAsync(),
                destStream = await dest.OpenAsync(FileAccessMode.ReadWrite))
            {
                BitmapDecoder decoder = await BitmapDecoder.CreateAsync(sourceStream);
                var m_displayHeightNonScaled = decoder.OrientedPixelHeight;
                var m_displayWidthNonScaled = decoder.OrientedPixelWidth;

                // Use the native (no orientation applied) image dimensions because we want to handle
                // orientation ourselves.
                BitmapTransform transform = new BitmapTransform();
                BitmapBounds bounds = new BitmapBounds()
                {
                    X = (uint)(startPointX * m_scaleFactor),
                    Y = (uint)(startPointY * m_scaleFactor),
                    Height = (uint)(height * m_scaleFactor),
                    Width = (uint)(width * m_scaleFactor)
                };
                transform.Bounds = bounds;

                // Scaling occurs before flip/rotation, therefore use the original dimensions
                // (no orientation applied) as parameters for scaling.
                transform.ScaledHeight = (uint)(decoder.PixelHeight * m_scaleFactor);
                transform.ScaledWidth = (uint)(decoder.PixelWidth * m_scaleFactor);
                transform.Rotation = BitmapRotation.None;

                // Fant is a relatively high quality interpolation mode.
                transform.InterpolationMode = BitmapInterpolationMode.Fant;
                BitmapPixelFormat format = decoder.BitmapPixelFormat;
                BitmapAlphaMode alpha = decoder.BitmapAlphaMode;

                // Set the encoder's destination to the temporary, in-memory stream.
                PixelDataProvider pixelProvider = await decoder.GetPixelDataAsync(
                        format,
                        alpha,
                        transform,
                        ExifOrientationMode.IgnoreExifOrientation,
                        ColorManagementMode.ColorManageToSRgb
                        );

                byte[] pixels = pixelProvider.DetachPixelData();


                Guid encoderID = Guid.Empty;

                switch (dest.FileType.ToLower())
                {
                    case ".png":
                        encoderID = BitmapEncoder.PngEncoderId;
                        break;
                    case ".bmp":
                        encoderID = BitmapEncoder.BmpEncoderId;
                        break;
                    default:
                        encoderID = BitmapEncoder.JpegEncoderId;
                        break;
                }

                // Write the pixel data onto the encoder. Note that we can't simply use the
                // BitmapTransform.ScaledWidth and ScaledHeight members as the user may have
                // requested a rotation (which is applied after scaling).
                var encoder = await BitmapEncoder.CreateAsync(encoderID, destStream);

                encoder.SetPixelData(
                    format,
                    alpha,
                    (bounds.Width),
                    (bounds.Height),
                    decoder.DpiX,
                    decoder.DpiY,
                    pixels
                    );

                await encoder.FlushAsync();
            }
        }
        public static async Task<bool> SetWallpaperAsync(StorageFile localAppDataFileName)
        {
            bool success = false;
            if (UserProfilePersonalizationSettings.IsSupported())
            {
                UserProfilePersonalizationSettings profileSettings = UserProfilePersonalizationSettings.Current;
                success = await profileSettings.TrySetLockScreenImageAsync(localAppDataFileName);
            }
            return success;
        }

        private static readonly int CALCULATE_BITMAP_MIN_DIMENSION = 50;

        static Color[] pixels;
        private static ColorThiefDotNet.ColorThief colorThief = new ColorThiefDotNet.ColorThief();

        private static async Task<Color> GetPixels(WriteableBitmap bitmap, Color[] pixels, Int32 width, Int32 height)

        {

            IRandomAccessStream bitmapStream = new InMemoryRandomAccessStream();

            await bitmap.ToStreamAsJpeg(bitmapStream);

            var bitmapDecoder = await BitmapDecoder.CreateAsync(bitmapStream);

            var pixelProvider = await bitmapDecoder.GetPixelDataAsync();

            Byte[] byteArray = pixelProvider.DetachPixelData();

            Int32 r = 0, g = 0, b = 0;

            int sum = pixels.Length;

            for (var i = 0; i < height; i++)

            {

                for (var j = 0; j < width; j++)

                {



                    r += byteArray[(i * width + j) * 4 + 2];

                    g += byteArray[(i * width + j) * 4 + 1];

                    b += byteArray[(i * width + j) * 4 + 0];

                }

            }

            return Color.FromArgb((byte)(255), (byte)(r / sum), (byte)(g / sum), (byte)(b / sum));

        }

        private static async Task<Color> FromBitmap(WriteableBitmap bitmap)

        {

            int width = bitmap.PixelWidth;

            int height = bitmap.PixelHeight;

            pixels = new Color[width * height];

            return await GetPixels(bitmap, pixels, width, height);

        }

        private static WriteableBitmap ScaleBitmapDown(WriteableBitmap bitmap)

        {

            int minDimension = Math.Min(bitmap.PixelWidth, bitmap.PixelHeight);



            if (minDimension <= CALCULATE_BITMAP_MIN_DIMENSION)

            {

                // If the bitmap is small enough already, just return it

                return bitmap;

            }



            float scaleRatio = CALCULATE_BITMAP_MIN_DIMENSION / (float)minDimension;



            WriteableBitmap resizedBitmap = bitmap.Resize((Int32)(bitmap.PixelWidth * scaleRatio), (Int32)(bitmap.PixelHeight * scaleRatio), WriteableBitmapExtensions.Interpolation.Bilinear);

            return resizedBitmap;

        }

        public static async Task<Color> GetMainColor(Uri path)
        {
            if (path == null)
            {
                return new UISettings().GetColorValue(UIColorType.Accent);
            }
            //get the file
            if (path.IsFile)
            {
                var file = await StorageFile.GetFileFromPathAsync(path.LocalPath);
                using (IRandomAccessStream stream = await file.OpenReadAsync())
                {
                    //Create a decoder for the image
                    var decoder = await BitmapDecoder.CreateAsync(stream);

                    ////Create a transform to get a 1x1 image
                    //var myTransform = new BitmapTransform { ScaledHeight = 1, ScaledWidth = 1 };

                    ////Get the pixel provider
                    //var pixels = await decoder.GetPixelDataAsync(
                    //    BitmapPixelFormat.Rgba8,
                    //    BitmapAlphaMode.Ignore,
                    //    myTransform,
                    //    ExifOrientationMode.IgnoreExifOrientation,
                    //    ColorManagementMode.DoNotColorManage);

                    ////Get the bytes of the 1x1 scaled image
                    //var bytes = pixels.DetachPixelData();

                    return FromColorThief((await colorThief.GetColor(decoder, 4)).Color);

                    //read the color 
                    //return Color.FromArgb(255, bytes[0], bytes[1], bytes[2]);
                }
            }
            else
            {
                RandomAccessStreamReference random = RandomAccessStreamReference.CreateFromUri(path);
                using (IRandomAccessStream stream = await random.OpenReadAsync())
                {
                    //Create a decoder for the image
                    var decoder = await BitmapDecoder.CreateAsync(stream);

                    ////Create a transform to get a 1x1 image
                    //var myTransform = new BitmapTransform { ScaledHeight = 1, ScaledWidth = 1 };

                    ////Get the pixel provider
                    //var pixels = await decoder.GetPixelDataAsync(
                    //    BitmapPixelFormat.Rgba8,
                    //    BitmapAlphaMode.Ignore,
                    //    myTransform,
                    //    ExifOrientationMode.IgnoreExifOrientation,
                    //    ColorManagementMode.DoNotColorManage);

                    ////Get the bytes of the 1x1 scaled image
                    //var bytes = pixels.DetachPixelData();

                    return FromColorThief((await colorThief.GetColor(decoder, 4)).Color);

                    //read the color 
                    //return Color.FromArgb(255, bytes[0], bytes[1], bytes[2]);
                }
            }
        }

        public static Color FromColorThief(ColorThiefDotNet.Color d)
        {
            var a = d.A;
            var r = d.R;
            var g = d.G;
            var b = d.B;
            return Color.FromArgb(a, r, g, b);
        }

        public static async Task<Color> New(Stream stream)

        {

            WriteableBitmap map = await BitmapFactory.FromStream(stream);

            WriteableBitmap scaledbmp = ScaleBitmapDown(map);

            return await FromBitmap(scaledbmp);

        }

        public static async Task<List<Color>> GetColorPalette(Uri path)
        {
            if (path == null)
            {
                return new List<Color>() { new UISettings().GetColorValue(UIColorType.Accent) };
            }
            //get the file
            if (path.IsFile)
            {
                var file = await StorageFile.GetFileFromPathAsync(path.LocalPath);
                using (IRandomAccessStream stream = await file.OpenReadAsync())
                {
                    //Create a decoder for the image
                    var decoder = await BitmapDecoder.CreateAsync(stream);

                    ////Create a transform to get a 1x1 image
                    //var myTransform = new BitmapTransform { ScaledHeight = 1, ScaledWidth = 1 };

                    ////Get the pixel provider
                    //var pixels = await decoder.GetPixelDataAsync(
                    //    BitmapPixelFormat.Rgba8,
                    //    BitmapAlphaMode.Ignore,
                    //    myTransform,
                    //    ExifOrientationMode.IgnoreExifOrientation,
                    //    ColorManagementMode.DoNotColorManage);

                    ////Get the bytes of the 1x1 scaled image
                    //var bytes = pixels.DetachPixelData();

                    return (await colorThief.GetPalette(decoder, 4)).Select(a => FromColorThief(a.Color)).ToList();

                    //read the color 
                    //return Color.FromArgb(255, bytes[0], bytes[1], bytes[2]);
                }
            }
            else
            {
                RandomAccessStreamReference random = RandomAccessStreamReference.CreateFromUri(path);
                using (IRandomAccessStream stream = await random.OpenReadAsync())
                {
                    //Create a decoder for the image
                    var decoder = await BitmapDecoder.CreateAsync(stream);

                    ////Create a transform to get a 1x1 image
                    //var myTransform = new BitmapTransform { ScaledHeight = 1, ScaledWidth = 1 };

                    ////Get the pixel provider
                    //var pixels = await decoder.GetPixelDataAsync(
                    //    BitmapPixelFormat.Rgba8,
                    //    BitmapAlphaMode.Ignore,
                    //    myTransform,
                    //    ExifOrientationMode.IgnoreExifOrientation,
                    //    ColorManagementMode.DoNotColorManage);

                    ////Get the bytes of the 1x1 scaled image
                    //var bytes = pixels.DetachPixelData();

                    return (await colorThief.GetPalette(decoder, 4)).Select(a => FromColorThief(a.Color)).ToList();

                    //read the color 
                    //return Color.FromArgb(255, bytes[0], bytes[1], bytes[2]);
                }
            }
        }
    }
}
