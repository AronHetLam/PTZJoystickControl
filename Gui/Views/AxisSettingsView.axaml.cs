using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PtzJoystickControl.Gui.Views;

public partial class AxisSettingsView : UserControl
{
    public AxisSettingsView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }
}
