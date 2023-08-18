using PtzJoystickControl.Core.Devices;
using PtzJoystickControl.Core.Utils;
using PtzJoystickControl.Core.ViscaCommands;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace PtzJoystickControl.Application.Devices;

public class ViscaIpDevice : ViscaIPDeviceBase
{
    private readonly object sendRecieveLock = new();
    private readonly HashSet<Func<ViscaIPHeaderType>> commandsToSend = new();
    private readonly List<Func<ViscaIPHeaderType>> commandsToRemoveFromSet = new();

    byte[] tmpBuffer = new byte[16];
    ushort tmpBuffIndex = 0;
    private uint packetId = 0;

    public ViscaIpDevice(string name) : this(name, null) { }

    public ViscaIpDevice(string name, IPEndPoint? viscaEndpint) : base(name, viscaEndpint) { }

    public override void BeginSocket()
    {
        if (socket == null)
        {
            if (protocol == Protocol.Udp)
            {
                socket = UdpSocket.GetInstance();
                UdpSocket.AddListenerCallback(ViscaIpEndpont, ParseViscaIPReply);
                return;
            }
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp)
            {
                SendTimeout = 500,
                NoDelay = true,
                SendBufferSize = sendBuffer.Length,
                ReceiveBufferSize = receiveBuffer.Length,
            };
        }

