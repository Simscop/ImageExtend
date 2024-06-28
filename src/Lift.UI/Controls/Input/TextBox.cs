using System.Windows.Input;
using Lift.UI.Interactivity;

namespace Lift.UI.Controls;

public class TextBox : System.Windows.Controls.TextBox
{
    public TextBox()
    {
        CommandBindings.Add(new CommandBinding(ControlCommands.Clear, (s, e) =>
        {
            SetCurrentValue(TextProperty, string.Empty);
        }));
    }
}
