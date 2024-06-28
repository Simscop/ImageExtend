using Lift.UI.Tools;
using System.Windows.Data;
using System.Windows;

namespace Lift.UI.V2.Controls.PropertyGrid;


/// <summary>
/// 默认编辑器，使用TextBlock作为基础容器
/// </summary>
public class ReadOnlyWithTextBlockEditor : BasePropertyEditor
{
    public override FrameworkElement CreateElement(PropertyItem propertyItem) =>
        new System.Windows.Controls.TextBlock()
        {
            HorizontalAlignment = HorizontalAlignment.Left,
            VerticalAlignment = VerticalAlignment.Center,
        };

    public override DependencyProperty GetDependencyProperty()
        => System.Windows.Controls.TextBlock.TextProperty;

    protected override IValueConverter GetConverter(PropertyItem propertyItem) =>
        ResourceHelper.GetResource<IValueConverter>("Object2StringConverter");
}
