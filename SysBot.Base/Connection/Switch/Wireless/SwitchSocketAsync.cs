using System.Buffers;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using static System.Buffers.Binary.BinaryPrimitives;
using static SysBot.Base.SwitchOffsetTypeUtil;

namespace SysBot.Base;

/// <summary>
/// Connection to a Nintendo Switch hosting the sys-module via a socket (Wi-Fi).
/// </summary>
/// <remarks>
/// Interactions are performed asynchronously.
/// </remarks>
public sealed class SwitchSocketAsync : SwitchSocket, ISwitchConnectionAsync
{
    private SwitchSocketAsync(IWirelessConnectionConfig cfg) : base(cfg) { }

    public static SwitchSocketAsync CreateInstance(IWirelessConnectionConfig cfg)
    {
        return new SwitchSocketAsync(cfg);
    }

    public override void Connect()
    {
        if (Connected)
        {
            Log("Already connected prior, skipping initial connection.");
            return;
        }

        Log("Connecting to device...");
        var result = Connection.BeginConnect(Info.IP, Info.Port, null, null);
        var success = result.AsyncWaitHandle.WaitOne(5000, true);
        if (!success || !Connection.Connected)
        {
            InitializeSocket();
            throw new Exception("Failed to connect to device.");
        }
        Connection.EndConnect(result);
        Log("Connected!");
        Label = Name;
    }

    public override void Reset()
    {
        try { if (Connection.Connected) Connection.Shutdown(SocketShutdown.Both); } catch { /* already dead */ }
        InitializeSocket();
        Connect();
    }

    public override void Disconnect()
    {
        Log("Disconnecting from device...");
        var result = Connection.BeginDisconnect(false, null, null);
        var success = result.AsyncWaitHandle.WaitOne(5000, true);
        if (!success || Connection.Connected)
        {
            InitializeSocket();
            throw new Exception("Failed to disconnect from device.");
        }
        Connection.EndDisconnect(result);
        Log("Disconnected! Resetting Socket.");
        InitializeSocket();
    }

    /// <summary> Only call this if you are sending small commands. </summary>
    public ValueTask<int> SendAsync(ReadOnlyMemory<byte> buffer, CancellationToken token)
        => Connection.SendAsync(buffer, token);

    private const int ReceiveTimeoutMs = 10_000;

    private async Task<byte[]> ReadBytesFromCmdAsync(ReadOnlyMemory<byte> cmd, int length, CancellationToken token)
    {
        try
        {
            await SendAsync(cmd, token).ConfigureAwait(false);
            var size = (length * 2) + 1;
            var buffer = ArrayPool<byte>.Shared.Rent(size);
            try
            {
                var mem = buffer.AsMemory()[..size];
                await ReceiveExactAsync(mem, token).ConfigureAwait(false);
                return DecodeResult(mem, length);
            }
            finally
            {
                ArrayPool<byte>.Shared.Return(buffer, true);
            }
        }
        catch (OperationCanceledException)
        {
            throw;
        }
        catch (SocketException ex)
        {
            Log($"{nameof(ReadBytesFromCmdAsync)} failed: {ex.Message}");
            throw;
        }
        catch (ObjectDisposedException ex)
        {
            Log($"{nameof(ReadBytesFromCmdAsync)} failed: {ex.Message}");
            throw new SocketException((int)SocketError.NotConnected);
        }
    }

    /// <summary> Reads until the buffer is full. A zero-byte read means the peer closed the connection. </summary>
    private async Task ReceiveExactAsync(Memory<byte> buffer, CancellationToken token)
    {
        using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
        cts.CancelAfter(ReceiveTimeoutMs);
        try
        {
            var total = 0;
            while (total < buffer.Length)
            {
                var read = await Connection.ReceiveAsync(buffer[total..], SocketFlags.None, cts.Token).ConfigureAwait(false);
                if (read == 0)
                    throw new SocketException((int)SocketError.ConnectionReset);
                total += read;
            }
        }
        catch (OperationCanceledException) when (!token.IsCancellationRequested)
        {
            throw new SocketException((int)SocketError.TimedOut);
        }
    }

    private static byte[] DecodeResult(ReadOnlyMemory<byte> buffer, int length)
    {
        try
        {
            var result = new byte[length];
            if (buffer.Length < 1)
                return [];
            var span = buffer.Span[..^1]; // Last byte is always a terminator
            Decoder.LoadHexBytesTo(span, result);
            return result;
        }
        catch (Exception)
        {
            return [];
        }
    }

