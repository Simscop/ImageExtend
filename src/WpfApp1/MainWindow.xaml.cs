using System.Text;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void ZoomIn_Click(object sender, RoutedEventArgs e)
        {
            myImage.LayoutTransform = new ScaleTransform(1.2, 1.2);
        }

        private void ZoomOut_Click(object sender, RoutedEventArgs e)
        {
            myImage.LayoutTransform = new ScaleTransform(0.8, 0.8);
        }

        private void Rotate_Click(object sender, RoutedEventArgs e)
        {
            myImage.LayoutTransform = new RotateTransform(90);
        }

        private void AdjustGamma_Click(object sender, RoutedEventArgs e)
        {
            if (myImage.Source is BitmapSource bitmapSource)
            {
                double gamma =3; // 这里可以弹出一个对话框让用户输入具体的gamma值
                myImage.Source = ImageProcessingHelper.AdjustGamma(bitmapSource, gamma);
            }
        }
    }
}