using System;

namespace Lift.UI.Controls;

public interface ISingleOpen : IDisposable
{
    bool CanDispose { get; }
}
