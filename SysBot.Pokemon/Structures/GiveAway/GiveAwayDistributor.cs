using PKHeX.Core;

namespace SysBot.Pokemon;

public class GiveawayDistributor<T> where T : PKM, new()
{
    public readonly Dictionary<string, GiveawayRequest<T>> Giveaway;
    public readonly GiveawayPool<T> Pool;

    public GiveawayDistributor(GiveawayPool<T> GApool)
    {
        Pool = GApool;
        Giveaway = Pool.Files;
    }
}