using System.ComponentModel;

namespace PtzJoystickControl.Core.Devices;

public interface IGamepadInfo : INotifyPropertyChanged
{
    public string Id { get; }
    public string Name { get; }
    bool IsActivated { get; set; }
    bool IsConnected { get; set; }
}
