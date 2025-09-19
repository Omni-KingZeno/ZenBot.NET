using System;
using System.Buffers.Binary;
using PKHeX.Core;

namespace SysBot.Pokemon;

public sealed class TradePartnerSV(TradeMyStatus9 info)
{
    public string TID7 { get; } = info.TID7.ToString("D6");
    public string SID7 { get; } = info.SID7.ToString("D4");
    public string TrainerName { get; } = info.OT;
    public TradeMyStatus9 MyInfo { get; } = info;
}

public sealed class TradeMyStatus9 : ITradePartner
{
    public readonly byte[] Data = new byte[0x30];

    public uint SID7 => BinaryPrimitives.ReadUInt32LittleEndian(Data.AsSpan(0)) / 1_000_000;
    public uint TID7 => BinaryPrimitives.ReadUInt32LittleEndian(Data.AsSpan(0)) % 1_000_000;

    public int Game => Data[4];
    public int Gender => Data[5];
    public int Language => Data[6];

    public string OT => StringConverter8.GetString(Data.AsSpan(8, 24));
}
