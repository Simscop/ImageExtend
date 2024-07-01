using OpenCvSharp;
using OpenCvSharp.WpfExtensions;
using System.Windows.Media.Imaging;
using Test.ImageExtend.ViewModels;
using System.Windows;

namespace Test.ImageExtend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
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
