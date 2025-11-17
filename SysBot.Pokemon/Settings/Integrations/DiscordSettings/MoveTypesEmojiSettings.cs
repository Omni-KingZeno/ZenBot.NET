using System.ComponentModel;
using PKHeX.Core;

namespace SysBot.Pokemon;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class MoveTypesEmojiSettings()
{
    public const string Types = nameof(Types);
    public override string ToString() => "Types Emoji Settings";

    [Category(Types), Description($"Discord code for the Normal Type Emoji.")]
    public string NormalEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Fighting Type Emoji.")]
    public string FightingEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Flying Type Emoji.")]
    public string FlyingEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Poison Type Emoji.")]
    public string PoisonEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Ground Type Emoji.")]
    public string GroundEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Rock Type Emoji.")]
    public string RockEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Bug Type Emoji.")]
    public string BugEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Ghost Type Emoji.")]
    public string GhostEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Steel Type Emoji.")]
    public string SteelEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Fire Type Emoji.")]
    public string FireEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Water Type Emoji.")]
    public string WaterEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Grass Type Emoji.")]
    public string GrassEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Electric Type Emoji.")]
    public string ElectricEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Psychic Type Emoji.")]
    public string PsychicEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Ice Type Emoji.")]
    public string IceEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Dragon Type Emoji.")]
    public string DragonEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Dark Type Emoji.")]
    public string DarkEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Fairy Type Emoji.")]
    public string FairyEmojiCode { get; set; } = string.Empty;

    [Category(Types), Description($"Discord code for the Stellar Type Emoji.")]
    public string StellarEmojiCode { get; set; } = string.Empty;

    public string GetEmojiCode(MoveType type) => type switch
    {
        MoveType.Normal => NormalEmojiCode,
        MoveType.Fighting => FightingEmojiCode,
        MoveType.Flying => FlyingEmojiCode,
        MoveType.Poison => PoisonEmojiCode,
        MoveType.Ground => GroundEmojiCode,
        MoveType.Rock => RockEmojiCode,
        MoveType.Bug => BugEmojiCode,
        MoveType.Ghost => GhostEmojiCode,
        MoveType.Steel => SteelEmojiCode,
        MoveType.Fire => FireEmojiCode,
        MoveType.Water => WaterEmojiCode,
        MoveType.Grass => GrassEmojiCode,
        MoveType.Electric => ElectricEmojiCode,
        MoveType.Psychic => PsychicEmojiCode,
        MoveType.Ice => IceEmojiCode,
        MoveType.Dragon => DragonEmojiCode,
        MoveType.Dark => DarkEmojiCode,
        MoveType.Fairy => FairyEmojiCode,
        _ => throw new ArgumentOutOfRangeException(nameof(type))
    };
}
