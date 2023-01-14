using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Model;

namespace PtzJoystickControl.Application.Commands;

public class FocusLockCommand : IStaticCommand
{
    public FocusLockCommand(IGamepad gamepad) : base(gamepad)
    {
    }

    public override string CommandName => "Focus lock";

    public override string AxisParameterName => "Action";

    public override string ButtonParameterName => "Action";

    public override IEnumerable<CommandValueOption> Options => optionsList;
    private static readonly IEnumerable<CommandValueOption> optionsList = new CommandValueOption[] {
        new CommandValueOption("Lock", (int)FocusLock.Lock),
        new CommandValueOption("Unlock", (int)FocusLock.Unlock),
    };

    public override void Execute(int value)
    {
        if (Enum.IsDefined((FocusLock)value))
            Gamepad.SelectedCamera?.FocusLock((FocusLock)value);
        else
            throw new ArgumentException("Invalid value. Must be one of enum FocusLock");

    }
}
