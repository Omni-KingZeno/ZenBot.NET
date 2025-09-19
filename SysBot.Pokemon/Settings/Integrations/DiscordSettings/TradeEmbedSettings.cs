using System.ComponentModel;
using PKHeX.Core;

namespace SysBot.Pokemon;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class TradeEmbedSettings
{
    private const string Emoji = nameof(Emoji);
    private const string Text = nameof(Text);
    public override string ToString() => "Discord Embed Settings";

    [Category(Text), Description("Uses the PKM's language for the embed if set to None. Forces the selected language if one is specified.")]
    public LanguageID ForceEmbedLanguage { get; set; } = LanguageID.None;

    [Category(Emoji), Description("If true, use GenderEmojiCodes for the Gender strings in the Discord embed.")]
    public bool UseGenderEmoji { get; set; } = false;

    [Category(Emoji), Description("List of emoji codes for Gender emojis.")]
    public GenderEmojiSettings GenderEmojiCodes { get; set; } = new();

    [Category(Emoji), Description("If true, use TypesEmojiCodes for the Move Type strings in the Discord embed.")]
    public bool UseMoveEmoji { get; set; } = false;

    [Category(Emoji), Description("If true, use TypesEmojiCodes for the Tera Type strings in the Discord embed.")]
    public bool UseTeraEmoji { get; set; } = false;

    [Category(Emoji), Description("List of emoji codes for Types emojis.")]
    public TypesEmojiSettings TypesEmojiCodes { get; set; } = new();

    [Category(Text), Description("If true, show the move PP amount in the Discord embed.")]
    public bool ShowMovePP { get; set; } = false;

    [Category(Text), Description("If true, show the Pokémon IVs in the Discord embed.")]
    public bool ShowIVs { get; set; } = true;

    [Category(Text), Description("If true, show the Pokémon EVs in the Discord embed.")]
    public bool ShowEVs { get; set; } = true;

    [Category(Text), Description("If true, will use Koi's TradeCord style embed layout")]
    public bool UseAlternateLayout { get; set; } = false;
}
