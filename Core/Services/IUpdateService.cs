using PtzJoystickControl.Core.Model;

namespace PtzJoystickControl.Core.Services
{
    public interface IUpdateService
    {
        Task<Update> CheckForUpdate();
    }
}
