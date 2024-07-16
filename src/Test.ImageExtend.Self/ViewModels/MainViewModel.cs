using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace Test.ImageExtend.ViewModels
{
    public partial class MainViewModel: ObservableObject
    {
        [RelayCommand]
        void AcquireRamanData()
        {
            Debug.WriteLine(" AcquireRamanData Command...");
        }

        [ObservableProperty]
        private BitmapFrame? _frame;

        [ObservableProperty]
        private BitmapFrame? _frameContinuous;

        [ObservableProperty]
        private DrawingImage _drawingImage;

        [ObservableProperty]
        private WriteableBitmap _writeableBitmap;

        [ObservableProperty]
        private RenderTargetBitmap _renderTargetBitmap;

        public MainViewModel()
        {
            string file = @"C:\\Users\\Administrator\\Desktop\\拉曼-软件资料\\Image\\3_16bit.tif";
            Mat mat = Cv2.ImRead(file);
            Frame = BitmapFrame.Create(mat?.ToBitmapSource());

            ////BitmapSource-continuous
            //var paths = new List<string>()
            //{
            //    @"C:\\Users\\Administrator\\Desktop\\拉曼-软件资料\\Image\\1_32bit.bmp",
            //    @"C:\\Users\\Administrator\\Desktop\拉曼-软件资料\\Image\\2_24bit.jpg",
            //    //@"C:\\Users\\Administrator\\Desktop\\拉曼-软件资料\Image\\3_16bit.tif",
            //    //@"C:\\Users\\Administrator\\Desktop\拉曼-软件资料\\Image\\4_16bit_S.TIF",
            //};
            //var _imgs = paths.Select(path => Cv2.ImRead(path)).ToArray();
            //int count = 0;
            //int total = paths.Count;
            //Task.Run(() =>
            //{
            //    while (true)
            //    {
            //        Thread.Sleep(1000);

            //        if (Application.Current == null) return;

            //        Application.Current.Dispatcher.Invoke(() =>
            //        {
            //            FrameContinuous = BitmapFrame.Create(_imgs[(count++) % total]?.ToBitmapSource());

            //        });
            //        GC.Collect();
            //    }
            //});

            //DrawingImage
            DrawingGroup drawingGroup = new DrawingGroup();
            GeometryDrawing geometryDrawing = new GeometryDrawing(
                Brushes.Blue,
                new Pen(Brushes.Black, 1),
                new RectangleGeometry(new System.Windows.Rect(0, 0, 100, 100))
            );
            drawingGroup.Children.Add(geometryDrawing);
            DrawingImage=new DrawingImage(drawingGroup);

            //WriteableBitmap
            WriteableBitmap = new WriteableBitmap(200, 200, 96, 96, PixelFormats.Pbgra32, null);
            int stride = WriteableBitmap.BackBufferStride;
            int bytesPerPixel = (WriteableBitmap.Format.BitsPerPixel + 7) / 8;
            int arraySize = stride * WriteableBitmap.PixelHeight;
            byte[] pixelData = new byte[arraySize];
            for (int y = 0; y < WriteableBitmap.PixelHeight; y++)
            {
                for (int x = 0; x < WriteableBitmap.PixelWidth; x++)
                {
                    int index = y * stride + x * bytesPerPixel;
                    pixelData[index + 0] = 0xFF; // Blue
                    pixelData[index + 1] = 0x00; // Green
                    pixelData[index + 2] = 0x00; // Red
                    pixelData[index + 3] = 0xFF; // Alpha
                }
            }
            WriteableBitmap.WritePixels(new Int32Rect(0, 0, WriteableBitmap.PixelWidth, WriteableBitmap.PixelHeight), pixelData, stride, 0);

            //RenderTargetBitmap
            RenderTargetBitmap = new RenderTargetBitmap(200, 200, 96, 96, PixelFormats.Pbgra32);
            DrawingVisual drawingVisual = new DrawingVisual();
            using (DrawingContext drawingContext = drawingVisual.RenderOpen())
            {
                drawingContext.DrawRectangle(Brushes.Yellow, null, new System.Windows.Rect(0, 0, 200, 200));
            }
            RenderTargetBitmap.Render(drawingVisual);


        }
    }
}
