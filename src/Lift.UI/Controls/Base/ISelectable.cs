using System.Windows;

namespace Lift.UI.Controls;

public interface ISelectable
{
    event RoutedEventHandler Selected;

    bool IsSelected { get; set; }
}
