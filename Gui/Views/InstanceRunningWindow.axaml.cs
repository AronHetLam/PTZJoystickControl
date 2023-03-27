using Avalonia.Controls;
using PtzJoystickControl.Gui.ViewModels;

namespace PtzJoystickControl.Gui.Views
{
    public partial class InstanceRunningWindow : Window
    {
        public InstanceRunningWindow()
        {
            InitializeComponent();

            DataContext = this;

            //var logWin = new LogWindow();
            //logWin.DataContext = new LogWindowViewModel();
            //logWin.Show();
        }
    }
}
