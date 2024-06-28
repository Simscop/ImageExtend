using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;

namespace Lift.UI.V2.Controls.PropertyGrid;

public class PropertyItemsControl : ListBox
{
    /// <summary>
    /// <inheritdoc />
    /// <para>
    /// 这个方法的作用是告诉 ItemsControl 是否应该使用指定的元素作为项目的容器，而不是默认的容器。
    /// </para>
    /// <para>
    /// 这里将它直接映到了 <see cref="PropertyItem"/> 类型。
    /// </para>
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
    protected override bool IsItemItsOwnContainerOverride(object item)
        => item is PropertyItem;

    public PropertyItemsControl()
    {
        // 用于控制在分组数据时是否启用虚拟化。

#if !NET40
        VirtualizingPanel.SetIsVirtualizingWhenGrouping(this, true);
        VirtualizingPanel.SetScrollUnit(this, ScrollUnit.Pixel);
#else
        System.Windows.Controls.ScrollViewer.SetCanContentScroll(this, false);
#endif
    }
}
