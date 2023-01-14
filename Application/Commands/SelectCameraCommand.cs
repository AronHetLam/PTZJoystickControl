using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using PtzJoystickControl.Core.Model;
using PtzJoystickControl.Application.Devices;

namespace PtzJoystickControl.Application.Commands;

public class SelectCameraCommand : IStaticCommand, INotifyPropertyChanged, INotifyCollectionChanged
{
    public SelectCameraCommand(IGamepad gamepad) : base(gamepad)
    {
        Cameras = gamepad.Cameras ?? new ObservableCollection<ViscaDeviceBase>();
        gamepad.PropertyChanged += Gamepad_PropertyChanged;
    }

    private void Gamepad_PropertyChanged(object? sender, PropertyChangedEventArgs? e)
    {
        if (e?.PropertyName == nameof(Gamepad.Cameras))
            Cameras = Gamepad.Cameras ?? new ObservableCollection<ViscaDeviceBase>();
    }

    private static ObservableCollection<ViscaDeviceBase> cameras = null!;

    private ObservableCollection<ViscaDeviceBase> Cameras
    {
        get { return cameras; }
        set
        {
            if (value == null) throw new ArgumentNullException(nameof(value));
            if (cameras != null)
            {
                cameras.CollectionChanged -= OnCamerasChanged;
                foreach (var camera in cameras)
                    camera.PropertyChanged -= OnDeviceChange;
            }

            cameras = value;
            
            cameras.CollectionChanged += OnCamerasChanged;
            foreach (var camera in cameras)
                camera.PropertyChanged += OnDeviceChange;
            NotifyPropertyChanged();
        }
    }

    public void OnCamerasChanged(object? o, NotifyCollectionChangedEventArgs e)
    {
        if (e.Action != NotifyCollectionChangedAction.Move && (e.OldItems?.Contains(Gamepad.SelectedCamera) ?? false))
            Gamepad.SelectedCamera = null;

        if (e != null)
        {
            if (e.OldStartingIndex >= 0)
                foreach (ViscaIpDevice device in e.OldItems!) device.PropertyChanged -= OnDeviceChange;

            if (e.NewStartingIndex >= 0)
                foreach (ViscaIpDevice device in e.NewItems!) device.PropertyChanged += OnDeviceChange;
        }

        NotifyCollectionChanged(e!);
    }

    private void OnDeviceChange(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(ViscaDeviceBase.Name))
            NotifyPropertyChanged(nameof(Options));
    }

    public override string CommandName => "Select camera";

    public override string AxisParameterName => "Camera";

    public override string ButtonParameterName => "Camera";

    public override IEnumerable<CommandValueOption> Options => Cameras
        .Select((val, i) => new CommandValueOption(val.Name, i));


    public override void Execute(int value)
    {
        Gamepad.SelectedCamera = 0 <= value && value < Cameras.Count() ? Cameras[value] : throw new ArgumentOutOfRangeException(
            $"Value out of range for Camera ObservableCollection. Count is {Cameras.Count()}, value was {value}");
    }

    public event PropertyChangedEventHandler? PropertyChanged;
    public event NotifyCollectionChangedEventHandler? CollectionChanged;

    protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    protected void NotifyCollectionChanged(NotifyCollectionChangedEventArgs collectionChangeEventArgs)
    {
        CollectionChanged?.Invoke(this, collectionChangeEventArgs);
    }
}
