using System.ComponentModel;

namespace SysBot.Pokemon;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class GenderEmojiSettings()
{
    public const string Gender = nameof(Gender);
    public override string ToString() => "Gender Emoji Settings";

    [Category(Gender), Description($"Discord code for the Male Emoji.")]
    public string MaleEmojiCode { get; set; } = string.Empty;

    [Category(Gender), Description($"Discord code for the Female Emoji.")]
    public string FemaleEmojiCode { get; set; } = string.Empty;

    [Category(Gender), Description($"Discord code for the Genderless Emoji.")]
    public string GenderlessEmojiCode { get; set; } = string.Empty;

    public string GetEmojiCode(byte gender) => gender switch
    {
        0 => MaleEmojiCode,
        1 => FemaleEmojiCode,
        2 => GenderlessEmojiCode,
        _ => throw new ArgumentOutOfRangeException(nameof(gender))
    };
}
