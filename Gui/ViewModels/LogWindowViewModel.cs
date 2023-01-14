using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PtzJoystickControl.Gui.ViewModels
{
    internal class LogWindowViewModel : ViewModelBase, INotifyPropertyChanged
    {
        private string _log = string.Empty;
        private readonly Listener _listenser;

        internal LogWindowViewModel()
        {
            _listenser = new Listener(this);
            Trace.Listeners.Add(_listenser);
        }
        
        ~LogWindowViewModel()
        {
            Trace.Listeners.Remove(_listenser);
        }

        public string Log
        {
            get => _log;
            set
            {
                _log = value;
                NotifyPropertyChanged();
            }
        }

        public void ClearLog()
        {
            Log = string.Empty;
        }

        private class Listener : TraceListener
        {
            private readonly LogWindowViewModel logWindowViewModel;

            internal Listener(LogWindowViewModel logWindowViewModel)
            {
                this.logWindowViewModel = logWindowViewModel;
            }
            public override void Write(string? message)
            {
                logWindowViewModel.Log += message;
            }

            public override void WriteLine(string? message)
            {
                logWindowViewModel.Log += $"{message}\n";

            }
        }

        public new event PropertyChangedEventHandler? PropertyChanged;
        private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
        }
    }
}
