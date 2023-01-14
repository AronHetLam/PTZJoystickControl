using PtzJoystickControl.Core.Commands;
using System.Collections.ObjectModel;
using System.ComponentModel;

namespace PtzJoystickControl.Core.Devices;

public interface IGamepad : IGamepadInfo, INotifyPropertyChanged
{
    //public abstract Guid Id { get; }
    //public abstract string Name { get; }
    public abstract ObservableCollection<ViscaDeviceBase>? Cameras { get; set; }
    public abstract IReadOnlyList<ICommand> Commands { get; }
    public abstract bool DetectInput { get; set; }
    public abstract IEnumerable<IInput> Inputs { get; }
    public abstract ViscaDeviceBase? SelectedCamera { get; set; }

    public abstract event PropertyChangedEventHandler? PersistentPropertyChanged;

    public void Acquire();
    public void Unacquire();
    public abstract void Update();
}
