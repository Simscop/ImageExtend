using System.Windows;
using System.Windows.Media;

namespace Test.ImageExtend.Self
{
    public partial class ImageAdjust : Window
    {
        public double Brigheness { get; set; } = -1;
        public double Contrast { get; set; } = -1;
        public double Gamma { get; set; } = -1;

        public event EventHandler<double>? BrightnessUpdated;
        public event EventHandler<double>? ContrastUpdated;
        public event EventHandler<double>? GammaUpdated;

        public ImageAdjust(double _brigheness, double _contrast,double  _gamma)
        {
            Brigheness= _brigheness;
            Contrast= _contrast;
            Gamma= _gamma;
            InitializeComponent();
        }

        private void Close_Click(object sender, RoutedEventArgs e)
        {
            DialogResult = true;
        }

        private void Reset_Click(object sender, RoutedEventArgs e)
        {       
            BrightnessSlider.Value = 1;
            GammaSlider.Value = 1;
            ContrastSlider.Value = 0;
        }

        private void BrightnessSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Brigheness = BrightnessSlider.Value;
            BrightnessUpdated?.Invoke(this, Brigheness);
        }

        private void GammaSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Gamma = GammaSlider.Value;
            GammaUpdated?.Invoke(this, Gamma);
        }

        private void ContrastSlider_ValueChanged(object sender, RoutedPropertyChangedEventArgs<double> e)
        {
            Contrast = ContrastSlider.Value;
            ContrastUpdated?.Invoke(this, Contrast);
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
