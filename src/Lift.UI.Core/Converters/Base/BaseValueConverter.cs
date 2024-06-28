using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Markup;

namespace Lift.Core.Converters;

public abstract class BaseValueConverter<T> : MarkupExtension, IValueConverter
    where T : class, new()
{
    /// <summary>
    /// 将输入的值转换成控件需要的值
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    public abstract object Convert(object? value, Type targetType, object? parameter, CultureInfo culture);

    /// <summary>
    /// 反过来转换，一般不会用，但是RadioButton的时候经常会用到
    /// </summary>
    /// <param name="value"></param>
    /// <param name="targetType"></param>
    /// <param name="parameter"></param>
    /// <param name="culture"></param>
    /// <returns></returns>
    /// <exception cref="NotImplementedException"></exception>
    public virtual object ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        => throw new NotImplementedException();

    /// <summary>
    /// 
    /// </summary>
    private static T? _instance = null;

    /// <summary>
    /// 
    /// </summary>
    /// <param name="serviceProvider"></param>
    /// <returns></returns>
    public override object ProvideValue(IServiceProvider serviceProvider)
        => _instance ??= new T();
}
