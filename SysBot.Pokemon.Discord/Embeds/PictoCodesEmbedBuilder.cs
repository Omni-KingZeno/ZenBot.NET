using Discord;
using StbImageSharp;
using StbImageWriteSharp;
using ReadColorComponents = StbImageSharp.ColorComponents;
using WriteColorComponents = StbImageWriteSharp.ColorComponents;

namespace SysBot.Pokemon.Discord;

public static class PictoCodesEmbedBuilder
{
    private readonly record struct RgbaImage(int Width, int Height, byte[] Data);

    public static (FileAttachment, EmbedBuilder) CreatePictoCodesEmbed(PictoCode[] codes)
    {
        var names = codes.Select(c => c.ToString()).ToArray();
        var sprites = new List<RgbaImage>(3);

        foreach (var n in names)
        {
            var bytes = Properties.Resources.ResourceManager.GetObject(n) as byte[]
                        ?? throw new InvalidOperationException($"Resource '{n}.png' not found.");
            var decoded = ImageResult.FromMemory(bytes, ReadColorComponents.RedGreenBlueAlpha)
                          ?? throw new InvalidOperationException($"Resource '{n}.png' is not a valid image.");
            sprites.Add(TrimTransparent(new RgbaImage(decoded.Width, decoded.Height, decoded.Data)));
        }

        const int spacing = 24;
        const int padding = 16;
        const int scale = 2;

        var innerHeight = sprites.Max(i => i.Height) * scale;
        var innerWidth = sprites.Sum(i => i.Width * scale) + spacing * (sprites.Count - 1);

        var width = innerWidth + padding * 2;
        var height = innerHeight + padding * 2;

        var canvas = new byte[width * height * 4];
        var dstX = padding;

        foreach (var img in sprites)
        {
            var scaledH = img.Height * scale;
            var dstY = padding + (innerHeight - scaledH) / 2;

            for (int sy = 0; sy < img.Height; sy++)
            {
                for (int sx = 0; sx < img.Width; sx++)
                {
                    int srcIdx = (sy * img.Width + sx) * 4;
                    byte r = img.Data[srcIdx];
                    byte g = img.Data[srcIdx + 1];
                    byte b = img.Data[srcIdx + 2];
                    byte a = img.Data[srcIdx + 3];
                    if (a == 0)
                        continue;

                    for (int dy = 0; dy < scale; dy++)
                    {
                        int py = dstY + sy * scale + dy;
                        for (int dx = 0; dx < scale; dx++)
                        {
                            int px = dstX + sx * scale + dx;
                            int dstIdx = (py * width + px) * 4;
                            canvas[dstIdx] = r;
                            canvas[dstIdx + 1] = g;
                            canvas[dstIdx + 2] = b;
                            canvas[dstIdx + 3] = a;
                        }
                    }
                }
            }

            dstX += img.Width * scale + spacing;
        }

        var ms = new MemoryStream();
        var writer = new ImageWriter();
        writer.WritePng(canvas, width, height, WriteColorComponents.RedGreenBlueAlpha, ms);
        ms.Position = 0;

        const string fileName = "pictocodes.png";
        var attachment = new FileAttachment(ms, fileName);

        var embed = new EmbedBuilder
        {
            Title = string.Join(", ", names),
        }.WithImageUrl($"attachment://{fileName}");

        return (attachment, embed);
    }

    /// <summary>
    /// Crops fully transparent margins so spacing and centering apply to the visible sprite.
    /// </summary>
    private static RgbaImage TrimTransparent(RgbaImage src)
    {
        int minX = src.Width, minY = src.Height, maxX = -1, maxY = -1;
        for (int y = 0; y < src.Height; y++)
        {
            for (int x = 0; x < src.Width; x++)
            {
                if (src.Data[(y * src.Width + x) * 4 + 3] == 0)
                    continue;
                if (x < minX) minX = x;
                if (x > maxX) maxX = x;
                if (y < minY) minY = y;
                if (y > maxY) maxY = y;
            }
        }

        if (maxX < 0)
            return src; // fully transparent, leave as is

        int w = maxX - minX + 1;
        int h = maxY - minY + 1;
        var trimmed = new byte[w * h * 4];
        int rowBytes = w * 4;

        for (int y = 0; y < h; y++)
        {
            int srcOffset = ((minY + y) * src.Width + minX) * 4;
            int dstOffset = y * rowBytes;
            Buffer.BlockCopy(src.Data, srcOffset, trimmed, dstOffset, rowBytes);
        }

        return new RgbaImage(w, h, trimmed);
    }
}
