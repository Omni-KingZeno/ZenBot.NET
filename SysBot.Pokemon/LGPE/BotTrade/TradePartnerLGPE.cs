using PKHeX.Core;

namespace SysBot.Pokemon;

public sealed class TradePartnerLGPE(SAV7b sav) : ITradePartner
{
    public uint TID7        => sav.TrainerTID7;
    public uint SID7        => sav.DisplaySID;
    public string OT        => sav.OT;
    public int Game         => (int)sav.Version;
    public int Gender       => sav.Gender;
    public int Language     => sav.Language;
    public string SyncID    => sav.Status.GameSyncID;
}
