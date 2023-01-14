using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Model;

namespace PtzJoystickControl.Application.Commands;

public class TiltCommand : IDynamicCommand
{
    public TiltCommand(IGamepad gamepad) : base(gamepad)
    {
    }

    public override string CommandName => "Tilt";
    public override string AxisParameterName => "Max speed";
    public override string ButtonParameterName => "Speed";

    public override IEnumerable<CommandDirectionOption> ButtonDirections => buttonDirectionsList;
    private static readonly IReadOnlyCollection<CommandDirectionOption> buttonDirectionsList = new CommandDirectionOption[] {
        new CommandDirectionOption("Up", Direction.High),
        new CommandDirectionOption("Down", Direction.Low),
    };

    public override int MaxValue => 20; //Max tilt speed
    public override int MinValue => 1;

    public override void Execute(int value, Direction direction)
    {
        if (0 > value || value > MaxValue) throw new ArgumentException($"Max tilt speed is {MaxValue}, {value} was geven.");

        TiltDir tiltDir;

        switch (direction)
        {
            case Direction.Stop:
                tiltDir = TiltDir.Stop;
                break;
            case Direction.High:
                tiltDir = TiltDir.Up;
                break;
            case Direction.Low:
                tiltDir = TiltDir.Down;
                break;
            default:
                tiltDir = TiltDir.Stop;
                break;
        }

        Gamepad.SelectedCamera?.Tilt((byte)value, tiltDir);
    }
}
