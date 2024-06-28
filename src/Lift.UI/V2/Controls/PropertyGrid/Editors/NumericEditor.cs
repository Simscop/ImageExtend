using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using Lift.UI.Controls;

namespace Lift.UI.V2.Controls.PropertyGrid.Editors;

public class NumericEditor : BasePropertyEditor
{
    /// <inheritdoc />
    public override FrameworkElement CreateElement(PropertyItem propertyItem)
    {
        var info =
            propertyItem.Value.GetMembers()
                .Where(member => member.Name == propertyItem.PropertyName)
                .ToList();

        var min = double.MinValue;
        var max = double.MaxValue;

        if (info.Count == 1
            && info[0].GetCustomAttribute(typeof(RangeAttribute)) is RangeAttribute range)
        {
            min = Converter(range.Minimum);
            max = Converter(range.Maximum);
        }

        var editor = new NumericUpDown()
        {
            Minimum = min,
            Maximum = max
        };
        return editor;
    }

    public override DependencyProperty GetDependencyProperty()
        => NumericUpDown.ValueProperty;

    double Converter(object obj) => obj switch
    {
        int i => i,
        double d => d,
        _ => throw new NotImplementedException()
    };
}
