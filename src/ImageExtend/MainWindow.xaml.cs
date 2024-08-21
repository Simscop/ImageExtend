using System.Diagnostics;
using System.Windows;

namespace ImageExtend
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : System.Windows.Window
    {
       public MainWindow()
        {
            InitializeComponent();
            this.DataContext = GlobalValue.ViewModel;
        }

        private void ImageExImage_MouseDoubleClick(object sender, System.Windows.Input.MouseButtonEventArgs e)
        {
            if (sender is not ImageEx viewer) return;

            var pos = viewer.ImageCurrentPosition;
            Debug.WriteLine($"ImageCurrentPosition {pos.x}-{pos.y}");

            var gridXY = viewer.GridCurrentPos;
            Debug.WriteLine($"GridCurrentPos  {gridXY.x}-{gridXY.y}");
        }
    }
}
