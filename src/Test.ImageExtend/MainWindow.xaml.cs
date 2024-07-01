using System.Drawing;
using System;
using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using Test.ImageExtend.ViewModels;
using System.Drawing.Imaging;
using System.Windows.Media.Imaging;

namespace Test.ImageExtend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Lift.UI.Controls.Window
    {
        public  MainViewModel MainViewModel = new();

        public MainWindow()
        {
            InitializeComponent();

            DataContext = MainViewModel;

            var img = Cv2.ImRead(@"..\..\..\Image\1.bmp");
            //var img = Cv2.ImRead(@"C:\Users\Administrator\Pictures\Saved Pictures\zjx.jpg");
            var source = img.ToWriteableBitmap();
            ImageEx.ImageSource = source;

            ImageViewer.ImageSource = BitmapFrame.Create(img.ToBitmapSource());
        }
    }
}
