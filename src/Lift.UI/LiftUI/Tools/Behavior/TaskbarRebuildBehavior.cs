using System;
using Lift.UI.Interactivity;
using System.Windows;
using System.Windows.Interop;
using Lift.UI.Tools.Interop;

namespace Lift.UI.Controls;

public sealed class TaskbarRebuildBehavior : Behavior<System.Windows.Window>
{
    private int taskbarCreated;

    public static readonly DependencyProperty ElementProperty =
        DependencyProperty.Register("Element", typeof(UIElement), typeof(TaskbarRebuildBehavior));

    public UIElement Element
    {
        get => (UIElement) this.GetValue(ElementProperty);
        set => this.SetValue(ElementProperty, value);
    }

    protected override void OnAttached()
    {
        this.AssociatedObject.Loaded += (sender, e) =>
        {
            this.taskbarCreated = InteropMethods.RegisterWindowMessage("TaskbarCreated");
            if (PresentationSource.FromVisual(this.AssociatedObject) is HwndSource hwndSource)
            {
                hwndSource.AddHook(this.WndProc);
            }
        };
    }

    private IntPtr WndProc(IntPtr hwnd, int msg, IntPtr wParam, IntPtr lParam, ref bool handled)
    {
        if (msg == this.taskbarCreated)
        {
            this.Element.Visibility = Visibility.Collapsed;
            this.Element.Visibility = Visibility.Visible;
        }

        return IntPtr.Zero;
    }
}
