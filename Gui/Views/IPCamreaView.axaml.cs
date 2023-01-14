using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PtzJoystickControl.Gui.Views;

public partial class IPCameraView : UserControl
{
    public IPCameraView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