        if (protocol == Protocol.Tcp && !socket.Connected)
        {
            try
            {
                SocketAsyncEventArgs socketAsyncEvArgs = new SocketAsyncEventArgs() { RemoteEndPoint = ViscaIpEndpont };
                socketAsyncEvArgs.Completed += OnConnected;

                if (!socket.ConnectAsync(socketAsyncEvArgs))
                    OnConnected(null, socketAsyncEvArgs);
            }
            catch (Exception e)
            {
                Console.WriteLine($"TCP BeginSocket {name}: {e.Message}");
                EndSocket();
            }
        }
    }

    private void OnConnected(object? _, SocketAsyncEventArgs eventArgs)
    {
        NotifyPropertyChanged(nameof(Connected));
        try
        {
            if (eventArgs.SocketError == SocketError.Success)
                socket!.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback(OnRecieve), null);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"TCP OnConnected {name}: {eventArgs?.SocketError} {e.Message}");
            EndSocket();
        }
    }

    private void OnRecieve(IAsyncResult res)
    {
        IPEndPoint remoteIPEndPoint = new IPEndPoint(System.Net.IPAddress.Any, 0);
        EndPoint remoteEndPoint = remoteIPEndPoint;

        int length = 0;
        try
        {
            length = socket?.EndReceive(res, out var error) ?? 0;
            Debug.WriteLine($"TCP OnReceive: {ViscaIpEndpont} - {BitConverter.ToString(receiveBuffer, 0, length)}");
            ParseViscaIPReply(receiveBuffer, length);
            socket!.BeginReceive(receiveBuffer, 0, receiveBuffer.Length, SocketFlags.None, new AsyncCallback(OnRecieve), null);
        }
        catch (Exception e)
        {
            Debug.WriteLine($"TCP OnReceive {name}: {e.Message}");
            NotifyPropertyChanged(nameof(Connected));
            EndSocket();
        }
    }

    public override void EndSocket()
    {
        if (socket != null)
        {
            EventHandler<SocketAsyncEventArgs> onComplete = (sender, e) =>
            {
                NotifyPropertyChanged(nameof(Connected));
                if (socket != UdpSocket.GetInstance())
                    socket.Dispose();
                socket = null;
            };

            SocketAsyncEventArgs socketAsyncEvArgs = new SocketAsyncEventArgs();
            socketAsyncEvArgs.Completed += onComplete;

            //Manually invoke on complete if DisconnectAsync completed synchronusly.
            if (!socket.Connected || !socket.DisconnectAsync(socketAsyncEvArgs))
                onComplete.Invoke(null, null!);
        }
        UdpSocket.RemoveListenerCallback(ViscaIpEndpont);
    }

    public void ParseViscaIPReply(byte[] buffer, int length)
    {
        ViscaIPCommandParser.ParseReply(this, buffer, length);
    }

    public override void Power(Power power)
    {
        powerCmd = power;
        AddCommand(AddPowerCommand);
    }

    public override void Pan(byte panSpeed, PanDir panDir)
    {
        PanTilt(panSpeed, tiltSpeed, panDir, tiltDir);
    }

    public override void Tilt(byte tiltSpeed, TiltDir tiltDir)
    {
        PanTilt(panSpeed, tiltSpeed, panDir, tiltDir);
    }

    public override void PanTilt(byte panSpeed, byte tiltSpeed, PanDir panDir, TiltDir tiltDir)
    {
        bool panTiltChanged = false;
        if (this.panDir != panDir || this.tiltDir != tiltDir)
        {
            panTiltChanged = true;
        }
        else if ((this.panDir != PanDir.Stop || this.tiltDir != TiltDir.Stop) && (this.panSpeed != panSpeed || this.tiltSpeed != tiltSpeed))
        {
            panTiltChanged = true;
        }

        this.panDir = panDir;
        this.tiltDir = tiltDir;
        this.panSpeed = panSpeed;
        this.tiltSpeed = tiltSpeed;

        if (panTiltChanged)
        {
            AddCommand(AddPanTiltCommand);
        }
    }

    public override void Zoom(byte zoomSpeed, ZoomDir zoomDir)
    {
        //zoomCmd byte is split in direction and speed. 1 is dir and 2 is speed in 0x12
        bool zoomChanged = false;
        if ((zoomCmd & 0xf0) != (byte)zoomDir)
        { //Direction changed
            zoomChanged = true;
        }
        else if (zoomDir != (byte)ZoomDir.Stop && (zoomCmd & 0x0f) != zoomSpeed)
        { //Speed changed
            zoomChanged = true;
        }

        zoomCmd = (byte)(zoomDir == ZoomDir.Stop ? 0x00 : ((byte)zoomDir | zoomSpeed & 0x0f));

        if (zoomChanged)
        {
            AddCommand(AddZoomCommand);
        }
    }

    public override void Focus(byte focusSpeed, FocusDir focusDir)
    {
        bool focusChanged = false;
        if ((focusCmd & 0xf0) != (byte)focusDir)
        {
            focusChanged = true;
        }
        else if (focusDir != (byte)FocusDir.Stop && (focusCmd & 0x0f) != focusSpeed)
        {
            focusChanged = true;
        }

        focusCmd = focusDir == (byte)FocusDir.Stop ? (byte)0x00 : (byte)((byte)focusDir | focusSpeed & 0x0f);

        if (focusChanged)
        {
            AddCommand(AddFocusCommand);
        }
    }

    public override void FocusMode(FocusMode focusMode)
    {
        focusModeCmd = focusMode;
        AddCommand(AddFocusModeCommand);
    }

    public override void FocusLock(FocusLock focusLock)
    {
        focusLockCmd = focusLock;
        AddCommand(AddFocusLockCommand);
    }

    public override void Preset(Preset preset, byte presetNumber)
    {
        presetCmd = preset;
        presetCmdNumber = presetNumber;
        AddCommand(AddPresetCommand);
    }

    public override void PresetRecallSpeed(byte value)
    {
        presetRecallSpeed = value;
        AddCommand(AddPresetRecallSpeedCommand);
    }

    public ViscaIPHeaderType AddPowerCommand()
    {
        commandBuilder.BuildPowerCommand(ref tmpBuffer, ref tmpBuffIndex, powerCmd);
        return ViscaIPHeaderType.Command;
    }

    public ViscaIPHeaderType AddPowerInquiry()
    {
        commandBuilder.BuildPowerInquiry(ref tmpBuffer, ref tmpBuffIndex);
        InquiryReplyParser = ViscaCommandParser.ParsePowerInquiryReply;
        return ViscaIPHeaderType.Inquery;
    }

    private ViscaIPHeaderType AddPanTiltCommand()
    {
        commandBuilder.BuildPanTiltCommand(ref tmpBuffer, ref tmpBuffIndex, panSpeed, tiltSpeed, panDir, tiltDir);
        return ViscaIPHeaderType.Command;
    }

    private ViscaIPHeaderType AddZoomCommand()
    {
        commandBuilder.BuildZoomCommand(ref tmpBuffer, ref tmpBuffIndex, zoomCmd);
        return ViscaIPHeaderType.Command;
    }

    private ViscaIPHeaderType AddFocusCommand()
    {
        commandBuilder.BuildFocusCommand(ref tmpBuffer, ref tmpBuffIndex, focusCmd);
        return ViscaIPHeaderType.Command;
    }

    private ViscaIPHeaderType AddFocusModeCommand()
    {
        commandBuilder.BuildFocusModeCommand(ref tmpBuffer, ref tmpBuffIndex, focusModeCmd);
        return ViscaIPHeaderType.Command;
    }

    private ViscaIPHeaderType AddFocusLockCommand()
    {
        commandBuilder.BuildFocusLockCommand(ref tmpBuffer, ref tmpBuffIndex, focusLockCmd);
        return ViscaIPHeaderType.Command;
    }

    private ViscaIPHeaderType AddPresetCommand()
    {
        commandBuilder.BuildPresetCommand(ref tmpBuffer, ref tmpBuffIndex, presetCmd, presetCmdNumber);
        return ViscaIPHeaderType.Command;
    }

    private ViscaIPHeaderType AddPresetRecallSpeedCommand()
    {
        commandBuilder.BuildPresetRecallSpeedCommand(ref tmpBuffer, ref tmpBuffIndex, presetRecallSpeed);
        return ViscaIPHeaderType.Command;
    }

    public void AddHeader(ViscaIPHeaderType commandType, ushort payloadLength)
    {
        sendBuffer[sendBuffIndex++] = (byte)((ushort)commandType >> 8);
        sendBuffer[sendBuffIndex++] = (byte)commandType;
        sendBuffer[sendBuffIndex++] = (byte)(payloadLength >> 8);
        sendBuffer[sendBuffIndex++] = (byte)payloadLength;
        sendBuffer[sendBuffIndex++] = (byte)(packetId >> 24);
        sendBuffer[sendBuffIndex++] = (byte)(packetId >> 16);
        sendBuffer[sendBuffIndex++] = (byte)(packetId >> 8);
        sendBuffer[sendBuffIndex++] = (byte)packetId;
        packetId++;
    }

    private async void AddCommand(Func<ViscaIPHeaderType> commandFunction)
    {
        lock (commandsToSend)
        {
            commandsToSend.Add(commandFunction);
        }

        while (await Task.Run(() => trySendCommandsWithLock()))
        {
            lock (commandsToSend)
            {
                if (commandsToSend.Count == 0)
                {
                    break;
                }
            }
        };
    }

    private bool trySendCommandsWithLock()
    {
        bool hadLock = false;
        try
        {
            if (Monitor.TryEnter(sendRecieveLock))
            {
                if ((DateTime.UtcNow - lastSendTime).Milliseconds >= SendWaitTime)
                {
                    buildCommand();
                    SendCommand();
                    lastSendTime = DateTime.UtcNow;
                }
            }
        }
        finally
        {
            if (Monitor.IsEntered(sendRecieveLock))
            {
                Monitor.Exit(sendRecieveLock);
                hadLock = true;
            }
        }
        return hadLock;
    }

    private void buildCommand()
    {
        sendBuffIndex = 0;
        lock (commandsToSend)
        {
            foreach (var command in commandsToSend)
            {
                tmpBuffIndex = 0;
                var headerType = command();

                ushort cmdLen = tmpBuffIndex;
                if (useHeader)
                    cmdLen += 8;

                if (sendBuffIndex + cmdLen <= sendBuffer.Length)
                {
                    if (useHeader)
                        AddHeader(headerType, tmpBuffIndex);

                    for (int i = 0; i < tmpBuffIndex; i++)
                        sendBuffer[sendBuffIndex++] = tmpBuffer[i];

                    commandsToRemoveFromSet.Add(command);
                }
                else
                    break;

                if (singleCommand) 
                    break;
            }
            foreach (var commandToRemove in commandsToRemoveFromSet)
            {
                commandsToSend.Remove(commandToRemove);
            }
        }
        commandsToRemoveFromSet.Clear();
    }

    public void SendCommand()
    {
        if (socket == null)
        {
            Debug.WriteLine("No socket: " + ViscaIpEndpont);
            BeginSocket();
            return;
        }


        if (protocol == Protocol.Tcp && !socket.Connected)
        {
            Debug.WriteLine("Reconnecting to client: " + ViscaIpEndpont);
            BeginSocket();
            return;
        }

        try
        {
            socket.SendTo(sendBuffer, sendBuffIndex, SocketFlags.None, ViscaIpEndpont);
        }
        catch (Exception e)
        {
            Debug.WriteLine(e);
            EndSocket();
        }
    }

}
