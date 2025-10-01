using Discord;
using SixLabors.ImageSharp;
using SixLabors.ImageSharp.Formats.Png;
using SixLabors.ImageSharp.PixelFormats;
using SixLabors.ImageSharp.Processing;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;

namespace SysBot.Pokemon.Discord;

public static class PictoCodesEmbedBuilder
{
    public static (FileAttachment, EmbedBuilder) CreatePictoCodesEmbed(PictoCode[] codes)
    {
        var names = codes.Select(c => c.ToString()).ToArray();
        var sprites = new List<Image<Rgba32>>(3);
        try
        {
            foreach (var n in names)
            {
                var bytes = Properties.Resources.ResourceManager.GetObject(n) as byte[]
                            ?? throw new InvalidOperationException($"Resource '{n}.png' not found.");
                sprites.Add(SixLabors.ImageSharp.Image.Load<Rgba32>(bytes));
            }

            const int spacing = 12;
            const int padding = 8;

            var innerHeight = sprites.Max(i => i.Height);
            var innerWidth = sprites.Sum(i => i.Width) + spacing * (sprites.Count - 1);

            var width = innerWidth + padding * 2;
            var height = innerHeight + padding * 2;

            using var canvas = new Image<Rgba32>(width, height, new Rgba32(0, 0, 0, 0));
            canvas.Mutate(ctx =>
            {
                var x = padding;
                foreach (var img in sprites)
                {
                    var y = padding + (innerHeight - img.Height) / 2;
                    ctx.DrawImage(img, new Point(x, y), 1f);
                    x += img.Width + spacing;
                }
            });

            var ms = new MemoryStream();
            canvas.Save(ms, new PngEncoder());
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
}
