using Avalonia.Data.Converters;
using PtzJoystickControl.Gui.ViewModels;
using System;
using System.Globalization;

namespace PtzJoystickControl.Gui.Views
{
    internal class CameraViewToCameraConverter : IValueConverter
    {
        private static readonly CameraToCameraViewConverter Instance = new();

        public CameraViewToCameraConverter()
        {
        }

        public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            if (value is IPCameraViewModel ipCameraViewModel)
                return ipCameraViewModel.Camera;

            return null;
        }

        public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
