using Avalonia.Controls;
using Avalonia.Markup.Xaml;

namespace PtzJoystickControl.Gui.Views
{
    public partial class CamerasView : UserControl
    {
        public CamerasView()
        {
            InitializeComponent();
        }

        private void InitializeComponent()
        {
            AvaloniaXamlLoader.Load(this);
        }
    }
}
