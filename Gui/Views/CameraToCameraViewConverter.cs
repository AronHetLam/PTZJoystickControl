using Avalonia.Data.Converters;
using PtzJoystickControl.Gui.ViewModels;
using PtzJoystickControl.Core.Devices;
using System;
using System.Globalization;

namespace PtzJoystickControl.Gui.Views;

public class CameraToCameraViewConverter : IValueConverter
{

    private static readonly CameraToCameraViewConverter Instance = new();

    public CameraToCameraViewConverter()
    {
    }

    public object? Convert(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        if (value == null)
            return null;

        var type = value.GetType();
        if (type.IsAssignableTo(typeof(ViscaIPDeviceBase)))
            return new IPCameraViewModel((ViscaIPDeviceBase)value);

        return null;
    }

    public object? ConvertBack(object? value, Type targetType, object? parameter, CultureInfo culture)
    {
        throw new NotImplementedException();
    }
}
