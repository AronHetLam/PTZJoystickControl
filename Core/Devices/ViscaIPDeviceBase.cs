using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;

namespace PtzJoystickControl.Core.Devices;

public abstract class ViscaIPDeviceBase : ViscaDeviceBase
{
    protected Socket? socket;
    protected Protocol protocol = Protocol.Udp;
    protected bool useHeader = true;
    protected bool singleCommand = true; //TODO: test if allowed or not...
    protected IPEndPoint ViscaIpEndpont;

    public ViscaIPDeviceBase(string name) : this(name, null) { }

    public ViscaIPDeviceBase(string name, IPEndPoint? viscaEndpint) : base(name)
    {
        Name = name;
        ViscaIpEndpont = viscaEndpint ?? new IPEndPoint(System.Net.IPAddress.Any, 5678);
    }

    public int remotePacketId { get; set; }

    public bool UseHeader
    {
        get => useHeader;
        set
        {
            useHeader = value;
            NotifyPropertyChanged();
            NotifyPersistentPropertyChanged();
        }
    }

    public bool SingleCommand
    {
        get => singleCommand;
        set
        {
            singleCommand = value;
            NotifyPropertyChanged();
            NotifyPersistentPropertyChanged();
        }
    }

    public Protocol Protocol
    {
        get => protocol;
        set
        {
            if (protocol != value)
            {
                protocol = value;
                OnSocketPropChange();
            }
        }
    }

    public int SendWaitTime
    {
        get => sendWaitTime;
        set => sendWaitTime = value;
    }

    public string IPAddress
    {
        get => ViscaIpEndpont.Address.ToString();
        set
        {
            if (System.Net.IPAddress.TryParse(value, out var ipAddress))
            {

                if (!ViscaIpEndpont.Address.Equals(ipAddress))
                {
                    ViscaIpEndpont.Address = ipAddress;
                    OnSocketPropChange();
                }
            }
            else
                throw new ArgumentException("Cannot parse IPAddress", nameof(value));
        }
    }

    public int Port
    {
        get => ViscaIpEndpont.Port;
        set
        {
            if (ViscaIpEndpont.Port != value)
            {
                ViscaIpEndpont.Port = value;
                OnSocketPropChange();
            }
        }
    }

    private void OnSocketPropChange([CallerMemberName] string propertyName = "")
    {
        EndSocket();
        BeginSocket();
        NotifyPropertyChanged(propertyName);
        NotifyPersistentPropertyChanged(propertyName);
    }

    public override bool Connected { get { return socket != null && (socket.Connected || base.Connected); } }

    public abstract void BeginSocket();

    public abstract void EndSocket();
}
