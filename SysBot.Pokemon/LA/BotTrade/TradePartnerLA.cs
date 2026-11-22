using System.Diagnostics;
using PKHeX.Core;
using static System.Buffers.Binary.BinaryPrimitives;

namespace SysBot.Pokemon;

public sealed class TradePartnerLA : ITradePartner
{
    public uint TID7 { get; }
    public uint SID7 { get; }
    public string OT { get; }
    public int Game { get; }
    public int Gender { get; }
    public int Language { get; }

    public TradePartnerLA(byte[] TIDSID, byte[] trainerNameObject, byte[] info)
    {
        Debug.Assert(TIDSID.Length == 4);
        TID7 = ReadUInt32LittleEndian(TIDSID.AsSpan()) % 1_000_000;
        SID7 = ReadUInt32LittleEndian(TIDSID.AsSpan()) / 1_000_000;
        OT = StringConverter8.GetString(trainerNameObject);
        Game = info[0];
        Gender = info[1];
        Language = info[3];
    }

    public const int MaxByteLengthStringObject = 26;
}
