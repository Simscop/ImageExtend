using Test.ImageExtend.Self;
using Test.ImageExtend.ViewModels;

namespace Test.ImageExtend
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
    }
}
