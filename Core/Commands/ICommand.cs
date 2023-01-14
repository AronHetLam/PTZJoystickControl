using PtzJoystickControl.Core.Model;

namespace PtzJoystickControl.Core.Commands;

public interface ICommand
{
    string CommandName { get; }
    string AxisParameterName { get; }
    string ButtonParameterName { get; }
    IEnumerable<CommandValueOption> Options { get; }

    void Execute(int value);
    void Execute(CommandValueOption value);
}
