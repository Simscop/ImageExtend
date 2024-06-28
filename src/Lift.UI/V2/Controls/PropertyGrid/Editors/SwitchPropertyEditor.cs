using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using Lift.UI.Tools;

namespace Lift.UI.V2.Controls.PropertyGrid;

public class SwitchPropertyEditor : BasePropertyEditor
{
    public override FrameworkElement CreateElement(PropertyItem propertyItem) => new ToggleButton()
    {
        Style = ResourceHelper.GetResourceInternal<Style>("ToggleButtonSwitch"),
        HorizontalAlignment = HorizontalAlignment.Left,
        IsEnabled = !propertyItem.PropertyGridAttribute.ReadOnly
    };

    public override DependencyProperty GetDependencyProperty()
        => ToggleButton.IsCheckedProperty;
}
