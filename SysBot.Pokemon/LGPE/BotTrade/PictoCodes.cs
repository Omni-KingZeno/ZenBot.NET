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
}
