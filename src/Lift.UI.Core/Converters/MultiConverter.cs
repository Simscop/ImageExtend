using System;
using System.Globalization;
using System.Windows.Data;
using Lift.Core.Converters;

namespace Lift.UI.Core.Converters;

public class MultiConverter : BaseValueConverter<MultiConverter>
{
    public override object Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        => CanToDouble(value) && parameter is not null ? (double) value! * ToDouble(parameter): Binding.DoNothing;

    private bool CanToDouble(object? value)
        => value is double or int or float or ushort or short;

    public double ToDouble(object? value)
        => value is string v ? double.Parse(v) : (double) value!;

    public override object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => CanToDouble(value) && parameter is not null ? (double) value! / ToDouble(parameter) : Binding.DoNothing;
}
