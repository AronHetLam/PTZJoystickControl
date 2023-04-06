using Avalonia.Utilities;
using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Model;
using System.Collections.Generic;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PtzJoystickControl.Gui.ViewModels;

public class InputViewModel : ViewModelBase, INotifyPropertyChanged
{
    private readonly IInput _input;
    private readonly InputViewModel? _secondInputViewModel;

    public InputViewModel(IInput input)
   {
        _input = input;
        _secondInputViewModel = input.SecondInput != null ? new InputViewModel(input.SecondInput) : null;
        WeakEventHandlerManager.Subscribe<INotifyPropertyChanged, PropertyChangedEventArgs, InputViewModel>(input, nameof(input.PropertyChanged), OnInputPropertyCahnged);
    }

    public string Name { get => _input.Name; }
    public IEnumerable<ICommand>? Commands { get => _input.Commands; }
    public ICommand? SelectedCommand
    {
        get => _input.SelectedCommand;
        set => _input.SelectedCommand = value;
    }

    public float InputValue { get => _input.InputValue; }
    public float RawInputValue { get => _input.RawInputValue; }
    public int MinValue { get => InputType == InputType.Axis ? -1 : 0; }
    public static int MaxValue { get => 1; }
    public float DeadZone { get => _input.DeadZone; set => _input.DeadZone = value; }
    public float Saturation { get => _input.Saturation; set => _input.Saturation = value; }
    public string OutputValue { get => $"{_input.CurrentDirection} {_input.CurrentValue}"; }

    public bool Inverted { get => _input.Inverted; set => _input.Inverted = value; }
    public bool DefaultCenter { get => _input.DefaultCenter; set => _input.DefaultCenter = value; }
    public bool IsAxis { get => InputType == InputType.Axis; }


    public IEnumerable<CommandValueOption>? Values { get => _input.Values; }
    public CommandValueOption? CommandValue
    {
        get => _input.CommandValue;
        set
        {
            if (value! != null!)
                _input.CommandValue = value;
        }
    }

    public IEnumerable<CommandDirectionOption>? Directions { get => _input.Directions; }
    public CommandDirectionOption? CommandDirection
    {
        get => _input.CommandDirection;
        set => _input.CommandDirection = value;
    }

    public bool IsCleared { get => _input.SelectedCommand != null; }
    public InputType InputType { get => _input.InputType; }

    public void ClearCommand()
    {
        SelectedCommand = null;
    }

    public bool ShowSecondCommand { get => IsAxis && SelectedCommand is IStaticCommand && _secondInputViewModel != null; }
    public bool DisableDirection { get => IsAxis && _secondInputViewModel == null; }
    public InputViewModel? SecondInputdiewModel { get => _secondInputViewModel; }

    private void OnInputPropertyCahnged(object? sender, PropertyChangedEventArgs e)
    {
        NotifyPropertyChanged(e?.PropertyName ?? "");
        if (e?.PropertyName == nameof(_input.SelectedCommand))
        {
            NotifyPropertyChanged(nameof(IsCleared));
            if (IsAxis)
                NotifyPropertyChanged(nameof(ShowSecondCommand));
        }

        else if (e?.PropertyName == nameof(_input.CurrentValue) || e?.PropertyName == nameof(_input.CurrentDirection))
            NotifyPropertyChanged(nameof(OutputValue));
    }

    public new event PropertyChangedEventHandler? PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
