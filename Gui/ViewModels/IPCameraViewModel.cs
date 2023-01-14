using Avalonia.Data;
using Avalonia.Utilities;
using PtzJoystickControl.Core.Devices;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Diagnostics;
using System.Runtime.CompilerServices;

namespace PtzJoystickControl.Gui.ViewModels;

public class IPCameraViewModel : ViewModelBase, INotifyPropertyChanged
{
    private readonly ViscaIPDeviceBase _camera;

    public static IEnumerable<Protocol> Protocols { get; } = Enum.GetValues<Protocol>();

    public IPCameraViewModel(ViscaDeviceBase camera)
    {
        _camera = (ViscaIPDeviceBase)camera;
        WeakEventHandlerManager.Subscribe<INotifyPropertyChanged, PropertyChangedEventArgs, IPCameraViewModel>(camera, nameof(camera.PropertyChanged), OnCameraPropertyCahnged);
    }

    private void OnCameraPropertyCahnged(object? sender, PropertyChangedEventArgs e)
    {
        NotifyPropertyChanged(e?.PropertyName ?? "");
    }

    public ViscaDeviceBase Camera { get => _camera; }

    public string Name
    {
        get => _camera.Name;
        set => _camera.Name = value;
    }


    public bool UseHeader
    {
        get => _camera.UseHeader;
        set => _camera.UseHeader = value;
    }

    public bool SingleCommand
    {
        get => _camera.SingleCommand;
        set => _camera.SingleCommand = value;
    }

    public Protocol Protocol
    {
        get => _camera.Protocol;
        set => _camera.Protocol = value;
    }

    public int SendWaitTime
    {
        get => _camera.SendWaitTime;
        set
        {
            try
            {
                _camera.SendWaitTime = value;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw new DataValidationException(e.Message);
            }
        }
    }

    public string IPAddress
    {
        get => _camera.IPAddress;
        set
        {
            try
            {
                _camera.IPAddress = value;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw new DataValidationException("Invalid IP");
            }
        }
    }

    public int Port
    {
        get => _camera.Port;
        set
        {
            try
            {
                _camera.Port = value;
            }
            catch (Exception e)
            {
                Debug.WriteLine(e);
                throw new DataValidationException("1-65535");
            }
        }
    }

    public bool Connected { get => _camera.Connected; }

    public new event PropertyChangedEventHandler? PropertyChanged;
    private void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public override string ToString()
    {
        return Name;
    }
}
