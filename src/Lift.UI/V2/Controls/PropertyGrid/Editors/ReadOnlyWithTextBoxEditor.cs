using Lift.UI.Tools;
using System.Windows.Data;
using System.Windows;

namespace Lift.UI.V2.Controls.PropertyGrid;

/// <summary>
/// 默认编辑器，只读，使用TextBox作为基础容器
/// </summary>
public class ReadOnlyWithTextBoxEditor : BasePropertyEditor
{
    public override FrameworkElement CreateElement(PropertyItem propertyItem)
        => new System.Windows.Controls.TextBox()
        {
            IsReadOnly = true
        };

    public override DependencyProperty GetDependencyProperty()
        => System.Windows.Controls.TextBox.TextProperty;

    protected override IValueConverter GetConverter(PropertyItem propertyItem) =>
        ResourceHelper.GetResource<IValueConverter>("Object2StringConverter");
}
