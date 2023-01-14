using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Model;

namespace PtzJoystickControl.Application.Commands;

public class PresetRecallSpeedComamnd : IStaticCommand
{
    public PresetRecallSpeedComamnd(IGamepad gamepad) : base(gamepad)
    {
    }

    public override string CommandName => "Preset Recall Speed";

    public override string AxisParameterName => "Speed";

    public override string ButtonParameterName => "Speed";

    private static readonly IEnumerable<CommandValueOption> optionsList = Enumerable
        .Range(1, 24)
        .Select(i => new CommandValueOption(i.ToString(), i))
        .ToArray();

    public override IEnumerable<CommandValueOption> Options => optionsList;

    public override void Execute(int value)
    {
        if (1 > value || value > 24) throw new ArgumentException($"Recall speed must be between 1 and 24 (incluseve), {value} was geven.");

        Gamepad.SelectedCamera?.PresetRecallSpeed((byte)value);
    }
}
