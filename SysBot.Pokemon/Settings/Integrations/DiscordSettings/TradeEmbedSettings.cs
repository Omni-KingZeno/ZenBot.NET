using System.ComponentModel;
using PKHeX.Core;

namespace SysBot.Pokemon;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class TradeEmbedSettings
{
    private const string Emoji = nameof(Emoji);
    private const string Text = nameof(Text);
    private const string Image = nameof(Image);
    public override string ToString() => "Discord Embed Settings";

    [Category(Text), Description("Information to be displayed in Trade Embeds")]
    [TypeConverter(typeof(DisplayedInfoArrayConverter))]
    public DisplayedInfo[] EmbedDisplayedInfo { get; set; } = [DisplayedInfo.SpeciesFormHeldItem, DisplayedInfo.Ability, DisplayedInfo.Nature, DisplayedInfo.Ball, DisplayedInfo.Shiny, DisplayedInfo.Alpha, DisplayedInfo.Level, DisplayedInfo.IVs, DisplayedInfo.Moves];

    [Category(Text), Description("If true, will use Koi's TradeCord style embed layout")]
    public bool UseAlternateLayout { get; set; } = false;

    [Category(Image), Description("If true, will use 256x256 images instead of 128x128")]
    public bool UseFullSizeImages { get; set; } = false;

    [Category(Text), Description("If true, show the move PP amount in the Discord embed.")]
    public bool ShowMovePP { get; set; } = false;

    [Category(Text), Description("Uses the PKM's language for the embed if set to None. Forces the selected language if one is specified.")]
    public LanguageID ForceEmbedLanguage { get; set; } = LanguageID.None;

    [Category(Emoji), Description("If true, use GenderEmojiCodes for the Gender strings in the Discord embed.")]
    public bool UseGenderEmoji { get; set; } = false;

    [Category(Emoji), Description("List of emoji codes for Gender emojis.")]
    public GenderEmojiSettings GenderEmojiCodes { get; set; } = new();

    [Category(Emoji), Description("If true, adds the Plus Move emoji to applicable Moves in the Discord embed.")]
    public bool UsePlusMoveEmoji { get; set; } = false;

    [Category(Emoji), Description("Emoji code for the Plus Move emoji.")]
    public string PlusMoveEmojiCode { get; set; } = string.Empty;

    [Category(Emoji), Description("If true, use TypesEmojiCodes for the Move Type strings in the Discord embed.")]
    public bool UseMoveEmoji { get; set; } = false;

    [Category(Emoji), Description("List of emoji codes for Types emojis.")]
    public MoveTypesEmojiSettings MoveTypesEmojiCodes { get; set; } = new();

    [Category(Emoji), Description("If true, use TypesEmojiCodes for the Tera Type strings in the Discord embed.")]
    public bool UseTeraEmoji { get; set; } = false;

    [Category(Emoji), Description("List of emoji codes for Types emojis.")]
    public TeraTypesEmojiSettings TeraTypesEmojiCodes { get; set; } = new();
}
