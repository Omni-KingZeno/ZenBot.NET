using System.ComponentModel;
using System.Drawing;
using System.Text.Json.Serialization;

namespace SysBot.Pokemon;

[TypeConverter(typeof(ExpandableObjectConverter))]
public class AnnouncementEmbedSettings
{
    private const string Embed = nameof(Embed);
    public override string ToString() => "Announcement Embed Settings";

    [Category(Embed), Description("Announcement Header (supports markdown)")]
    public string? Header { get; set; } = "# Announcement";

    [Browsable(false)]
    public string ImgURL => string.IsNullOrEmpty(ImageURL) ? "https://archives.bulbagarden.net/media/upload/c/c3/VSRocker_PE.png" : ImageURL;
    [Category(Embed), Description("Enter a URL to display an image from the web with your announcement.")]
    public string? ImageURL { get; set; }

    [Category(Embed), Description("Where to display the image in the Announcement embed")]
    public ImageLocation ImageLocation { get; set; } = ImageLocation.Thumbnail;

    [Browsable(false)]
    [JsonPropertyName("EmbedColor")]
    public string EmbedColorHex { get; set; } = "#800080";

    [Category(Embed), Description("Color of the sidebar in the Announcement embed")]
    [JsonIgnore]
    public Color EmbedColor
    {
        get => ColorTranslator.FromHtml(EmbedColorHex);
        set => EmbedColorHex = $"#{value.R:X2}{value.G:X2}{value.B:X2}";
    }
}

public enum ImageLocation
{
    Thumbnail,
    MainBody
}
