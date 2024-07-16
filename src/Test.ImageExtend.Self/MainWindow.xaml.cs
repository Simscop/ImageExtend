using Test.ImageExtend.ViewModels;

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
        }
    }
}
