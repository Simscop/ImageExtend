using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace Lift.UI.V2.Controls.PropertyGrid.Editors;

/// <summary>
/// 
/// </summary>
public class TextBoxEditor : BasePropertyEditor
{
    /// <summary>
    /// <inheritdoc cref="BasePropertyEditor.CreateElement"/>
    /// </summary>
    /// <param name="propertyItem"></param>
    /// <returns></returns>
    public override FrameworkElement CreateElement(PropertyItem propertyItem) =>
        new System.Windows.Controls.TextBox()
        {
            IsReadOnly = false
        };

    /// <summary>
    /// <inheritdoc cref="BasePropertyEditor.GetDependencyProperty"/>
    /// </summary>
    /// <returns></returns>
    public override DependencyProperty GetDependencyProperty()
        => System.Windows.Controls.TextBox.TextProperty;


}
