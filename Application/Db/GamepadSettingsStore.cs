using PtzJoystickControl.Core.Db;
using PtzJoystickControl.Core.Model;
using System.Diagnostics;
using System.Runtime.InteropServices;
using System.Text.Json;

namespace PtzJoystickControl.Application.Db;

public class GamepadSettingsStore : IGamepadSettingsStore
{
    private List<GamepadSettings>? StoredGamepadSettings;
    private readonly string _camerasFilePath;

    public GamepadSettingsStore()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
            _camerasFilePath = Path.Combine(Environment.GetFolderPath(
               Environment.SpecialFolder.UserProfile),
               ".PTZJoystickControl/Gamepads.json");
        else
            _camerasFilePath = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                "PTZJoystickControl/Gamepads.json");
    }

    public List<GamepadSettings> GetAllGamepadSettings()
    {
        if (StoredGamepadSettings != null)
            return StoredGamepadSettings;

        if (!File.Exists(_camerasFilePath))
            return new List<GamepadSettings>();

        try
        {
            string serializedGamepadSettings = File.ReadAllText(_camerasFilePath);
            StoredGamepadSettings = JsonSerializer.Deserialize<List<GamepadSettings>>(serializedGamepadSettings)
                ?? new List<GamepadSettings>();
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            StoredGamepadSettings = new List<GamepadSettings>();
        }

        return StoredGamepadSettings;
    }

    public GamepadSettings? GetGamepadSettingsById(string id)
    {
        return GetAllGamepadSettings()?.FirstOrDefault((item) => item.Id == id);
    }

    public bool SaveGamepadSettings(GamepadSettings GamepadSettings)
    {
        var gamepadSettings = GetAllGamepadSettings();
        int i = gamepadSettings.FindIndex(g => g.Id == GamepadSettings.Id);

        if (i >= 0) gamepadSettings[i] = GamepadSettings;
        else gamepadSettings.Add(GamepadSettings);

        try
        {
            var serializedGamepadSettings = JsonSerializer.Serialize(gamepadSettings);
            File.WriteAllText(_camerasFilePath, serializedGamepadSettings);
            return true;
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            return false;
        }
    }
}
