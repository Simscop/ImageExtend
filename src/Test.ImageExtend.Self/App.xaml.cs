using System.Windows;
using Test.ImageExtend.ViewModels;

namespace Test.ImageExtend
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
