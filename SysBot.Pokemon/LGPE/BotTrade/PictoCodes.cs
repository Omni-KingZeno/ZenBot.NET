using PKHeX.Core;

namespace SysBot.Pokemon;

public enum PictoCode : int
{
    Pikachu     = 0,
    Eevee       = 1,
    Bulbasaur   = 2,
    Charmander  = 3,
    Squirtle    = 4,
    Pidgey      = 5,
    Caterpie    = 6,
    Rattata     = 7,
    Jigglypuff  = 8,
    Diglett     = 9,
}

public static class PictoCodesExtensions
{
    public static PictoCode[] GetPictoCodesFromLinkCode(int seed)
    {
        var xoro = new Xoroshiro128Plus((uint)seed);

        var result = new PictoCode[3];
        for (var i = 0; i < 3; i++)
            result[i] = (PictoCode)(xoro.NextInt(10));

        return result;
    }

    /// <summary>Splits a 0-999 value into three picture digits (72 becomes 0, 7, 2).</summary>
    public static PictoCode[] FromDigits(int value) =>
    [
        (PictoCode)(value / 100),
        (PictoCode)(value / 10 % 10),
        (PictoCode)(value % 10),
    ];

    /// <summary>Finds a link code seed (0-99999999) whose first three pictures match <paramref name="wanted"/>.</summary>
    public static bool TryGetLinkCode(ReadOnlySpan<PictoCode> wanted, out int code, Func<int, bool>? isAllowed = null)
    {
        const int max = 100_000_000;
        var start = Random.Shared.Next(max);

        for (var i = 0; i < max; i++)
        {
            var seed = (start + i) % max;
            var xoro = new Xoroshiro128Plus((uint)seed);

            var match = true;
            for (var j = 0; j < wanted.Length; j++)
            {
                if ((PictoCode)xoro.NextInt(10) != wanted[j])
                {
                    match = false;
                    break;
                }
            }

            if (match && (isAllowed?.Invoke(seed) ?? true))
            {
                code = seed;
                return true;
            }
        }

        code = 0;
        return false;
    }
}
