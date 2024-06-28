using System;
using System.Windows.Input;

namespace Lift.UI.Tools.Command;

public class SimpleRelayCommand : ICommand
{
    private Action _action;

    public SimpleRelayCommand(Action action)
    {
        _action = action;
    }

    public event EventHandler CanExecuteChanged;

    public bool CanExecute(object parameter)
    {
        return true;
    }

    public void Execute(object parameter)
    {
        _action();
    }
}
