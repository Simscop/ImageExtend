using System;
using System.Windows.Media;
using Lift.UI.Data;

namespace Lift.UI.Controls;

public class Screenshot
{
    public static event EventHandler<FunctionEventArgs<ImageSource>> Snapped;

    public void Start() => new ScreenshotWindow(this).Show();

    internal void OnSnapped(ImageSource source) => Snapped?.Invoke(this, new FunctionEventArgs<ImageSource>(source));
}
