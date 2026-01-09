using System.ComponentModel;
using PKHeX.Core;

namespace SysBot.Pokemon;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class TeraTypesEmojiSettings()
{
    public const string Types = nameof(Types);
    public override string ToString() => "Tera Emoji Settings";

    [Category(Types), Description($"Discord code for the Normal Tera Emoji.")]
    public string NormalEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Fighting Tera Emoji.")]
    public string FightingEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Flying Tera Emoji.")]
    public string FlyingEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Poison Tera Emoji.")]
    public string PoisonEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Ground Tera Emoji.")]
    public string GroundEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Rock Tera Emoji.")]
    public string RockEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Bug Tera Emoji.")]
    public string BugEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Ghost Tera Emoji.")]
    public string GhostEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Steel Tera Emoji.")]
    public string SteelEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Fire Tera Emoji.")]
    public string FireEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Water Tera Emoji.")]
    public string WaterEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Grass Tera Emoji.")]
    public string GrassEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Electric Tera Emoji.")]
    public string ElectricEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Psychic Tera Emoji.")]
    public string PsychicEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Ice Tera Emoji.")]
    public string IceEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Dragon Tera Emoji.")]
    public string DragonEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Dark Tera Emoji.")]
    public string DarkEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Fairy Tera Emoji.")]
    public string FairyEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Stellar Tera Emoji.")]
    public string StellarEmojiCode { get; set; } = string.Empty;

    public string GetEmojiCode(GemType type) => type switch
    {
        GemType.Normal   => NormalEmojiCode,
        GemType.Fighting => FightingEmojiCode,
        GemType.Flying   => FlyingEmojiCode,
        GemType.Poison   => PoisonEmojiCode,
        GemType.Ground   => GroundEmojiCode,
        GemType.Rock     => RockEmojiCode,
        GemType.Bug      => BugEmojiCode,
        GemType.Ghost    => GhostEmojiCode,
        GemType.Steel    => SteelEmojiCode,
        GemType.Fire     => FireEmojiCode,
        GemType.Water    => WaterEmojiCode,
        GemType.Grass    => GrassEmojiCode,
        GemType.Electric => ElectricEmojiCode,
        GemType.Psychic  => PsychicEmojiCode,
        GemType.Ice      => IceEmojiCode,
        GemType.Dragon   => DragonEmojiCode,
        GemType.Dark     => DarkEmojiCode,
        GemType.Fairy    => FairyEmojiCode,
        GemType.Stellar  => StellarEmojiCode,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
