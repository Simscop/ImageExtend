using System.Windows;
using System.Windows.Controls;

namespace ImageExtend.ImageEx;

//实现控件布局（缩放等）

public class SimplePanel : Panel
{
    /// <summary>
    /// 测量控件的大小需求。控件会遍历所有子元素，测量它们的大小，然后返回所需的最大尺寸
    /// </summary>
    /// <param name="constraint"></param>
    /// <returns></returns>
    protected override Size MeasureOverride(Size constraint)
    {
        var maxSize = new Size();

        foreach (UIElement child in InternalChildren)
        {
            if (child == null) continue;
            child.Measure(constraint);
            maxSize.Width = Math.Max(maxSize.Width, child.DesiredSize.Width);
            maxSize.Height = Math.Max(maxSize.Height, child.DesiredSize.Height);
        }

        return maxSize;
    }

    /// <summary>
    /// 将所有子元素安排在指定大小的矩形中，并返回父控件的安排大小
    /// </summary>
    /// <param name="arrangeSize"></param>
    /// <returns></returns>
    protected override Size ArrangeOverride(Size arrangeSize)
    {
        foreach (UIElement child in InternalChildren)
        {
            child?.Arrange(new Rect(arrangeSize));
        }

        return arrangeSize;
    }
}
