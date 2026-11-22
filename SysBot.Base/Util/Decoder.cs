using System.Buffers;

namespace SysBot.Base;

/// <summary>
/// Decodes a sys-botbase protocol message into raw data.
/// </summary>
public static class Decoder
{
    public static byte[] ConvertHexByteStringToBytes(ReadOnlySpan<byte> bytes) => Convert.FromHexString(bytes);

    public static void LoadHexBytesTo(ReadOnlySpan<byte> str, Span<byte> dest)
    {
        var status = Convert.FromHexString(str, dest, out _, out _);
        if (status != OperationStatus.Done)
            throw new FormatException($"Invalid hex response from the console: {status}.");
    }

    public static byte[] ConvertPeekToBytes(ReadOnlySpan<byte> bytes) =>
        Convert.FromHexString(bytes.TrimEnd((byte)'\n'));
}
