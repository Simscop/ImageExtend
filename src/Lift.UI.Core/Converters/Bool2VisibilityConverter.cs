using System;
using System.Globalization;
using System.Windows.Data;
using Lift.Core.Converters;

namespace Lift.UI.Core.Converters;

public class Bool2VisibilityConverter : BaseValueConverter<Bool2VisibilityConverter>
{
    public override object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not bool v) return Binding.DoNothing;

        return v ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
    }

    public override object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value is not System.Windows.Visibility v) return Binding.DoNothing;

        return v == System.Windows.Visibility.Visible;
    }
}
