using System.Windows;
using System.Windows.Media.Imaging;

namespace WpfApp1
{
    public static class ImageProcessingHelper
    {
        public static BitmapSource AdjustGamma(BitmapSource source, double gamma)
        {
            if (source == null) throw new ArgumentNullException(nameof(source));
            if (gamma <= 0) throw new ArgumentOutOfRangeException(nameof(gamma), "Gamma must be greater than zero.");

            int width = source.PixelWidth;
            int height = source.PixelHeight;
            int stride = width * ((source.Format.BitsPerPixel + 7) / 8);
            byte[] pixels = new byte[height * stride];

            source.CopyPixels(pixels, stride, 0);

            byte[] gammaLut = new byte[256];
            for (int i = 0; i < 256; i++)
            {
                gammaLut[i] = (byte)Math.Min(255, (int)((255.0 * Math.Pow(i / 255.0, 1.0 / gamma)) + 0.5));
            }

            for (int i = 0; i < pixels.Length; i++)
            {
                pixels[i] = gammaLut[pixels[i]];
            }

            WriteableBitmap result = new WriteableBitmap(width, height, source.DpiX, source.DpiY, source.Format, null);
            result.WritePixels(new Int32Rect(0, 0, width, height), pixels, stride, 0);

            return result;
        }

        // 其他图像处理方法可以在此添加，如调整亮度、对比度等
    }
}