    public Task<byte[]> ReadBytesAsync(uint offset, int length, CancellationToken token) => Read(Heap, offset, length, token);
    public Task<byte[]> ReadBytesMainAsync(ulong offset, int length, CancellationToken token) => Read(Main, offset, length, token);
    public Task<byte[]> ReadBytesAbsoluteAsync(ulong offset, int length, CancellationToken token) => Read(Absolute, offset, length, token);

    public Task<byte[]> ReadBytesMultiAsync(IReadOnlyDictionary<ulong, int> offsetSizes, CancellationToken token) => ReadMulti(Heap, offsetSizes, token);
    public Task<byte[]> ReadBytesMainMultiAsync(IReadOnlyDictionary<ulong, int> offsetSizes, CancellationToken token) => ReadMulti(Main, offsetSizes, token);
    public Task<byte[]> ReadBytesAbsoluteMultiAsync(IReadOnlyDictionary<ulong, int> offsetSizes, CancellationToken token) => ReadMulti(Absolute, offsetSizes, token);

    public Task WriteBytesAsync(ReadOnlyMemory<byte> data, uint offset, CancellationToken token) => Write(Heap, data, offset, token);
    public Task WriteBytesMainAsync(ReadOnlyMemory<byte> data, ulong offset, CancellationToken token) => Write(Main, data, offset, token);
    public Task WriteBytesAbsoluteAsync(ReadOnlyMemory<byte> data, ulong offset, CancellationToken token) => Write(Absolute, data, offset, token);

    public async Task<ulong> GetMainNsoBaseAsync(CancellationToken token)
    {
        try
        {
            byte[] baseBytes = await ReadBytesFromCmdAsync(SwitchCommand.GetMainNsoBase(), sizeof(ulong), token).ConfigureAwait(false);
            if (baseBytes.Length < sizeof(ulong))
            {
                Log($"{nameof(GetMainNsoBaseAsync)}: Invalid response length");
                return 0;
            }
            return ReadUInt64BigEndian(baseBytes);
        }
        catch (Exception ex) when (ex is not (SocketException or OperationCanceledException))
        {
            Log($"{nameof(GetMainNsoBaseAsync)} failed: {ex.Message}");
            return 0;
        }
    }

    public async Task<ulong> GetHeapBaseAsync(CancellationToken token)
    {
        try
        {
            var baseBytes = await ReadBytesFromCmdAsync(SwitchCommand.GetHeapBase(), sizeof(ulong), token).ConfigureAwait(false);
            if (baseBytes.Length < sizeof(ulong))
            {
                Log($"{nameof(GetHeapBaseAsync)}: Invalid response length");
                return 0;
            }
            return ReadUInt64BigEndian(baseBytes);
        }
        catch (Exception ex) when (ex is not (SocketException or OperationCanceledException))
        {
            Log($"{nameof(GetHeapBaseAsync)} failed: {ex.Message}");
            return 0;
        }
    }

