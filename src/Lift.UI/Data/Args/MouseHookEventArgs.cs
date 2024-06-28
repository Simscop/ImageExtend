using System;
using Lift.UI.Tools.Interop;

namespace Lift.UI.Data;

public class MouseHookEventArgs : EventArgs
{
    public MouseHookMessageType MessageType { get; set; }

    public InteropValues.POINT Point { get; set; }
}
