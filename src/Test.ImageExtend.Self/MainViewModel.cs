using CommunityToolkit.Mvvm.ComponentModel;
using CommunityToolkit.Mvvm.Input;
using OpenCvSharp;
using System.Diagnostics;
using System.Windows;

namespace ImageExtend.ViewModels
{
    public partial class MainViewModel: ObservableObject
    {
        [RelayCommand]
        void AcquireRamanData()
        {
            Debug.WriteLine(" AcquireRamanData Command...");
        }

        [ObservableProperty]
        (int, int) _fillEndPoint = new();

        [ObservableProperty]
        (int, int) _gridPosition = new();

        partial void OnGridPositionChanged((int, int) value)
        {
            Debug.WriteLine($"OnGridPositionChanged_({value.Item1},{value.Item2})");
        }

        [RelayCommand]
        void ChangeShow()
        {
            int col = (int)Convert.ToInt64(DateTime.Now.ToString("ss")) % 4;
            var row = col + 1;
            Debug.WriteLine($"col_{col} row_{row}");
            FillEndPoint = (col, row);
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
                        if (Application.Current == null) return;

                        Application.Current?.Dispatcher?.Invoke(() =>
                        {
                            DisplayModel.Original = _imgs[(count++) % total];
                        });

                        Thread.Sleep(100);

                        GC.Collect();
                    }
                });
            }
        }
    }
}
