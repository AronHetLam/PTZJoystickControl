using System.Text.Json;
using PtzJoystickControl.Core.Devices;
using System.Diagnostics;
using PtzJoystickControl.Core.Db;
using PtzJoystickControl.Core.Model;
using System.Runtime.InteropServices;

namespace PtzJoystickControl.Application.Db
{
    public class CameraSettingsStore : ICameraSettingsStore
    {
        private readonly string _cameratFilePath;

        public CameraSettingsStore()
        {
            if (RuntimeInformation.IsOSPlatform(OSPlatform.Linux))
                _cameratFilePath = Path.Combine(Environment.GetFolderPath(
                    Environment.SpecialFolder.UserProfile),
                    ".PTZJoystickControl/Cameras.json");
            else
                _cameratFilePath = Path.Combine(
                    Environment.GetFolderPath(Environment.SpecialFolder.ApplicationData),
                    "PTZJoystickControl/Cameras.json");

        }

        public IEnumerable<ViscaDeviceBase> GetAllCameras()
        {
            if (!File.Exists(_cameratFilePath))
                return new List<ViscaIPDeviceBase>();

            try
            {
                string serializedViscaDeviceSettings = File.ReadAllText(_cameratFilePath);
                var viscaDeviceSettingsList = JsonSerializer.Deserialize<IEnumerable<ViscaDeviceSettings>>(serializedViscaDeviceSettings) ?? Enumerable.Empty<ViscaDeviceSettings>();
                return viscaDeviceSettingsList
                    .Select(viscaDeviceSettings =>
                    {
                        var viscaDevice = (ViscaDeviceBase?)Activator.CreateInstance(
                            Type.GetType(viscaDeviceSettings.IViscaDeveiceTypeAssemblyQualifiedName)!,
                            viscaDeviceSettings.Name);
                        if (viscaDevice is ViscaIPDeviceBase viscaIPDevice)
                        {
                            viscaIPDevice.IPAddress = viscaDeviceSettings.IpAddress;
                            viscaIPDevice.Port = viscaDeviceSettings.Port;
                            viscaIPDevice.Protocol = viscaDeviceSettings.Protocol;
                            viscaIPDevice.SendWaitTime = viscaDeviceSettings.SendWaitTime;
                            viscaIPDevice.UseHeader = viscaDeviceSettings.UseHeader;
                        }
                        return viscaDevice;
                    })
                    .Where(viscaDevice => viscaDevice != null)
                    .ToList()!;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return new List<ViscaIPDeviceBase>();
            }
        }

        public bool SaveCameras(IEnumerable<ViscaDeviceBase> viscaDevices)
        {
            try
            {
                var viscaDeviceSettings = viscaDevices.Select(viscaDevice => new ViscaDeviceSettings(viscaDevice));
                string serializedViscaDeviceSettings = JsonSerializer.Serialize(viscaDeviceSettings);
                File.WriteAllText(_cameratFilePath, serializedViscaDeviceSettings);
                return true;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                return false;
            }
        }
    }
}
