using PtzJoystickControl.Core.Db;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Services;
using System.Collections.ObjectModel;

namespace PtzJoystickControl.Application.Services;

public class CamerasService : ICamerasService
{
    private readonly ICameraSettingsStore _camerasDb;
    public ObservableCollection<ViscaDeviceBase> Cameras { get; } = new();

    public CamerasService(ICameraSettingsStore camerasDb)
    {
        _camerasDb = camerasDb;
        Cameras.CollectionChanged += CamerasCollectionChanged;
        LoadCamerasFromDb();
    }

    public void LoadCamerasFromDb()
    {
        var storedCameras = _camerasDb.GetAllCameras();
        foreach(var camera in Cameras)
            RemoveCamera(camera);

        if (storedCameras != null)
            foreach (var camera in storedCameras)
                AddCamera(camera);
    }

    public void AddCamera(ViscaDeviceBase camera)
    {
        Cameras.Add(camera);
        camera.PersistentPropertyChanged += CameraPersistentPropertyChanged;
    }

    public void RemoveCamera(ViscaDeviceBase camera)
    {
        Cameras.Remove(camera);
        camera.PersistentPropertyChanged -= CameraPersistentPropertyChanged;
    }

    private void CameraPersistentPropertyChanged(object? sender, System.ComponentModel.PropertyChangedEventArgs e)
    {
        if (sender is ViscaDeviceBase)
            _camerasDb.SaveCameras(Cameras);
    }

    private void CamerasCollectionChanged(object? sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
    {
        if (sender == Cameras)
            _camerasDb.SaveCameras(Cameras);
    }
}
