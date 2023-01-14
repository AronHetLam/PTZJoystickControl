using PtzJoystickControl.Core.Devices;

namespace PtzJoystickControl.Core.Db
{
    public interface ICameraSettingsStore
    {
        IEnumerable<ViscaDeviceBase> GetAllCameras();
        bool SaveCameras(IEnumerable<ViscaDeviceBase> viscaDevices);
    }
}