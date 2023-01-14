using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Services;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Runtime.CompilerServices;

namespace PtzJoystickControl.Gui.ViewModels;

public class GamepadsViewModel : ViewModelBase, INotifyPropertyChanged
{
    private readonly IGamepadsService _gamepadsService;
    private IGamepadInfo? _selectedGamepadInfo;
    private IGamepad? _selectedGamepad;
    private IEnumerable<InputViewModel>? _inputViewModels = Enumerable.Empty<InputViewModel>();

    public GamepadsViewModel(IGamepadsService gamepadsService)
    {
        _gamepadsService = gamepadsService;
        SelectedGamepadInfo = _gamepadsService.Gamepads.FirstOrDefault(g => g.IsActivated);
    }

    public ObservableCollection<IGamepadInfo> AvailableGamepads => _gamepadsService.Gamepads;
    public IGamepadInfo? SelectedGamepadInfo
    {
        get => _selectedGamepadInfo;
        set
        {
            if (_selectedGamepadInfo != value)
            {
                if (_selectedGamepadInfo != null)
                {
                    _gamepadsService.DeactivateGamepad(_selectedGamepadInfo);
                    _selectedGamepadInfo.PropertyChanged -= OnSelecetdGamepadInfoPropertyChanged;
                }

                _selectedGamepadInfo = value;

                if (_selectedGamepadInfo != null)
                {
                    _gamepadsService.ActivateGamepad(_selectedGamepadInfo);
                    _selectedGamepadInfo.PropertyChanged += OnSelecetdGamepadInfoPropertyChanged;
                }

                SelectedGamepad = _gamepadsService.ActiveGamepads.FirstOrDefault(g => g.Id == _selectedGamepadInfo?.Id);
                NotifyPropertyChanged();
            }
        }
    }

    public IGamepad? SelectedGamepad
    {
        get => _selectedGamepad;
        set
        {
            if (_selectedGamepad != value)
            {
                if (_selectedGamepad != null)
                    _selectedGamepad.PropertyChanged -= OnSelecetdGamepadPropertyChanged;

                _selectedGamepad = value;

                if (_selectedGamepad != null)
                    _selectedGamepad.PropertyChanged += OnSelecetdGamepadPropertyChanged;

                InputViewModels = _selectedGamepad?.Inputs.Select(i => new InputViewModel(i));
                NotifyPropertyChanged();
            }
        }
    }

    private void OnSelecetdGamepadPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IGamepad.IsConnected) && !SelectedGamepad!.IsConnected)
            SelectedGamepad = null;
        else
            NotifyPropertyChanged(e.PropertyName ?? "");
    }

    public void OnSelecetdGamepadInfoPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (e.PropertyName == nameof(IGamepadInfo.IsActivated)
            && SelectedGamepad == null
            && SelectedGamepadInfo!.IsActivated
            && SelectedGamepadInfo!.IsConnected)
            SelectedGamepad = _gamepadsService.ActiveGamepads.FirstOrDefault(g => g.Id == _selectedGamepadInfo?.Id);

        NotifyPropertyChanged(e.PropertyName ?? "");
    }

    public IEnumerable<InputViewModel>? InputViewModels
    {
        get => _inputViewModels;
        set
        {
            _inputViewModels = value;
            NotifyPropertyChanged();
        }
    }


    public new event PropertyChangedEventHandler? PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
