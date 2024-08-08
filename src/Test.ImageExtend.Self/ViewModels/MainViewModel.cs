using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using System.Diagnostics;
using System.Windows;
using Test.ImageExtend.Self;

namespace Test.ImageExtend.ViewModels
{
    public partial class MainViewModel: ObservableObject
    {
        [RelayCommand]
        void AcquireRamanData()
        {
            Debug.WriteLine(" AcquireRamanData Command...");
        }

        [ObservableProperty]
        DisplayModel _displayModel = new();

        public MainViewModel()
        {
            int type = 1;

            if (type == 0)
            {
                string file = @"C:\\Users\\Administrator\\Desktop\\拉曼-软件资料\\Image\\3_16bit.tif";
                Mat mat = Cv2.ImRead(file);
                DisplayModel.Original = mat;
            }
            else if (type == 1) 
            {
                //BitmapSource-continuous
                var paths = new List<string>()
            {
                //@"C:\\Users\\Administrator\\Desktop\\拉曼-软件资料\\Image\\1_32bit.bmp",
                //@"C:\\Users\\Administrator\\Desktop\拉曼-软件资料\\Image\\2_24bit.jpg",
                @"C:\\Users\\Administrator\\Desktop\\拉曼-软件资料\Image\\3_16bit.tif",
                //@"C:\\Users\\Administrator\\Desktop\拉曼-软件资料\\Image\\4_16bit_S.TIF",
            };
                var _imgs = paths.Select(path => Cv2.ImRead(path)).ToArray();
                int count = 0;
                int total = paths.Count;
                Task.Run(() =>
                {
                    while (true)
                    {
                        Thread.Sleep(1000);

                        if (Application.Current == null) return;

                        Application.Current.Dispatcher.Invoke(() =>
                        {
                            DisplayModel.Original = _imgs[(count++) % total];
                            //FrameContinuous = BitmapFrame.Create(_imgs[(count++) % total]?.ToBitmapSource());

                        });
                        GC.Collect();
                    }
                });
            }
        }
    }
}
