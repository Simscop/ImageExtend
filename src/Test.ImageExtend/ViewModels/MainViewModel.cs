using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
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
