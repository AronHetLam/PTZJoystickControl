using PtzJoystickControl.Core.Db;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Model;
using PtzJoystickControl.Core.Services;
using PtzJoystickControl.SdlGamepads.Devices;
using PtzJoystickControl.SdlGamepads.Models;
using SDL2;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Diagnostics;

namespace PtzJoystickControl.SdlGamepads.Services;

public class SdlGamepadsService : IGamepadsService
{
    private readonly IGamepadSettingsStore _gamppadSettingsDb;
    private readonly ICamerasService _camerasService;
    private readonly ICommandsService _commandsService;
    public ObservableCollection<IGamepadInfo> Gamepads { get; private set; } = new();
    public ObservableCollection<IGamepad> ActiveGamepads { get; private set; } = new();

    public SdlGamepadsService(IGamepadSettingsStore gamppadSettingsDb, ICamerasService camerasService, ICommandsService commandsService)
    {
        SDL.SDL_Init(SDL.SDL_INIT_JOYSTICK);

        _gamppadSettingsDb = gamppadSettingsDb;
        _camerasService = camerasService;
        _commandsService = commandsService;

        _gamppadSettingsDb.GetAllGamepadSettings().ForEach(x => Gamepads.Add(new SdlGamepadInfo()
        {
            Id = x.Id,
            Name = x.Name,
            IsConnected = false,
            IsActivated = x.IsActivated,
            DeviceIndex = -1,
            InstanceId = -1
        }));

        Task.Run(UpdateLoop);
    }

    public void ActivateGamepad(IGamepadInfo gamepadInfo)
    {
        if (gamepadInfo is SdlGamepadInfo sdlGamepadInfo && !ActiveGamepads.Any(g => g.Id == gamepadInfo.Id))
        {
            if (gamepadInfo.IsConnected)
            {
                var gamepad = LoadGamepad(sdlGamepadInfo);
                gamepad.PersistentPropertyChanged += GamepadPersistentPropertyChanged;
                gamepad.Acquire();
                gamepad.IsActivated = true;
                ActiveGamepads.Add(gamepad);
            }
            sdlGamepadInfo.IsActivated = true;
        }
    }

    public void DeactivateGamepad(IGamepadInfo gamepadInfo)
    {
        if (gamepadInfo is SdlGamepadInfo sdlGamepadInfo)
        {
            var gamepad = (SdlGamepad?)ActiveGamepads.FirstOrDefault(g => g.Id == sdlGamepadInfo.Id);
            if (gamepad != null)
            {
                gamepad.IsActivated = false;
                DisconnectGamepad(gamepad);
            }

            sdlGamepadInfo.IsActivated = false;
        }
    }

    private void DisconnectGamepad(SdlGamepad gamepad)
    {
        if (ActiveGamepads.Remove(gamepad))
        {
            gamepad.Unacquire();
            gamepad.PersistentPropertyChanged -= GamepadPersistentPropertyChanged;
        }
    }

    private SdlGamepad LoadGamepad(SdlGamepadInfo gamepadInfo)
    {
        var gamepad = new SdlGamepad(gamepadInfo, _commandsService, _camerasService.Cameras);

        GamepadSettings? gamepadSettings = _gamppadSettingsDb.GetGamepadSettingsById(gamepadInfo.Id);
        if (gamepadSettings == null)
            return gamepad;

        var commandsDict = gamepad.Commands.ToDictionary(val => val.GetType().ToString());
        foreach (IInput input in gamepad.Inputs)
        {
            InputSettings? storedInput = gamepadSettings.Inputs?.FirstOrDefault(i => i.Id == input.Id);

            if (storedInput != null)
            {
                if (storedInput.CommandType != null && commandsDict.TryGetValue(storedInput.CommandType, out var command))
                    input.SelectedCommand = command;
                input.CommandDirection = storedInput.CommandDirection;
                input.CommandValue = storedInput.CommandValue;
                input.Inverted = storedInput.Inverted;
                input.Saturation = storedInput.DeadZoneHigh;
                input.DeadZone = storedInput.DeadZoneLow;
                if(input.SecondInput != null && storedInput.SecondInputSettings != null) {
                    if (storedInput.SecondInputSettings.CommandType != null && commandsDict.TryGetValue(storedInput.SecondInputSettings.CommandType, out var secondCommand))
                        input.SecondInput.SelectedCommand = secondCommand;
                    input.SecondInput.CommandDirection = storedInput.SecondInputSettings.CommandDirection;
                    input.SecondInput.CommandValue = storedInput.SecondInputSettings.CommandValue;
                }
            }
        }

        return gamepad;
    }

