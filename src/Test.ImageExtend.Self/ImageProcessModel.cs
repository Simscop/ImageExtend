using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Test.ImageExtend.Self
{
    public static class ImageProcessModel
    {
        public static ImageSource Gamma(ImageSource? source, double gamma)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (gamma <= 0) throw new ArgumentOutOfRangeException(nameof(gamma), "Gamma must be greater than zero.");
            BitmapSource? bitmapSource = ConvertToBitmapSource(source);
            BitmapSource? gammaBitmap = ApplyGamma(bitmapSource, gamma);
            bitmapSource=null;
            source = null;
            return gammaBitmap;
        }

        public static ImageSource Brightness(ImageSource? source, double brightness)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (brightness < 0 || brightness > 2) throw new ArgumentOutOfRangeException(nameof(brightness), "Brightness must be between 0 and 2.");
            BitmapSource? bitmapSource = ConvertToBitmapSource(source);
            BitmapSource brightnessBitmap = ApplyBrightness(bitmapSource, brightness);
            bitmapSource = null;
            source = null;
            return brightnessBitmap;
        }

        public static ImageSource Contrast(ImageSource? source, double contrast)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (contrast < 0 || contrast > 2) throw new ArgumentOutOfRangeException(nameof(contrast), "Contrast must be between 0 and 2.");
            BitmapSource? bitmapSource = ConvertToBitmapSource(source);
            BitmapSource contrastBitmap = ApplyContrast(bitmapSource, contrast);
            bitmapSource = null;
            source = null;
            return contrastBitmap;
        }

        private static BitmapSource ApplyGamma(BitmapSource source, double gamma)
        {
            if(gamma==1) return source;
            BitmapSource newsource;

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * (source.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            source.CopyPixels(pixelData, stride, 0);

            byte[] gammaLUT = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                gammaLUT[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / gamma)) + 0.5));
            }

            for (int i = 0; i < pixelData.Length; i++)
            {
                pixelData[i] = gammaLUT[pixelData[i]]; // 灰度
            }

            //switch (source.Format)
            //{
            //    case PixelFormat pf when pf == PixelFormats.Bgra32 || pf == PixelFormats.Pbgra32:
            //        for (int i = 0; i < pixelData.Length; i += 4)
            //        {
            //            pixelData[i] = gammaLUT[pixelData[i]];       // 蓝色
            //            pixelData[i + 1] = gammaLUT[pixelData[i + 1]]; // 绿色
            //            pixelData[i + 2] = gammaLUT[pixelData[i + 2]]; // 红色
            //                                                           // Alpha 通道不变，pixelData[i + 3]
            //        }
            //        break;
            //    case PixelFormat pf when pf == PixelFormats.Bgr32 || pf == PixelFormats.Bgr24:
            //        for (int i = 0; i < pixelData.Length; i += 4)
            //        {
            //            pixelData[i] = gammaLUT[pixelData[i]];       // 蓝色
            //            pixelData[i + 1] = gammaLUT[pixelData[i + 1]]; // 绿色
            //            pixelData[i + 2] = gammaLUT[pixelData[i + 2]]; // 红色
            //        }
            //        break;
            //    case PixelFormat pf when pf == PixelFormats.Gray8:
            //        for (int i = 0; i < pixelData.Length; i++)
            //        {
            //            pixelData[i] = gammaLUT[pixelData[i]]; // 灰度
            //        }
            //        break;
            //    default:
            //        throw new NotSupportedException($"Unsupported pixel format: {source.Format}");
            //}
            newsource= BitmapSource.Create(width, height, source.DpiX, source.DpiY, source.Format, source.Palette, pixelData, stride);
            source.Freeze();
            return newsource;
        }

        private static BitmapSource ConvertToBitmapSource(ImageSource source)
        {
            // 处理不同类型的 ImageSource
            switch (source)
            {
                case BitmapFrame bitmapFrame:
                    return bitmapFrame;
                case DrawingImage drawingImage:
                    DrawingVisual drawingVisual = new DrawingVisual();
                    using (DrawingContext drawingContext = drawingVisual.RenderOpen())
                    {
                        drawingContext.DrawImage(drawingImage, new System.Windows.Rect(0, 0, drawingImage.Width, drawingImage.Height));
                    }
                    RenderTargetBitmap renderBitmap = new RenderTargetBitmap((int)drawingImage.Width, (int)drawingImage.Height, 96, 96, PixelFormats.Pbgra32);
                    renderBitmap.Render(drawingVisual);
                    return renderBitmap;
                case WriteableBitmap writeableBitmap:
                    return writeableBitmap;
                case RenderTargetBitmap renderTargetBitmap:
                    return renderTargetBitmap;
                case CachedBitmap cachedBitmap:
                    return (BitmapSource)cachedBitmap;
                default:
                    throw new NotSupportedException("Unsupported ImageSource type");
            }
        }

        private static BitmapSource ApplyBrightness(BitmapSource source, double brightness)
        {
            if (brightness == 1) return source;
            BitmapSource newsource;

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * (source.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            source.CopyPixels(pixelData, stride, 0);

            for (int i = 0; i < pixelData.Length; i++)
            {
                pixelData[i] = ClampToByte(pixelData[i] * brightness); // 灰度
            }

            //switch (source.Format)
            //{
            //    case PixelFormat pf when pf == PixelFormats.Bgra32 || pf == PixelFormats.Pbgra32:
            //        for (int i = 0; i < pixelData.Length; i += 4)
            //        {
            //            pixelData[i] = ClampToByte(pixelData[i] * brightness);       // 蓝色
            //            pixelData[i + 1] = ClampToByte(pixelData[i + 1] * brightness); // 绿色
            //            pixelData[i + 2] = ClampToByte(pixelData[i + 2] * brightness); // 红色
            //                                                                           // Alpha 通道不变，pixelData[i + 3]
            //        }
            //        break;
            //    case PixelFormat pf when pf == PixelFormats.Bgr32|| pf == PixelFormats.Bgr24:
            //        for (int i = 0; i < pixelData.Length; i += 4)
            //        {
            //            pixelData[i] = ClampToByte(pixelData[i] * brightness);       // 蓝色
            //            pixelData[i + 1] = ClampToByte(pixelData[i + 1] * brightness); // 绿色
            //            pixelData[i + 2] = ClampToByte(pixelData[i + 2] * brightness); // 红色
            //        }
            //        break;
            //    case PixelFormat pf when pf == PixelFormats.Gray8:
            //        for (int i = 0; i < pixelData.Length; i++)
            //        {
            //            pixelData[i] = ClampToByte(pixelData[i] * brightness); // 灰度
            //        }
            //        break;
            //    default:
            //        throw new NotSupportedException($"Unsupported pixel format: {source.Format}");
            //}

            newsource = BitmapSource.Create(width, height, source.DpiX, source.DpiY, source.Format, source.Palette, pixelData, stride);
            source.Freeze();
            return newsource;
        }

        private static byte ClampToByte(double value)
        {
            return (byte)Math.Max(0, Math.Min(255, value));
        }

        private static BitmapSource ApplyContrast(BitmapSource source, double contrast)
        {  
            if (contrast == 1) return source;
            BitmapSource newsource;

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * (source.Format.BitsPerPixel / 8);
            byte[] pixelData = new byte[height * stride];
            source.CopyPixels(pixelData, stride, 0);

            double factor = (contrast + 1) / (1 - contrast);

            for (int i = 0; i < pixelData.Length; i++)
            {
                pixelData[i] = ClampToByte(factor * (pixelData[i] - 128) + 128); // 灰度
            }

            //switch (source.Format)
            //{
            //    case PixelFormat pf when pf == PixelFormats.Bgra32 || pf == PixelFormats.Pbgra32:
            //        for (int i = 0; i < pixelData.Length; i += 4)
            //        {
            //            pixelData[i] = ClampToByte(factor * (pixelData[i] - 128) + 128);       // 蓝色
            //            pixelData[i + 1] = ClampToByte(factor * (pixelData[i + 1] - 128) + 128); // 绿色
            //            pixelData[i + 2] = ClampToByte(factor * (pixelData[i + 2] - 128) + 128); // 红色
            //                                                                                     // Alpha 通道不变，pixelData[i + 3]
            //        }
            //        break;
            //    case PixelFormat pf when pf == PixelFormats.Bgr32|| pf == PixelFormats.Bgr24:
            //        for (int i = 0; i < pixelData.Length; i += 4)
            //        {
            //            pixelData[i] = ClampToByte(factor * (pixelData[i] - 128) + 128);       // 蓝色
            //            pixelData[i + 1] = ClampToByte(factor * (pixelData[i + 1] - 128) + 128); // 绿色
            //            pixelData[i + 2] = ClampToByte(factor * (pixelData[i + 2] - 128) + 128); // 红色
            //        }
            //        break;
            //    case PixelFormat pf when pf == PixelFormats.Gray8:
            //        for (int i = 0; i < pixelData.Length; i++)
            //        {
            //            pixelData[i] = ClampToByte(factor * (pixelData[i] - 128) + 128); // 灰度
            //        }
            //        break;
            //    default:
            //        throw new NotSupportedException($"Unsupported pixel format: {source.Format}");
            //}

            newsource = BitmapSource.Create(width, height, source.DpiX, source.DpiY, source.Format, source.Palette, pixelData, stride);
            source.Freeze();
            return newsource;
        }
    }
}