    public async Task<string> GetTitleID(CancellationToken token)
    {
        try
        {
            var bytes = await ReadRaw(SwitchCommand.GetTitleID(), 17, token).ConfigureAwait(false);
            if (bytes.Length == 0)
            {
                Log($"{nameof(GetTitleID)}: Invalid response");
                return string.Empty;
            }
            return Encoding.ASCII.GetString(bytes).Trim();
        }
        catch (Exception ex) when (ex is not (SocketException or OperationCanceledException))
        {
            Log($"{nameof(GetTitleID)} failed: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<string> GetBotbaseVersion(CancellationToken token)
    {
        try
        {
            // Allows up to 9 characters for version, and trims extra '\0' if unused.
            var bytes = await ReadRaw(SwitchCommand.GetBotbaseVersion(), 10, token).ConfigureAwait(false);
            if (bytes.Length == 0)
            {
                Log($"{nameof(GetBotbaseVersion)}: Invalid response");
                return string.Empty;
            }
            return Encoding.ASCII.GetString(bytes).Trim('\0');
        }
        catch (Exception ex) when (ex is not (SocketException or OperationCanceledException))
        {
            Log($"{nameof(GetBotbaseVersion)} failed: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<string> GetGameInfo(string info, CancellationToken token)
    {
        try
        {
            var bytes = await ReadRaw(SwitchCommand.GetGameInfo(info), 17, token).ConfigureAwait(false);
            if (bytes.Length == 0)
            {
                Log($"{nameof(GetGameInfo)}: Invalid response");
                return string.Empty;
            }
            return Encoding.ASCII.GetString(bytes).Trim('\0', '\n');
        }
        catch (Exception ex) when (ex is not (SocketException or OperationCanceledException))
        {
            Log($"{nameof(GetGameInfo)} failed: {ex.Message}");
            return string.Empty;
        }
    }

    public async Task<bool> IsProgramRunning(ulong pid, CancellationToken token)
    {
        try
        {
            var bytes = await ReadRaw(SwitchCommand.IsProgramRunning(pid), 17, token).ConfigureAwait(false);
            if (bytes.Length == 0)
            {
                Log($"{nameof(IsProgramRunning)}: Invalid response");
                return false;
            }
            return ulong.TryParse(Encoding.ASCII.GetString(bytes).Trim(), out var value) && value == 1;
        }
        catch (Exception ex) when (ex is not (SocketException or OperationCanceledException))
        {
            Log($"{nameof(IsProgramRunning)} failed: {ex.Message}");
            return false;
        }
    }

    private async Task<byte[]> Read(ICommandBuilder b, ulong offset, int length, CancellationToken token)
    {
        if (length <= MaximumTransferSize)
        {
            var cmd = b.Peek(offset, length);
            return await ReadBytesFromCmdAsync(cmd, length, token).ConfigureAwait(false);
        }

        byte[] result = new byte[length];
        for (int i = 0; i < length; i += MaximumTransferSize)
        {
            int len = MaximumTransferSize;
            int delta = length - i;
            if (delta < MaximumTransferSize)
                len = delta;

            var cmd = b.Peek(offset + (uint)i, len);
            var bytes = await ReadBytesFromCmdAsync(cmd, len, token).ConfigureAwait(false);
            bytes.CopyTo(result, i);
            await Task.Delay((MaximumTransferSize / DelayFactor) + BaseDelay, token).ConfigureAwait(false);
        }
        return result;
    }

    private Task<byte[]> ReadMulti(ICommandBuilder b, IReadOnlyDictionary<ulong, int> offsetSizes, CancellationToken token)
    {
        var totalSize = offsetSizes.Values.Sum();
        var cmd = b.PeekMulti(offsetSizes);
        return ReadBytesFromCmdAsync(cmd, totalSize, token);
    }

    private async Task Write(ICommandBuilder b, ReadOnlyMemory<byte> data, ulong offset, CancellationToken token)
    {
        if (data.Length <= MaximumTransferSize)
        {
            var cmd = b.Poke(offset, data.Span);
            await SendAsync(cmd, token).ConfigureAwait(false);
            return;
        }
        int byteCount = data.Length;
        for (int i = 0; i < byteCount; i += MaximumTransferSize)
        {
            var length = byteCount - i;
            if (length > MaximumTransferSize)
                length = MaximumTransferSize;
            var cmd = GetPoke(b, data.Span, offset, i, length);
            await SendAsync(cmd, token).ConfigureAwait(false);
            await Task.Delay((MaximumTransferSize / DelayFactor) + BaseDelay, token).ConfigureAwait(false);
        }
    }

    private static byte[] GetPoke(ICommandBuilder b, ReadOnlySpan<byte> data, ulong offset, int i, int length)
    {
        var slice = data.Slice(i, length);
        return b.Poke(offset + (uint)i, slice);
    }

    public async Task<byte[]> ReadRaw(ReadOnlyMemory<byte> command, int length, CancellationToken token)
    {
        try
        {
            await SendAsync(command, token).ConfigureAwait(false);
            var buffer = new byte[length];
            await Connection.ReceiveAsync(buffer, token).ConfigureAwait(false);
            return buffer;
        }
        catch (Exception ex) when (ex is not (SocketException or OperationCanceledException))
        {
            Log($"{nameof(ReadRaw)} failed: {ex.Message}");
            return [];
        }
    }

    public async Task SendRaw(ReadOnlyMemory<byte> command, CancellationToken token)
    {
        try
        {
            await SendAsync(command, token).ConfigureAwait(false);
        }
        catch (Exception ex) when (ex is not (SocketException or OperationCanceledException))
        {
            Log($"{nameof(SendRaw)} failed: {ex.Message}");
        }
    }

    public Task<byte[]> PointerPeek(int size, IEnumerable<long> jumps, CancellationToken token)
    {
        return ReadBytesFromCmdAsync(SwitchCommand.PointerPeek(jumps, size), size, token);
    }

    public async Task PointerPoke(ReadOnlyMemory<byte> data, IEnumerable<long> jumps, CancellationToken token)
    {
        await SendAsync(SwitchCommand.PointerPoke(jumps, data.Span), token).ConfigureAwait(false);
    }

    public async Task<ulong> PointerAll(IEnumerable<long> jumps, CancellationToken token)
    {
        try
        {
            var offsetBytes = await ReadBytesFromCmdAsync(SwitchCommand.PointerAll(jumps), sizeof(ulong), token).ConfigureAwait(false);
            if (offsetBytes.Length < sizeof(ulong))
            {
                Log($"{nameof(PointerAll)}: Invalid response length {offsetBytes.Length}");
                return 0;
            }
            // Sent visually (big-endian, thanks); interpret as such.
            return ReadUInt64BigEndian(offsetBytes);
        }
        catch (Exception ex) when (ex is not (SocketException or OperationCanceledException))
        {
            Log($"{nameof(PointerAll)} failed: {ex.Message}");
            return 0;
        }
    }

    public async Task<ulong> PointerRelative(IEnumerable<long> jumps, CancellationToken token)
    {
        try
        {
            var offsetBytes = await ReadBytesFromCmdAsync(SwitchCommand.PointerRelative(jumps), sizeof(ulong), token).ConfigureAwait(false);
            if (offsetBytes.Length < sizeof(ulong))
            {
                Log($"{nameof(PointerRelative)}: Invalid response length {offsetBytes.Length}");
                return 0;
            }
            // Sent visually (big-endian, thanks); interpret as such.
            return ReadUInt64BigEndian(offsetBytes);
        }
        catch (Exception ex) when (ex is not (SocketException or OperationCanceledException))
        {
            Log($"{nameof(PointerRelative)} failed: {ex.Message}");
            return 0;
        }
    }

    private async Task<(bool, T)> TryReadInternal<T>(Func<ulong, int, CancellationToken, Task<byte[]>> readMethod, ulong offset, CancellationToken token) where T : unmanaged
    {
        try
        {
            int size = Marshal.SizeOf<T>();
            var data = await readMethod(offset, size, token).ConfigureAwait(false);
            if (data.Length < size)
                return (false, default);

            if (!BitConverter.IsLittleEndian)
                Array.Reverse(data, 0, size);

            var value = MemoryMarshal.Read<T>(data);
            return (true, value);
        }
        catch (Exception ex) when (ex is not (SocketException or OperationCanceledException))
        {
            Log($"{nameof(TryReadInternal)}<{typeof(T).Name}> failed: {ex.Message}");
            return (false, default);
        }
    }

    public Task<(bool Success, T Value)> TryReadMain<T>(ulong offset, CancellationToken token) where T : unmanaged
        => TryReadInternal<T>(ReadBytesMainAsync, offset, token);

    public Task<(bool Success, T Value)> TryReadAbsolute<T>(ulong offset, CancellationToken token) where T : unmanaged
        => TryReadInternal<T>(ReadBytesAbsoluteAsync, offset, token);

    public async Task<byte[]> PixelPeek(CancellationToken token)
    {
        await SendAsync(SwitchCommand.PixelPeek(), token).ConfigureAwait(false);

        var data = await FlexRead(token).ConfigureAwait(false);
        var result = Array.Empty<byte>();
        try
        {
            result = Decoder.ConvertPeekToBytes(data);
        }
        catch (Exception e)
        {
            LogError($"Malformed screenshot data received:\n{e.Message}");
        }

        return result;
    }

    private const int FlexReadIdleTimeoutMs = 5_000;

    private async Task<byte[]> FlexRead(CancellationToken token)
    {
        using var ms = new MemoryStream();
        var buffer = ArrayPool<byte>.Shared.Rent(64 * 1024);
        try
        {
            while (true)
            {
                int read;
                using var cts = CancellationTokenSource.CreateLinkedTokenSource(token);
                cts.CancelAfter(FlexReadIdleTimeoutMs);
                try
                {
                    read = await Connection.ReceiveAsync(buffer.AsMemory(), SocketFlags.None, cts.Token).ConfigureAwait(false);
                }
                catch (OperationCanceledException) when (!token.IsCancellationRequested)
                {
                    throw new SocketException((int)SocketError.TimedOut);
                }

                if (read == 0)
                    throw new SocketException((int)SocketError.ConnectionReset);

                ms.Write(buffer, 0, read);
                if (buffer[read - 1] == (byte)'\n')
                    return ms.ToArray();
            }
        }
        finally
        {
            ArrayPool<byte>.Shared.Return(buffer);
        }
    }
}
