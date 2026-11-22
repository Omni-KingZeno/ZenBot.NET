using System.Net.Sockets;

namespace SysBot.Base;

/// <summary>
/// Abstract class representing the communication over a Wi-Fi socket.
/// </summary>
public abstract class SwitchSocket : IConsoleConnection
{
    protected Socket Connection { get; private set; }
    protected readonly IWirelessConnectionConfig Info;
    private readonly ProtocolType Protocol;
    private readonly SocketType Type;

    public string Name { get; }
    public string Label { get; set; }
    public bool Connected => Connection.Connected;

    public int MaximumTransferSize { get; set; } = 0x1C0;
    public int BaseDelay { get; set; } = 64;
    public int DelayFactor { get; set; } = 256;

    protected SwitchSocket(IWirelessConnectionConfig wi, SocketType type = SocketType.Stream, ProtocolType protocol = ProtocolType.Tcp)
    {
        Type = type;
        Protocol = protocol;
        Connection = CreateSocket();
        Info = wi;
        Name = Label = wi.IP;
    }

    public void Log(string message) => LogInfo(message);
    public void LogInfo(string message) => LogUtil.LogInfo(message, Label);
    public void LogError(string message) => LogUtil.LogError(message, Label);

    public abstract void Connect();
    public abstract void Reset();
    public abstract void Disconnect();
    public void InitializeSocket()
    {
        try { Connection?.Dispose(); } catch { /* ignore */ }
        Connection = CreateSocket();
    }

    private Socket CreateSocket()
    {
        var socket = new Socket(Type, Protocol);
        if (Type != SocketType.Stream || Protocol != ProtocolType.Tcp)
            return socket;
        try
        {
            // Detect dead peers/NAT drops instead of finding out on the next read.
            socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveTime, 30);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveInterval, 10);
            socket.SetSocketOption(SocketOptionLevel.Tcp, SocketOptionName.TcpKeepAliveRetryCount, 3);
        }
        catch (Exception ex) when (ex is SocketException or PlatformNotSupportedException or ArgumentException)
        {
            // Keepalive tuning isn't supported everywhere; the defaults still work.
        }
        return socket;
    }
}
