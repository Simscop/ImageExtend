using CommunityToolkit.Mvvm.ComponentModel;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Diagnostics;
using System.Windows.Media.Imaging;

namespace Test.ImageExtend.Self
{
    public partial class DisplayModel:ObservableObject
    {
        [ObservableProperty]
        private Mat _original = new();

        [ObservableProperty]
        private Mat _u8 = new();

        [ObservableProperty]
        private Mat _display = new();

        [ObservableProperty]
        private BitmapFrame? _frame;

        [ObservableProperty]
        private double _contrast = 1;

        [ObservableProperty]
        private double _brightness = 0;

        [ObservableProperty]
        private double _gamma = 1;

        [ObservableProperty]
        private int _colorMode = 0;

        partial void OnOriginalChanged(Mat value) 
        {
            U8 = Original.To8UC1(1);
        }

        partial void OnU8Changed(Mat value) 
            => Display = U8.Gamma(Gamma).Adjust(Contrast, (int)Brightness);

        partial void OnContrastChanged(double value)
            => TempDisplayDo();

        partial void OnBrightnessChanged(double value)
            => TempDisplayDo();

        partial void OnGammaChanged(double value)
            => TempDisplayDo();

        private void TempDisplayDo()
        {
            if (ValidMat(Display))
                Display = U8.Gamma(Gamma).Adjust(Contrast, (int)Brightness);
        }

        partial void OnDisplayChanged(Mat value)
            => TempFrameDo();

        partial void OnColorModeChanged(int value)
            => TempFrameDo();

        private void TempFrameDo()
        {
            if (ValidMat(Display))
                Frame = BitmapFrame.Create(Display.ApplyColor(ColorMode).ToBitmapSource());
        }

        private bool ValidMat(Mat mat) => mat.Cols != 0 || mat.Rows != 0;
    }

    public static class DisplayMatExtension
    {
        public static Mat To8UC1(this Mat mat, int mode = 0)
        {
            var u8 = new Mat();

            if (mat == null) return mat;

            if (mat.Type() == MatType.CV_8UC3)
                mat = mat.CvtColor(ColorConversionCodes.BGR2GRAY);

            mat.MinMaxLoc(out double afeawef, out var sfafe);

            if (mat.Type() == MatType.CV_16UC1)
            {
                mat.ConvertTo(u8, MatType.CV_8UC1, 1.0 / 257);

                return u8;
            }

            if (mat.Channels() != 1) return mat;
            if (mat.Depth() == 0) return mat;

            var depth = mat.Depth();

            switch (mode)
            {
                case 0:
                    mat.ConvertTo(u8, MatType.CV_8UC1, 1, 0);
                    return u8;
                case 1:
                    var mat64 = mat.To64F();
                    mat64!.MinMaxLoc(out var min, out double max);
                    (((mat64 - min) / (max - min)) * 255)
                        .ToMat()
                        .ConvertTo(u8, MatType.CV_8UC1);
                    return u8;
                case 2:
                    break;
                default:
                    break;
            }

            return u8;
        }

        public static Mat? To64F(this Mat mat)
        {
            if (mat.Channels() != 1) return null;
            var temp = new Mat(mat.Rows, mat.Cols, MatType.CV_64FC1);
            mat.ConvertTo(temp, MatType.CV_64FC1, 1, 0);
            return temp;
        }

        private static Mat Apply(this Mat mat, Mat color)
        {
            var res = new Mat();
            Cv2.ApplyColorMap(mat, res, color);
            return res;
        }

        private static Mat Apply(this Mat mat, ColormapTypes type)
        {
            var dst = new Mat();
            Cv2.ApplyColorMap(mat, dst, type);
            return dst;
        }

        private static class ColorMaps
        {
            private static Mat? _gray;

            private static Mat? _green;

            private static Mat? _red;

            private static Mat? _blue;

            private static Mat? _purple;

            public static Mat Gray
            {
                get
                {
                    if (_gray != null)
                    {
                        return _gray;
                    }

                    _gray = new Mat(256, 1, MatType.CV_8UC3);
                    for (int i = 0; i < 256; i++)
                    {
                        _gray.Set(i, 0, new Vec3b((byte)i, (byte)i, (byte)i));
                    }

                    return _gray;
                }
            }

            public static Mat Green
            {
                get
                {
                    if (_green != null)
                    {
                        return _green;
                    }

                    _green = new Mat(256, 1, MatType.CV_8UC3);
                    for (int i = 0; i < 256; i++)
                    {
                        _green.Set(i, 0, new Vec3b(0, (byte)i, 0));
                    }

                    return _green;
                }
            }

