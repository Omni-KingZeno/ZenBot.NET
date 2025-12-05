using PKHeX.Core;
using PKHeX.Drawing.PokeSprite;

namespace SysBot.Pokemon.WinForms;

public static class InitUtil
{
    public static void InitializeStubs(ProgramMode mode)
    {
        var sav = GetFakeSaveFile(mode);
        SetupItemTradeSpecies(sav);
        SetUpSpriteCreator(sav);
    }

    private static SaveFile GetFakeSaveFile(ProgramMode mode) => mode switch
    {
        ProgramMode.LGPE => new SAV7b(),
        ProgramMode.SWSH => new SAV8SWSH(),
        ProgramMode.BDSP => new SAV8BS(),
        ProgramMode.LA => new SAV8LA(),
        ProgramMode.SV => new SAV9SV(),
        ProgramMode.LZA => new SAV9ZA(),
        _ => throw new ArgumentOutOfRangeException(nameof(mode)),
    };

    private static void SetUpSpriteCreator(SaveFile sav)
    {
        SpriteUtil.Initialize(sav);
        StreamSettings.CreateSpriteFile = (pk, fn) =>
        {
            var png = pk.Sprite();
            png.Save(fn);
        };
    }

    private static void SetupItemTradeSpecies(SaveFile sav)
    {
        ValidSpeciesConverter.Version = sav switch
        {
            SAV7b => GameVersion.GG,
            SAV8SWSH => GameVersion.SWSH,
            SAV8BS => GameVersion.BDSP,
            SAV8LA => GameVersion.PLA,
            SAV9SV => GameVersion.SV,
            _ => GameVersion.ZA,
        };
    }
}
