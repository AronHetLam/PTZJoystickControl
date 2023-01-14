using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Model;

namespace PtzJoystickControl.Application.Commands;

public class PresetCommand : IStaticCommand
{
    public PresetCommand(IGamepad gamepad) : base(gamepad)
    {
    }

    public override string CommandName => "Preset";

    public override string AxisParameterName => "Action";

    public override string ButtonParameterName => "Action";

    public override IEnumerable<CommandValueOption> Options => optionsList;
    private static readonly IEnumerable<CommandValueOption> optionsList = Enum
        .GetValues<Preset>()
        //.OfType<Preset>()
        .SelectMany(value =>
        {
            string name = Enum.GetName(value)!;
            int shiftedValue = (byte)value << 7;
            return Enumerable.Range(0, 128)
                .Select(number => new CommandValueOption($"{name} - {number + 1}", shiftedValue | number));
        })
        .Prepend(new CommandValueOption("Toggle Set", -1))
        .ToArray();

    public static bool SetPreset { get; set; }

    public override void Execute(int value)
    {
        int presetNumber = value & 0x7F;
        int preset = value >> 7;
        if (Enum.IsDefined((Preset)preset) && 0 <= presetNumber && presetNumber <= 127)
        {
            if (SetPreset)
            {
                preset = (byte)Preset.Set;
                SetPreset = false;
            }
            Gamepad.SelectedCamera?.Preset((Preset)preset, (byte)presetNumber);
        }
        else if (value == -1)
            SetPreset = true;
        else
            throw new ArgumentException("Invalid value. Must be one of enum Preset or -1");
    }
}
