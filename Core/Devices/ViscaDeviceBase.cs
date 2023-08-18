using PtzJoystickControl.Core.ViscaCommands;
using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace PtzJoystickControl.Core.Devices;

public abstract class ViscaDeviceBase : INotifyPropertyChanged
{
    protected string name = null!;
    protected int sendWaitTime = 30;
    protected byte[] sendBuffer = new byte[128];
    protected byte[] receiveBuffer = new byte[128];
    protected int sendBuffIndex = 0;
    protected int receiveBuffIndex = 0;
    protected DateTime lastSendTime = DateTime.UtcNow;
    protected DateTime lastReceiveTime = DateTime.MinValue;

    protected Power powerCmd;

    protected byte panSpeed = 0x04;
    protected byte tiltSpeed = 0x04;
    protected PanDir panDir = PanDir.Stop;
    protected TiltDir tiltDir = TiltDir.Stop;

    protected byte zoomCmd = 0x00;

    protected byte focusCmd;
    protected FocusMode focusModeCmd;
    protected FocusLock focusLockCmd;

    protected Preset presetCmd;
    protected byte presetCmdNumber;
    protected byte presetRecallSpeed = 0x04;

    protected ViscaCommandBuilder commandBuilder;

    public event PropertyChangedEventHandler? PropertyChanged;

    protected void NotifyPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public event PropertyChangedEventHandler? PersistentPropertyChanged;
    protected void NotifyPersistentPropertyChanged([CallerMemberName] string propertyName = "")
    {
        PersistentPropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }

    public ViscaDeviceBase(string name)
    {
        Name = name;
        commandBuilder = new ViscaCommandBuilder((byte)0x81);
    }
    
    protected internal Action<ViscaDeviceBase, byte[], int>? InquiryReplyParser { get; set; }
    protected internal bool Acked { get; internal set; }
    protected internal bool Completed { get; internal set; }

    public string Name
    {
        get { return name; }
        set
        {
            if (name == value)
                return;

            if(string.IsNullOrWhiteSpace(value))
                throw new ArgumentException($"Name cannot be empty", nameof(value));

                name = value;
                NotifyPropertyChanged();
                NotifyPersistentPropertyChanged();
            
        }
    }

    public byte ViscaAddress
    {
        get => commandBuilder.Address;
        set => commandBuilder.Address = 0 < value && value < 128 
            ? value
            : throw new ArgumentOutOfRangeException($"ViscaAddress must be between 1 and 127. {value} was given.");
    }

    protected DateTime LastSendTime
    {
        get => lastSendTime;
        set {
            var tempConnected = Connected;
            lastSendTime = value;
            if (tempConnected != Connected)
                NotifyPropertyChanged(nameof(Connected));
        }
    }

    protected internal DateTime LastReceiveTime {
        get => lastReceiveTime;
        set
        {
            var tempConnected = Connected;
            lastReceiveTime = value;
            if (tempConnected != Connected)
                NotifyPropertyChanged(nameof(Connected));
        }
    }

    public virtual bool Connected { 
        get {
            var t = lastSendTime - lastReceiveTime;
            return t <= TimeSpan.FromMilliseconds(500) && t >= TimeSpan.FromMilliseconds(-500); 
        } 
    }
    
    public Power? PowerState { get; protected internal set; }

    public abstract void Power(Power byteVal);
    public abstract void Pan(byte panSpeed, PanDir panDir);
    public abstract void Tilt(byte tiltSpeed, TiltDir tiltDir);
    public abstract void PanTilt(byte panSpeed, byte tiltSpeed, PanDir panDir, TiltDir tiltDir);
    public abstract void Zoom(byte zoomSpeed, ZoomDir zoomDir);
    public abstract void Focus(byte focusSpeed, FocusDir focusDir);
    public abstract void FocusMode(FocusMode focusMode);
    public abstract void FocusLock(FocusLock focusMode);
    public abstract void Preset(Preset preset, byte presetNumber);
    public abstract void PresetRecallSpeed(byte value);
}
