using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using Lift.UI.Tools;
using Lift.UI.V2.Controls.PropertyGrid.UserControls;

namespace Lift.UI.V2.Controls.PropertyGrid.Editors;

public class PasswordBoxEditor : BasePropertyEditor
{
    public override FrameworkElement CreateElement(PropertyItem propertyItem)
        => new ExtendPasswordBox();

    public override DependencyProperty GetDependencyProperty()
        => ExtendPasswordBox.ActualPasswordProperty;
}
