using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Model;

namespace PtzJoystickControl.Application.Commands;

public class PanCommand : IDynamicCommand
{
    public PanCommand(IGamepad gamepad) : base(gamepad)
    {
    }

    public override string CommandName => "Pan";
    public override string AxisParameterName => "Max speed";
    public override string ButtonParameterName => "Speed";

    public override int MaxValue => 24; //Max pan speed
    public override int MinValue => 1;
    public override IEnumerable<CommandDirectionOption> ButtonDirections => buttonDirectionsList;
    public static readonly IReadOnlyCollection<CommandDirectionOption> buttonDirectionsList = new CommandDirectionOption[] {
        new CommandDirectionOption("Right", Direction.High),
        new CommandDirectionOption("Left", Direction.Low)
    };

    public override void Execute(int value, Direction direction)
    {
        if (0 > value || value > MaxValue) throw new ArgumentException($"Max pan speed is {MaxValue}, {value} was geven.");

        PanDir panDir;

        switch (direction)
        {
            case Direction.Stop:
                panDir = PanDir.Stop;
                break;
            case Direction.High:
                panDir = PanDir.Right;
                break;
            case Direction.Low:
                panDir = PanDir.Left;
                break;
            default:
                panDir = PanDir.Stop;
                break;
        }

        Gamepad.SelectedCamera?.Pan((byte)value, panDir);
    }

}
