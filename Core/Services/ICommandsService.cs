using PtzJoystickControl.Core.Commands;
using PtzJoystickControl.Core.Devices;

namespace PtzJoystickControl.Core.Services;

public interface ICommandsService
{
    public IEnumerable<ICommand> GetCommandsForGamepad(IGamepad gamepad);
}
