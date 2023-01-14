using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Model;

namespace PtzJoystickControl.Application.Commands;

public class PowerCommand : IStaticCommand
{
    public PowerCommand(IGamepad gamepad) : base(gamepad)
    {
    }

    public override string CommandName => "Power";

    public override string AxisParameterName => "Action";

    public override string ButtonParameterName => "Action";

    public override IEnumerable<CommandValueOption> Options => optionsList;
    private static readonly IEnumerable<CommandValueOption> optionsList = new CommandValueOption[] 
    {
        new CommandValueOption("Power On", (int)Power.On),
        new CommandValueOption("Power Off", (int)Power.Off),
    };

    public override void Execute(int value)
    {
        byte byteVal = (byte)value;
        if (Enum.IsDefined((Power)byteVal))
            Gamepad.SelectedCamera?.Power((Power)byteVal);
        else
            throw new ArgumentException("Invalid value. Must be one of enum Power.");
    }
}
