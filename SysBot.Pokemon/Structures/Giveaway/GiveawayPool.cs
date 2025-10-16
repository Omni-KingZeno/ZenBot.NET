using PKHeX.Core;

namespace SysBot.Pokemon;

public class GiveawayPool<T> : PokemonPoolBase<T, GiveawayRequest<T>> where T : PKM, new()
{
    protected override string PoolName => nameof(GiveawayPool<T>);

    protected override GiveawayRequest<T> CreateRequest(T pkm, string fn) =>
        new(pkm, fn);
}
