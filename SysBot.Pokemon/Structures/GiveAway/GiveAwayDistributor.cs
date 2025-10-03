using PKHeX.Core;

namespace SysBot.Pokemon;

public class GiveAwayDistributor<T> where T : PKM, new()
{
    public readonly Dictionary<string, GiveAwayRequest<T>> GiveAway;
    public readonly PokemonGAPool<T> Pool;

    public GiveAwayDistributor(PokemonGAPool<T> GApool)
    {
        Pool = GApool;
        GiveAway = Pool.Files;
    }
}