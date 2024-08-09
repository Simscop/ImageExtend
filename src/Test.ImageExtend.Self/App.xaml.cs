using ImageExtend.ViewModels;
using System.Windows;

namespace ImageExtend
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App : Application
    {

    }

    public static class GlobalValue
    {
        public static MainViewModel ViewModel = new MainViewModel();
    }

}
