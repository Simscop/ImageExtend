using System;

namespace Lift.UI.Interactivity;

public class PreviewInvokeEventArgs : EventArgs
{
    public bool Cancelling { get; set; }
}
