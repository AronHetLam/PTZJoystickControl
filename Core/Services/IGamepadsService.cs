using PtzJoystickControl.Core.Devices;
using System.Collections.ObjectModel;

namespace PtzJoystickControl.Core.Services;

public interface IGamepadsService
{
    public ObservableCollection<IGamepadInfo> Gamepads { get; }
    public ObservableCollection<IGamepad> ActiveGamepads { get; }

    void ActivateGamepad(IGamepadInfo gamepad);
    void DeactivateGamepad(IGamepadInfo gamepad);
}
