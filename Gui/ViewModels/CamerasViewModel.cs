using Avalonia.Utilities;
using PtzJoystickControl.Application.Devices;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Services;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PtzJoystickControl.Gui.ViewModels;

public class CamerasViewModel : ViewModelBase, INotifyPropertyChanged
{
    private readonly ICamerasService _camerasService;
    private readonly GamepadsViewModel _gamepadsViewModel;

    public CamerasViewModel(ICamerasService camerasService, GamepadsViewModel gamepadsViewModel)
    {
        _camerasService = camerasService;
        _gamepadsViewModel = gamepadsViewModel;
        Cameras = _camerasService.Cameras;

        WeakEventHandlerManager.Subscribe<INotifyCollectionChanged, NotifyCollectionChangedEventArgs, CamerasViewModel>(camerasService.Cameras, nameof(camerasService.Cameras.CollectionChanged), OnCamerasServicePropertyCahnged);
        WeakEventHandlerManager.Subscribe<INotifyPropertyChanged, PropertyChangedEventArgs, CamerasViewModel>(gamepadsViewModel, nameof(gamepadsViewModel.PropertyChanged), OnGamepadsViewModelPropertyCahnged);
    }

    private void OnGamepadsViewModelPropertyCahnged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IGamepad.SelectedCamera) || e.PropertyName == nameof(GamepadsViewModel.SelectedGamepad))
            NotifyPropertyChanged(nameof(IGamepad.SelectedCamera));
    }

    private void OnCamerasServicePropertyCahnged(object? sender, NotifyCollectionChangedEventArgs e)
    {
        NotifyPropertyChanged(nameof(Cameras));
    }

    public ObservableCollection<ViscaDeviceBase> Cameras { get; }

    public ViscaDeviceBase? SelectedCamera
    {
        get => _gamepadsViewModel.SelectedGamepad?.SelectedCamera;
        set
        {
            if (_gamepadsViewModel.SelectedGamepad != null)
                _gamepadsViewModel.SelectedGamepad.SelectedCamera = value;
        }
    }

    public void AddCamera() =>
        _camerasService.AddCamera(new ViscaIpDevice($"Camera #{Cameras.Count() + 1}"));

    public void RemoveCamera(object camera)
    {
        _camerasService.RemoveCamera((ViscaDeviceBase)camera);
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    private class CollectionChangedEventArgs
    {
    }
}
