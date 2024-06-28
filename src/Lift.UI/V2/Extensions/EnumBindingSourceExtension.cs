using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Globalization;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Markup;

namespace Lift.UI.V2.Extensions;

/// <summary>
/// <code>
/// ItemsSource="{Binding Source={liftui:EnumBindingSource {x:Type local:Status}}}"
/// </code>
/// </summary>
public class EnumBindingSourceExtension : MarkupExtension
{
    private Type? _enumType;

    public Type? EnumType
    {
        get { return _enumType; }
        set
        {
            if (value == _enumType) return;

            if (null != value)
            {
                var enumType = Nullable.GetUnderlyingType(value) ?? value;
                if (!enumType.IsEnum)
                    throw new ArgumentException("Type must bu for an Enum");
            }

            _enumType = value;
        }
    }

    public EnumBindingSourceExtension() { }

    public EnumBindingSourceExtension(Type enumType) => EnumType = enumType;

    public override object ProvideValue(IServiceProvider serviceProvider)
    {
        if (_enumType == null)
            throw new InvalidOperationException("The EnumTYpe must be specified.");

        var actualEnumType = Nullable.GetUnderlyingType(_enumType) ?? _enumType;
        var enumValues = Enum.GetValues(actualEnumType);

        if (actualEnumType == _enumType) return enumValues;

        var tempArray = Array.CreateInstance(actualEnumType, enumValues.Length + 1);
        enumValues.CopyTo(tempArray, 1);

        return tempArray;

    }
}