            public static Mat Red
            {
                get
                {
                    if (_red != null)
                    {
                        return _red;
                    }

                    _red = new Mat(256, 1, MatType.CV_8UC3);
                    for (int i = 0; i < 256; i++)
                    {
                        _red.Set(i, 0, new Vec3b(0, 0, (byte)i));
                    }

                    return _red;
                }
            }

            public static Mat Blue
            {
                get
                {
                    if (_blue != null)
                    {
                        return _blue;
                    }

                    _blue = new Mat(256, 1, MatType.CV_8UC3);
                    for (int i = 0; i < 256; i++)
                    {
                        _blue.Set(i, 0, new Vec3b((byte)i, 0, 0));
                    }

                    return _blue;
                }
            }

            public static Mat Pruple
            {
                get
                {
                    if (_purple != null)
                    {
                        return _purple;
                    }

                    _purple = new Mat(256, 1, MatType.CV_8UC3);
                    for (int i = 0; i < 256; i++)
                    {
                        _purple.Set(i, 0, new Vec3b((byte)i, 0, (byte)i));
                    }

                    return _purple;
                }
            }
        }

        public static Mat ApplyColor(this Mat mat, int mode)
        => mode switch
        {
            0 => mat,
            1 => mat.Apply(ColorMaps.Green),
            2 => mat.Apply(ColorMaps.Red),
            3 => mat.Apply(ColorMaps.Blue),
            4 => mat.Apply(ColorMaps.Pruple),
            5 => mat.Apply(ColormapTypes.Autumn),
            6 => mat.Apply(ColormapTypes.Bone),
            7 => mat.Apply(ColormapTypes.Jet),
            8 => mat.Apply(ColormapTypes.Winter),
            9 => mat.Apply(ColormapTypes.Rainbow),
            10 => mat.Apply(ColormapTypes.Ocean),
            11 => mat.Apply(ColormapTypes.Summer),
            12 => mat.Apply(ColormapTypes.Spring),
            13 => mat.Apply(ColormapTypes.Cool),
            14 => mat.Apply(ColormapTypes.Hsv),
            15 => mat.Apply(ColormapTypes.Pink),
            16 => mat.Apply(ColormapTypes.Hot),
            17 => mat.Apply(ColormapTypes.Parula),
            18 => mat.Apply(ColormapTypes.Magma),
            19 => mat.Apply(ColormapTypes.Inferno),
            20 => mat.Apply(ColormapTypes.Plasma),
            21 => mat.Apply(ColormapTypes.Viridis),
            22 => mat.Apply(ColormapTypes.Cividis),
            23 => mat.Apply(ColormapTypes.Twilight),
            24 => mat.Apply(ColormapTypes.TwilightShifted),
            _ => throw new Exception()
        };

        /// <summary>
        /// 设置图像的gamma值
        /// </summary>
        /// <param name="img"></param>
        /// <param name="gamma"></param>
        /// <returns></returns>
        public static Mat Gamma(this Mat img, double gamma)
        {
            if (img.Type() != MatType.CV_8UC1) return img;

            if (Math.Abs(gamma - 1) < 0.00001) return img;

            var lut = new Mat(1, 256, MatType.CV_8U);
            for (var i = 0; i < 256; i++)
                lut.Set<byte>(0, i, (byte)(Math.Pow(i / 255.0, gamma) * 255.0));

            var output = new Mat();
            Cv2.LUT(img, lut, output);

            return output;
        }

        /// <summary>
        /// 调整图像的亮度和对比度
        /// </summary>
        /// <param name="mat"></param>
        /// <param name="contrast"></param>
        /// <param name="brightness"></param>
        /// <returns></returns>
        public static Mat Adjust(this Mat mat, double contrast, int brightness)
        {
            if (Math.Abs(contrast - 1) < 0.00001 && brightness == 0) return mat;
            var result = new Mat();
            mat.ConvertTo(result, -1, contrast, brightness);
            return result;
        }

        public static List<string> Colors { get; set; } = new()
            {
                "Gray",
                "Green",
                "Red",
                "Blue",
                "Purple",
                "Autumn",
                "Bone",
                "Jet",
                "Winter",
                "Rainbow",
                "Ocean",
                "Summer",
                "Spring",
                "Cool",
                "Hsv",
                "Pink",
                "Hot",
                "Parula",
                "Magma",
                "Inferno",
                "Plasma",
                "Viridis",
                "Cividis",
                "Twilight",
            };

    }
}
