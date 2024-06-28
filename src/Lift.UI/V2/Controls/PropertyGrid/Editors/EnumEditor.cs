using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls.Primitives;
using System.Windows.Media;
using Lift.UI.Controls;

namespace Lift.UI.V2.Controls.PropertyGrid.Editors;

public class EnumEditor : BasePropertyEditor
{
    public override FrameworkElement CreateElement(PropertyItem propertyItem) => new System.Windows.Controls.ComboBox()
    {
        ItemsSource = Enum.GetValues(propertyItem.BindingType),
    };

    public override DependencyProperty GetDependencyProperty()
        => Selector.SelectedItemProperty;
}
