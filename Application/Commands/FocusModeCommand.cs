using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Model;

namespace PtzJoystickControl.Application.Commands;

public class FocusModeCommand : IStaticCommand
{
    public FocusModeCommand(IGamepad gamepad) : base(gamepad)
    {
    }

    public override string CommandName => "Focus mode";

    public override string AxisParameterName => "Action";

    public override string ButtonParameterName => "Action";

    public override IEnumerable<CommandValueOption> Options => optionsList;
    private static readonly IEnumerable<CommandValueOption> optionsList = new CommandValueOption[] {
        new CommandValueOption("Manual focus", (int)FocusMode.Manual),
        new CommandValueOption("Auto focus", (int)FocusMode.Auto),
        new CommandValueOption("Toggle mode", (int)FocusMode.Toggle),
    };

    public override void Execute(int value)
    {
        byte byteVal = (byte)value;
        if (Enum.IsDefined((FocusMode)byteVal))
            Gamepad.SelectedCamera?.FocusMode((FocusMode)byteVal);
        else
            throw new ArgumentException("Invalid value. Must be one of enum FocusMode.");
    }
}
