using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Model;

namespace PtzJoystickControl.Application.Commands;

public class FocusMoveCommand : IDynamicCommand
{
    public FocusMoveCommand(IGamepad gamepad) : base(gamepad)
    {
    }

    public override string CommandName => "Manual focus";
    public override string AxisParameterName => "Max speed";
    public override string ButtonParameterName => "Speed";

    public override int MaxValue => 7; //Max focus speed
    public override int MinValue => 0;

    public override IEnumerable<CommandDirectionOption> ButtonDirections => buttonDirectionsList;
    private static readonly IReadOnlyCollection<CommandDirectionOption> buttonDirectionsList = new CommandDirectionOption[] {
        new CommandDirectionOption("Far", Direction.High),
        new CommandDirectionOption("Near", Direction.Low)
    };

    public override void Execute(int value, Direction direction)
    {
        if (0 > value || value > MaxValue) throw new ArgumentException($"Max focus speed is {MaxValue}, {value} was geven.");

        FocusDir focusDir;

        switch (direction)
        {
            case Direction.Stop:
                focusDir = FocusDir.Stop;
                break;
            case Direction.High:
                focusDir = FocusDir.Far;
                break;
            case Direction.Low:
                focusDir = FocusDir.Near;
                break;
            default:
                focusDir = FocusDir.Stop;
                break;
        }

        Gamepad.SelectedCamera?.Focus((byte)value, focusDir);
    }
}
