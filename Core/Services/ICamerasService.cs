using PtzJoystickControl.Core.Devices;
using System.Collections.ObjectModel;

namespace PtzJoystickControl.Core.Services;

public interface ICamerasService
{
    public ObservableCollection<ViscaDeviceBase> Cameras { get; }
    public void AddCamera(ViscaDeviceBase camera);
    public void RemoveCamera(ViscaDeviceBase camera);
}