using PtzJoystickControl.Core.Devices;

namespace PtzJoystickControl.Core.Model;

public class InputSettings
{

    public string? CommandType { get; set; }
    public CommandDirectionOption? CommandDirection { get; set; }
    public CommandValueOption? CommandValue { get; set; }
    public string Id { get; set; } = null!;
    public bool Inverted { get; set; }
    public float DeadZoneHigh { get; set; }
    public float DeadZoneLow { get; set; }
    public InputSettings? SecondInputSettings { get; set; }

    public InputSettings()
    {
    }

    public InputSettings(IInput input)
    {
        if (input == null) throw new ArgumentNullException("input");

        CommandType = input.SelectedCommand?.GetType().ToString();
        CommandDirection = input.CommandDirection;
        CommandValue = input.CommandValue;
        Id = input.Id;
        Inverted = input.Inverted;
        DeadZoneHigh = input.Saturation;
        DeadZoneLow = input.DeadZone;
        SecondInputSettings = input.SecondInput != null ? new InputSettings(input.SecondInput) : null;
    }
}