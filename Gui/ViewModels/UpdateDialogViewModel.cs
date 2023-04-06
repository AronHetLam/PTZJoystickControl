using PtzJoystickControl.Core.Model;
using System.Diagnostics;

namespace PtzJoystickControl.Gui.ViewModels;

public class UpdateDialogViewModel : ViewModelBase
{
    private readonly Update _update;
    public UpdateDialogViewModel(Update upate)
    {
        _update = upate;
    }

    public bool IsAvailable { get => _update.Available; }
    public string? LatestVersion { get => _update.Version; }
    public bool Error { get => _update.Error; }
    public string? ErrorMessage { get => _update.ErrorMessage; }
    public bool NotAvailableOrError { get => !(IsAvailable || Error); }
    public void Download()
    {
        if (_update.Uri != null) Process.Start(new ProcessStartInfo
        {
            FileName = _update.Uri.ToString(),
            UseShellExecute = true
        });
    }
}
