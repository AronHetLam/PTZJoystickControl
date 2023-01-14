using Avalonia.Controls;
using Avalonia.Markup.Xaml;
using Avalonia;
using Avalonia.Data;

namespace PtzJoystickControl.Gui.Views;

public partial class InputView : UserControl
{
    public InputView()
    {
        InitializeComponent();
    }

    private void InitializeComponent()
    {
        AvaloniaXamlLoader.Load(this);
    }

    //public static readonly DirectProperty<InputView, bool> ShowAxisSettingsProperty =
    //    AvaloniaProperty.RegisterDirect<InputView, bool>(nameof(ShowAxisSettings), o => o.ShowAxisSettings, (o, v) => o.ShowAxisSettings = v, false, BindingMode.OneWay);

    //private bool _showAxisSettings = false;

    //public bool ShowAxisSettings
    //{
    //    get { return _showAxisSettings; }
    //    set { SetAndRaise(ShowAxisSettingsProperty, ref _showAxisSettings, value); }
    //}

}
