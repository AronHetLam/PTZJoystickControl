using PtzJoystickControl.Application.Commands;
using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Services;

namespace PtzJoystickControl.Application.Services;

public class CommandsService : ICommandsService
{
    public IEnumerable<ICommand> GetCommandsForGamepad(IGamepad gamepad)
    {
        return new ICommand[]
        {
            new PanCommand(gamepad),
            new TiltCommand(gamepad),
            new ZoomCommand(gamepad),
            new FocusMoveCommand(gamepad),
            new FocusModeCommand(gamepad),
            new FocusLockCommand(gamepad),
            new PresetCommand(gamepad),
            new PresetRecallSpeedComamnd(gamepad),
            new SelectCameraCommand(gamepad),
            new PowerCommand(gamepad)
        };
    }
}
