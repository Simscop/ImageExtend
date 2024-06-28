// https://github.com/wf-soft/WFsoft.AsyncCommand

using System.Threading.Tasks;
using System.Windows.Input;

namespace Lift.UI.Tools.Command;

public interface IAsyncCommand : ICommand
{
    Task ExecuteAsync(object parameter);
}
