using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Model;

namespace PtzJoystickControl.Application.Commands;

public class ZoomCommand : IDynamicCommand
{
    public ZoomCommand(IGamepad gamepad) : base(gamepad)
    {
    }

    public override string CommandName => "Zoom";
    public override string AxisParameterName => "Max speed";
    public override string ButtonParameterName => "Speed";

    public override int MaxValue => 7;
    public override int MinValue => 0;

    public override IEnumerable<CommandDirectionOption> ButtonDirections => buttonDirectionsList;
    private static readonly IReadOnlyCollection<CommandDirectionOption> buttonDirectionsList = new CommandDirectionOption[] {
        new CommandDirectionOption("Tele", Direction.High),
        new CommandDirectionOption("Wide", Direction.Low),
    };

    public override void Execute(int value, Direction direction)
    {
        if (0 > value || value > MaxValue) throw new ArgumentException($"Max zoom speed is {MaxValue}, {value} was geven.");

        ZoomDir zoomDir;

        switch (direction)
        {
            case Direction.Stop:
                zoomDir = ZoomDir.Stop;
                break;
            case Direction.High:
                zoomDir = ZoomDir.Tele;
                break;
            case Direction.Low:
                zoomDir = ZoomDir.Wide;
                break;
            default:
                zoomDir = ZoomDir.Stop;
                break;
        }

        Gamepad.SelectedCamera?.Zoom((byte)value, zoomDir);
    }
}