    private async Task UpdateLoop()
    {
        DateTime lastUpdate = DateTime.MinValue;
        TimeSpan updateInterval = TimeSpan.FromMilliseconds(15);
        TimeSpan delayMs;
        while (true)
        {
            delayMs = updateInterval - (DateTime.UtcNow - lastUpdate);
            if (delayMs.Ticks > 0)
                await Task.Delay(delayMs);
            lastUpdate = DateTime.UtcNow;

            try
            {
                while (SDL.SDL_PollEvent(out var sdlEvent) > 0)
                {
                    switch (sdlEvent.type)
                    {
                        case SDL.SDL_EventType.SDL_JOYAXISMOTION:
                            OnJoystickAxisMotion(sdlEvent.jaxis);
                            break;
                        case SDL.SDL_EventType.SDL_JOYBALLMOTION:
                            break;
                        case SDL.SDL_EventType.SDL_JOYHATMOTION:
                            break;
                        case SDL.SDL_EventType.SDL_JOYBUTTONDOWN:
                        case SDL.SDL_EventType.SDL_JOYBUTTONUP:
                            OnJoystickButtonEvent(sdlEvent.jbutton);
                            break;
                        case SDL.SDL_EventType.SDL_JOYDEVICEADDED:
                            OnJoystickAdded(sdlEvent.jdevice);
                            break;
                        case SDL.SDL_EventType.SDL_JOYDEVICEREMOVED:
                            OnJoystickRemoved(sdlEvent.jdevice);
                            break;
                        case SDL.SDL_EventType.SDL_CONTROLLERAXISMOTION:
                            break;
                        case SDL.SDL_EventType.SDL_CONTROLLERBUTTONDOWN:
                            break;
                        case SDL.SDL_EventType.SDL_CONTROLLERBUTTONUP:
                            break;
                        case SDL.SDL_EventType.SDL_CONTROLLERDEVICEADDED:
                            break;
                        case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMOVED:
                            break;
                        case SDL.SDL_EventType.SDL_CONTROLLERDEVICEREMAPPED:
                            break;
                        case SDL.SDL_EventType.SDL_CONTROLLERTOUCHPADDOWN:
                            break;
                        case SDL.SDL_EventType.SDL_CONTROLLERTOUCHPADMOTION:
                            break;
                        case SDL.SDL_EventType.SDL_CONTROLLERTOUCHPADUP:
                            break;
                        case SDL.SDL_EventType.SDL_CONTROLLERSENSORUPDATE:
                            break;
                        default:
                            break;
                    }
                }
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
            }
        }
    }

    private void OnJoystickAxisMotion(SDL.SDL_JoyAxisEvent jaxis)
    {
        var instanceId = jaxis.which;
        ActiveGamepads.Cast<SdlGamepad>()
            .First(g => g.InstanceId == instanceId)
            .OnAxisEvent(jaxis);
    }

    private void OnJoystickButtonEvent(SDL.SDL_JoyButtonEvent jbutton)
    {
        var instanceId = jbutton.which;
        ActiveGamepads.Cast<SdlGamepad>()
            .First(g => g.InstanceId == instanceId)
            .OnButtonEvent(jbutton);
    }

    private void OnJoystickAdded(SDL.SDL_JoyDeviceEvent jdevice)
    {
        var deviceIndex = jdevice.which;
        var id = SDL.SDL_JoystickGetDeviceGUID(deviceIndex).ToString();
        var count = Gamepads.Count(g => g.Id.StartsWith(id) && g.IsConnected);
        id += $"#{count}";

        var gamepadInfo = Gamepads.FirstOrDefault(g => g.Id == id);
        if (gamepadInfo == null)
        {
            var name = SDL.SDL_JoystickNameForIndex(deviceIndex);
            if (count > 0) name += $" #{count + 1}";
            gamepadInfo = new SdlGamepadInfo()
            {
                Id = id,
                Name = name,
            };
            Gamepads.Add(gamepadInfo);
        }

        ((SdlGamepadInfo)gamepadInfo).DeviceIndex = deviceIndex;
        ((SdlGamepadInfo)gamepadInfo).InstanceId = SDL.SDL_JoystickGetDeviceInstanceID(deviceIndex);
        gamepadInfo.IsConnected = true;
        if (gamepadInfo.IsActivated)
            ActivateGamepad(gamepadInfo);
    }

    private void OnJoystickRemoved(SDL.SDL_JoyDeviceEvent jdevice)
    {
        var instanceId = jdevice.which;
        var gamepadInfo = Gamepads.FirstOrDefault(g => ((SdlGamepadInfo)g).InstanceId == instanceId);
        if (gamepadInfo == null)
            return;

        var gamepad = (SdlGamepad?)ActiveGamepads.FirstOrDefault(g => g.Id == gamepadInfo.Id);
        if(gamepad != null) 
        {
            DisconnectGamepad(gamepad);
            gamepad.IsConnected = false;
        }
        gamepadInfo.IsConnected = false;
    }

    private void GamepadPersistentPropertyChanged(object? sender, PropertyChangedEventArgs e)
    {
        if (sender is IGamepad gamepad)
            _gamppadSettingsDb.SaveGamepadSettings(new GamepadSettings(gamepad));
    }
}
