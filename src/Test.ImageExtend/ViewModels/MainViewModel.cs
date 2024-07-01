using System.Diagnostics;
using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;

namespace Test.ImageExtend.ViewModels
{
    public partial class MainViewModel: ObservableObject
    {
        [RelayCommand]
        void AcquireRamanData()
        {
            Debug.WriteLine(" AcquireRamanData Command...");
        }
    }
}
