using PtzJoystickControl.Core.Devices;

namespace PtzJoystickControl.Core.Model;

public class GamepadSettings
{
    public string Id { get; set; } = null!;
    public List<InputSettings>? Inputs { get; set; }
    public string Name { get; set; } = null!;
    public bool IsActivated { get; set; }

    public GamepadSettings()
    {
    }

    public GamepadSettings(IGamepad gmaepad)
    {
        Id = gmaepad.Id;
        Inputs = new List<InputSettings>(gmaepad.Inputs.Count());
        foreach (IInput input in gmaepad.Inputs)
        {
            Inputs.Add(new InputSettings(input));
        }
        Name = gmaepad.Name;
        IsActivated = gmaepad.IsActivated;
    }
}
