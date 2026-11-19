using Discord;
using SkiaSharp;

namespace SysBot.Pokemon.Discord;

public static class PictoCodesEmbedBuilder
{
    public static (FileAttachment, EmbedBuilder) CreatePictoCodesEmbed(PictoCode[] codes)
    {
        var names = codes.Select(c => c.ToString()).ToArray();
        var sprites = new List<SKBitmap>(3);
        try
        {
            foreach (var n in names)
            {
                var bytes = Properties.Resources.ResourceManager.GetObject(n) as byte[]
                            ?? throw new InvalidOperationException($"Resource '{n}.png' not found.");
                var bmp = SKBitmap.Decode(bytes)
                          ?? throw new InvalidOperationException($"Resource '{n}.png' is not a valid image.");
                sprites.Add(TrimTransparent(bmp));
            }

            const int spacing = 24;
            const int padding = 16;
            const int scale = 2;

            var innerHeight = sprites.Max(i => i.Height) * scale;
            var innerWidth = sprites.Sum(i => i.Width * scale) + spacing * (sprites.Count - 1);

            var width = innerWidth + padding * 2;
            var height = innerHeight + padding * 2;

            using var canvas = new SKBitmap(width, height, SKColorType.Rgba8888, SKAlphaType.Premul);
            using (var c = new SKCanvas(canvas))
            {
                c.Clear(SKColors.Transparent);
                var sampling = new SKSamplingOptions(SKFilterMode.Nearest);
                var x = padding;
                foreach (var img in sprites)
                {
                    var w = img.Width * scale;
                    var h = img.Height * scale;
                    var y = padding + (innerHeight - h) / 2;
                    using var skImg = SKImage.FromBitmap(img);
                    c.DrawImage(skImg, SKRect.Create(x, y, w, h), sampling);
                    x += w + spacing;
                }
            }

            using var image = SKImage.FromBitmap(canvas);
            using var data = image.Encode(SKEncodedImageFormat.Png, 100);

            var ms = new MemoryStream();
            data.SaveTo(ms);
            ms.Position = 0;

            var fileName = "pictocodes.png";
            var attachment = new FileAttachment(ms, fileName);

            var embed = new EmbedBuilder
            {
                Title = string.Join(", ", names),
            }.WithImageUrl($"attachment://{fileName}");

            return (attachment, embed);
        }
        finally
        {
            foreach (var img in sprites)
                img.Dispose();
        }
    }

    /// <summary>
    /// Crops fully transparent margins so spacing and centering apply to the visible sprite.
    /// Takes ownership of <paramref name="src"/> (disposes it) when a new bitmap is returned.
    /// </summary>
    private static SKBitmap TrimTransparent(SKBitmap src)
    {
        int minX = src.Width, minY = src.Height, maxX = -1, maxY = -1;
        for (int y = 0; y < src.Height; y++)
        {
            for (int x = 0; x < src.Width; x++)
            {
                if (src.GetPixel(x, y).Alpha == 0)
                    continue;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }

        if (maxX < 0)
            return src; // fully transparent, leave as is

        var rect = new SKRectI(minX, minY, maxX + 1, maxY + 1);
        using var subset = new SKBitmap();
        if (!src.ExtractSubset(subset, rect))
            return src;

        var trimmed = subset.Copy(); // independent pixels, safe to dispose src
        src.Dispose();
        return trimmed;
    }
}
