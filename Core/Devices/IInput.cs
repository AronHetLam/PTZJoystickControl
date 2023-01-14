using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Model;
using System.ComponentModel;

namespace PtzJoystickControl.Core.Devices;

public interface IInput : INotifyPropertyChanged
{
    ICommand? SelectedCommand { get; set; }
    CommandDirectionOption? CommandDirection { get; set; }
    IEnumerable<ICommand>? Commands { get; }
    CommandValueOption? CommandValue { get; set; }
    public Direction CurrentDirection { get; }
    public IInput? SecondInput { get; }
    int CurrentValue { get; }
    IEnumerable<CommandDirectionOption>? Directions { get; }
    InputType InputType { get; }
    float InputValue { get; set; }
    float RawInputValue { get; }
    float DeadZone { get; set; }
    float Saturation { get; set; }
    bool Inverted { get; set; }
    bool DefaultCenter { get; set; }
    string Name { get; }
    string Id { get; }
    IEnumerable<CommandValueOption>? Values { get; }

    event PropertyChangedEventHandler PersistentPropertyChanged;
}
