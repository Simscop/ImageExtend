using System.Windows;
using Test.ImageExtend.ViewModels;

namespace Test.ImageExtend.Self
{
    /// <summary>
    /// ImageDisposeView.xaml 的交互逻辑
    /// </summary>
    public partial class ImageDisposeView : Window
    {
        public ImageDisposeView()
        {
            InitializeComponent();
            this.DataContext = GlobalValue.ViewModel;
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {
            GlobalValue.ViewModel.DisplayModel = new();
        }

        private void BrightnessUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (BrightnessSlider.Value + BrightnessSlider.TickFrequency <= BrightnessSlider.Maximum)
            {
                BrightnessSlider.Value += BrightnessSlider.TickFrequency;
            }
        }

        private void BrightnessDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (BrightnessSlider.Value - BrightnessSlider.TickFrequency >= BrightnessSlider.Minimum)
            {
                BrightnessSlider.Value -= BrightnessSlider.TickFrequency;
            }
        }

        private void GammaSliderUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (GammaSlider.Value + GammaSlider.TickFrequency <= GammaSlider.Maximum)
            {
                GammaSlider.Value += GammaSlider.TickFrequency;
            }
        }

        private void GammaSliderDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (GammaSlider.Value - GammaSlider.TickFrequency >= GammaSlider.Minimum)
            {
                GammaSlider.Value -= GammaSlider.TickFrequency;
            }
        }

        private void ContrastSliderUpButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContrastSlider.Value + ContrastSlider.TickFrequency <= ContrastSlider.Maximum)
            {
                ContrastSlider.Value += ContrastSlider.TickFrequency;
            }
        }

        private void ContrastSliderDownButton_Click(object sender, RoutedEventArgs e)
        {
            if (ContrastSlider.Value - ContrastSlider.TickFrequency >= ContrastSlider.Minimum)
            {
                ContrastSlider.Value -= ContrastSlider.TickFrequency;
            }
        }

    }
}
