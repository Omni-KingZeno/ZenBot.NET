using PKHeX.Core;

namespace SysBot.Pokemon;

public class PokemonPool<T>(BaseConfig settings) : PokemonPoolBase<T, LedyRequest<T>> where T : PKM, new()
{
    protected override string PoolName => nameof(PokemonPool<T>);
    protected override bool Randomized => settings.Shuffled;

    protected override bool ShouldAdd(T pk, LegalityAnalysis la)
    {
        if (settings.Legality.ResetHOMETracker && pk is IHomeTrack h)
            h.Tracker = 0;
        return true;
    }

    protected override LedyRequest<T> CreateRequest(T pkm, string fn) =>
        new(pkm, fn);
}
