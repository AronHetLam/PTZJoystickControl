using System.Diagnostics;
using System.Net;
using System.Net.Sockets;

namespace PtzJoystickControl.Core.Utils;

public class UdpSocket
{
    private static readonly Dictionary<EndPoint, Action<byte[], int>> receiveCallbacks = new();
    private static Socket? socket;
    private static int port = 0;
    private readonly static EndPoint remoteReceiveFromEndPoint = new IPEndPoint(IPAddress.Any, 0);
    private readonly static EndPoint currentRemoteEndPoint = new IPEndPoint(IPAddress.Any, 0);

    public int Port
    {
        get => port;
        set => port = value;
    }

    private static byte[] _buffer = new byte[128];

    public static Socket GetInstance()
    {
        if (socket == null)
            socket = new Socket(AddressFamily.InterNetwork, SocketType.Dgram, ProtocolType.Udp)
            {
                ReceiveBufferSize = 128,
                SendBufferSize = 128
            };

        if (!socket.IsBound)
        {
            try
            {
                socket.Bind(new IPEndPoint(IPAddress.Any, 0));
                EndPoint remoteEndPoint = new IPEndPoint(IPAddress.Any, 0);
                socket.BeginReceiveFrom(_buffer, 0, _buffer.Length, SocketFlags.None, ref remoteEndPoint, new AsyncCallback(OnRecieve), null);
            }
            catch (Exception e)
            {
                Debug.WriteLine($"UDP GetInstance: {e.Message}");
            }
        }
        return socket;
    }

    private static void OnRecieve(IAsyncResult res)
    {
        try
        {
            EndPoint remoteEndPoint = currentRemoteEndPoint;
            var length = socket?.EndReceiveFrom(res, ref remoteEndPoint) ?? 0;

            Debug.WriteLine($"UDP OnReceive: {remoteEndPoint} - {BitConverter.ToString(_buffer, 0, length)}");
            if (length > 0 && receiveCallbacks.TryGetValue(remoteEndPoint, out var callback))
                callback(_buffer, length);

            remoteEndPoint = remoteReceiveFromEndPoint;
            socket?.BeginReceiveFrom(_buffer, 0, _buffer.Length, SocketFlags.None, ref remoteEndPoint, new AsyncCallback(OnRecieve), null);
        }
        catch (SocketException e)
        {
            Debug.WriteLine($"UDP OnReceive: {e.Message}");
            socket?.Dispose();
            socket = null;
        }
        catch (Exception e)
        {
            Debug.WriteLine($"UDP OnReceive: {e.Message}");
        }
    }

    /// <summary>
    /// Add callback to receive UDP packets from remote endpint
    /// </summary>
    /// <param name="remoteEndpint">Remote endpoint to receive packets from</param>
    /// <param name="callback">Callback wich recived buffer and content length as parameters</param>
    public static void AddListenerCallback(EndPoint remoteEndpint, Action<byte[], int> callback)
    {
        if (receiveCallbacks.ContainsKey(remoteEndpint))
            throw new ArgumentException("Only one callback is allowed for each remote endpoint.");
        receiveCallbacks.Add(remoteEndpint, callback);
    }

    public static void RemoveListenerCallback(EndPoint remoteEndpint)
    {
        receiveCallbacks.Remove(remoteEndpint);
    }
}
