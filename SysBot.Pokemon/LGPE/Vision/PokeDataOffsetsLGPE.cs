namespace SysBot.Pokemon;

/// <summary>
/// Pokémon Let's GO Pikachu &amp; Eevee RAM offsets and constants.
/// </summary>
public static class PokeDataOffsetsLGPE
{
    public const string LGPEGameVersion = "1.0.2";
    public const string LetsGoPikachuID = "010003F003A34000";
    public const string LetsGoEeveeID = "0100187003A36000";

    public const int BoxFormatSlotSize  = 0x104;
    public const int TrainerDataLength  = 0x168;
    public const int SlotCount          = 25;
    public const int GapSize            = 380;

    // HEAP Offsets
    public const uint Trader1MyStatusOffset         = 0x41A28240;
    public const uint Trader2MyStatusOffset         = 0x41A28078;
    public const uint TradePartnerPokemonOffset     = 0x41A22858;
    public const uint BoxStartOffset                = 0x533675B0;
    public const uint TrainerDataOffset             = 0x53321CF0;
    public const uint TextSpeedOffset               = 0x53321EDC;
    public const uint OverworldOffset               = 0x5E1CE550;

    // MAIN Offsets
    public const uint WaitingScreenOffset           = 0x15363d8;
    public const uint CurrentScreenOffset           = 0x1610E68;

    // Screen Constants
    public const uint SaveScreen                    = 0x7250;
    public const uint SaveScreen2                   = 0x6250;
    public const uint MenuScreen                    = 0xD080;
    public const uint BoxScreen                     = 0xF080;
    public const uint WaitingToTradeScreen          = 0x0080;

}

public enum ScreenScenario : ushort
{
    WaitingToTrade  = 0x0080,
    WaitingToTrade2 = 0x1080,
    Save2           = 0x6250,
    Save            = 0x7250,
    SelectFaraway   = 0xA080,
    Scroll          = 0xB080,
    Menu            = 0xD080,
    YesNoSelector   = 0xE080,
    Box             = 0xF080
}
