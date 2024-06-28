using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;

namespace Lift.UI.V2.Converters;

/// <summary>
/// <code>
/// [TypeConverter(typeof(EnumDescriptionTypeConverter))]
/// public enum Status
/// {
///     [Description("This is horrible")]
///     Horrible,
///     [Description("This is Bad")]
///     Bad,
///     [Description("This is SoSo")]
///     SoSo,
///     [Description("This is Good")]
///     Good,
///     [Description("This is Better")]
///     Better,
///     [Description("This is Best")]
///     Best
/// }
/// </code>
/// </summary>
/// <param name="type"></param>
public class EnumDescriptionTypeConverter(Type type) : EnumConverter(type)
{
    /// <summary>
    /// <inheritdoc/>
    /// </summary>
    public override object? ConvertTo(ITypeDescriptorContext? context, CultureInfo? culture, object? value, Type destinationType)
    {
        if (destinationType != typeof(string))
            return base.ConvertTo(context, culture, value, destinationType);

        if (null == value)
            return string.Empty;

        var fi = value.GetType().GetField(value.ToString() ?? "");

        if (null == fi)
            return string.Empty;

        var attributes =
            (DescriptionAttribute[]) fi.GetCustomAttributes(typeof(DescriptionAttribute), false);

        return ((attributes.Length > 0) && (!string.IsNullOrEmpty(attributes[0].Description)))
            ? attributes[0].Description
            : value.ToString();

    }
}
