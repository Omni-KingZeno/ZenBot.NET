using Discord;
using Discord.Commands;
using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon.Discord;

public partial class AnnouncementModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static readonly AnnouncementEmbedSettings Settings = SysCord<T>.Runner.Hub.Config.Discord.AnnouncementEmbedSettings;

    [Command("Announcement")]
    [Alias("announce")]
    [Summary("Sends an announcement message to all whitelisted guild channels.")]
    [RequireOwner]
    public async Task AnnouncementBroadcast([Summary("Announcement to Broadcast")][Remainder] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            await ReplyAsync("You must include a message.").ConfigureAwait(false);
            return;
        }

        string? tempImagePath = null;
        try
        {
            tempImagePath = await DownloadAttachmentIfPresent().ConfigureAwait(false);
            await SendToWhitelistedChannels(message, tempImagePath).ConfigureAwait(false);
            await Context.Message.DeleteAsync().ConfigureAwait(false);
        }
        finally
        {
            CleanupTempFile(tempImagePath);
        }
    }

    [Command("PreviewAnnouncement")]
    [Alias("preview")]
    [Summary("Sends a preview of an announcement message to the current channel.")]
    [RequireOwner]
    public async Task PreviewAnnouncement([Summary("Announcement to Broadcast")][Remainder] string message)
    {
        if (string.IsNullOrWhiteSpace(message))
        {
            await ReplyAsync("You must include a message.").ConfigureAwait(false);
            return;
        }

        string? tempImagePath = null;
        try
        {
            tempImagePath = await DownloadAttachmentIfPresent().ConfigureAwait(false);
            var color = new Color(Settings.EmbedColor.R, Settings.EmbedColor.G, Settings.EmbedColor.B);

            if (tempImagePath != null)
            {
                await using var fileStream = new FileStream(tempImagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
                var fileName = Path.GetFileName(tempImagePath);
                var file = new FileAttachment(fileStream, fileName);

                var embed = CreateAnnouncementEmbed(message, $"attachment://{fileName}", color);
                await Context.Channel.SendFileAsync(file, embed: embed.Build()).ConfigureAwait(false);
            }
            else
            {
                var embed = CreateAnnouncementEmbed(message, Settings.ImgURL, color);
                await Context.Channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
            }

            await Context.Message.DeleteAsync().ConfigureAwait(false);
        }
        finally
        {
            CleanupTempFile(tempImagePath);
        }
    }

    private async Task<string?> DownloadAttachmentIfPresent()
    {
        var attachment = Context.Message.Attachments.FirstOrDefault();
        if (attachment == null)
            return null;

        try
        {
            using var client = new HttpClient();
            client.Timeout = TimeSpan.FromSeconds(10);

            var imageBytes = await client.GetByteArrayAsync(attachment.Url).ConfigureAwait(false);
            var tempPath = Path.Combine(Path.GetTempPath(), $"{Guid.NewGuid()}{Path.GetExtension(attachment.Filename)}");

            await File.WriteAllBytesAsync(tempPath, imageBytes).ConfigureAwait(false);
            return tempPath;
        }
        catch (Exception ex)
        {
            LogUtil.LogSafe(ex, $"Failed to download attachment: {ex.Message}");
            return null;
        }
    }

    private async Task SendToWhitelistedChannels(string message, string? imagePath)
    {
        var successCount = 0;
        var failCount = 0;

        foreach (var guild in Context.Client.Guilds)
        {
            var whitelistedChannels = guild.TextChannels.Where(c => SysCordSettings.Settings.ChannelWhitelist.Contains(c.Id));

            foreach (var channel in whitelistedChannels)
            {
                try
                {
                    await SendAnnouncementToChannel(channel, message, imagePath, Settings.ImgURL).ConfigureAwait(false);
                    successCount++;
                }
                catch (Exception ex)
                {
                    LogUtil.LogSafe(ex, $"Failed to send announcement in {channel.Name} of {guild.Name}: {ex.Message}");
                    failCount++;
                }
            }
        }

        LogUtil.LogInfo($"Announcement broadcast complete. Succeeded: {successCount}, Failed: {failCount}", "Announce");
    }

    private static async Task SendAnnouncementToChannel(ITextChannel channel, string message, string? imagePath, string imageUrl)
    {
        var color = new Color(Settings.EmbedColor.R, Settings.EmbedColor.G, Settings.EmbedColor.B);

        if (imagePath != null)
        {
            await using var fileStream = new FileStream(imagePath, FileMode.Open, FileAccess.Read, FileShare.Read);
            var fileName = Path.GetFileName(imagePath);
            var file = new FileAttachment(fileStream, fileName);

            var embed = CreateAnnouncementEmbed(message, $"attachment://{fileName}", color);
            await channel.SendFileAsync(file, embed: embed.Build()).ConfigureAwait(false);
        }
        else
        {
            var embed = CreateAnnouncementEmbed(message, imageUrl, color);
            await channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
        }
    }

    private static EmbedBuilder CreateAnnouncementEmbed(string message, string imageUrl, Color color)
    {
        var embed = new EmbedBuilder()
            .WithColor(color)
            .WithDescription($"{Settings.Header}\n{message}");

        if (Settings.ImageLocation is ImageLocation.Thumbnail)
            embed.WithThumbnailUrl(imageUrl);
        else
            embed.WithImageUrl(imageUrl);

        return embed;
    }

    private static void CleanupTempFile(string? filePath)
    {
        if (filePath != null && File.Exists(filePath))
        {
            try
            {
                File.Delete(filePath);
            }
            catch (Exception ex)
            {
                LogUtil.LogSafe(ex, $"Failed to delete temporary file {filePath}: {ex.Message}");
            }
        }
    }
}
