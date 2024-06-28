using System.Windows;
using System.Windows.Controls;

namespace WpfApp1
{
    public class MyCustomPanel:Panel
    {
        protected override Size MeasureOverride(Size availableSize)
        {
            Size totalSize = new Size();

            foreach (UIElement child in InternalChildren)
            {
                if (child == null) continue;

                // Measure each child element
                child.Measure(availableSize);

                // Update the total size needed by this panel
                totalSize.Width = Math.Max(totalSize.Width, child.DesiredSize.Width);
                totalSize.Height += child.DesiredSize.Height;
            }

            return totalSize;
        }

        protected override Size ArrangeOverride(Size finalSize)
        {
            double yOffset = 0;

            foreach (UIElement child in InternalChildren)
            {
                if (child == null) continue;

                // Arrange each child element
                child.Arrange(new Rect(0, yOffset, finalSize.Width, child.DesiredSize.Height));

                // Update the offset for the next child
                yOffset += child.DesiredSize.Height;
            }

            return finalSize;
        }
    }
}
