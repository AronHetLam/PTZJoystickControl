using PtzJoystickControl.Core.Model;

namespace PtzJoystickControl.Core.Db
{
    public interface IGamepadSettingsStore
    {
        List<GamepadSettings> GetAllGamepadSettings();
        GamepadSettings? GetGamepadSettingsById(string id);
        bool SaveGamepadSettings(GamepadSettings GamepadSettings);
    }
}