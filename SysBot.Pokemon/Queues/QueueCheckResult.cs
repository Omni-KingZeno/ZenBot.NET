using PKHeX.Core;

namespace SysBot.Pokemon;

/// <summary>
/// Stores data for indicating how a queue position/presence check resulted.
/// </summary>
/// <typeparam name="T"></typeparam>
public sealed record QueueCheckResult<T>(
    bool InQueue = false,
    TradeEntry<T>? Detail = null,
    int Position = -1,
    int QueueCount = -1)
    where T : PKM, new()
{
    public static readonly QueueCheckResult<T> None = new();

    public string GetMessage()
    {
        if (!InQueue || Detail is null)
            return "You are not in the queue.";
        var position = $"{Position}/{QueueCount}";
        var msg = $"You are in the {Detail.Type} queue! Position: {position} (ID {Detail.Trade.ID})";
        var pk = Detail.Trade.TradeData;
        var strings = GameInfo.GetStrings("en");
        var receiving = Detail.Trade.Type switch
        {
            PokeTradeType.MysteryEgg => "Mystery Egg",
            PokeTradeType.ItemTrade => strings.itemlist[pk.HeldItem],
            _ => strings.Species[pk.Species]
        };
        if (pk.Species != 0)
            msg += $", Receiving: {receiving}";
        return msg;
    }
}
