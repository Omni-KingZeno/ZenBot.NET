using System.Text.Json;
using Discord;
using Discord.WebSocket;
using SysBot.Base;

namespace SysBot.Pokemon.Discord;

/// <summary>
/// Lets the Discord project trigger updates implemented in the WinForms project (UpdateManager),
/// and carries the "who asked / where to report back" info across the process restart.
/// </summary>
public static class UpdateBridge
{
    public sealed record UpdateInfo(bool IsUpToDate, string? LatestVersion, bool CanAutoUpdate);

    /// <summary>Written to disk right before the update is applied, read back after relaunch.</summary>
    public sealed class PendingUpdateNotice
    {
        public ulong ChannelId { get; set; }
        public ulong RequestedByUserId { get; set; }
        public string? TargetVersion { get; set; }
        public bool BotsWereRunning { get; set; }
        public DateTime RequestedUtc { get; set; } = DateTime.UtcNow;
    }

    /// <summary>Filled in by the WinForms project at startup.</summary>
    public static Func<Task<UpdateInfo>>? CheckAsync { get; set; }

    /// <summary>
    /// Filled in by the WinForms project at startup. On success the process exits and this never returns;
    /// if it returns, the update did not happen and the result is the failure reason.
    /// </summary>
    public static Func<PendingUpdateNotice, Task<string>>? ApplyAsync { get; set; }

    private static int _updating;
    public static bool TryBeginUpdate() => Interlocked.CompareExchange(ref _updating, 1, 0) == 0;
    public static void EndUpdate() => Interlocked.Exchange(ref _updating, 0);

    private static readonly TimeSpan MaxAge = TimeSpan.FromMinutes(15);
    private static string NoticePath
    {
        get
        {
            var dir = Path.Combine(
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData),
                "ZenBot");
            Directory.CreateDirectory(dir);
            return Path.Combine(dir, "update_pending.json");
        }
    }

    public static void SavePending(PendingUpdateNotice notice)
    {
        try
        {
            File.WriteAllText(NoticePath, JsonSerializer.Serialize(notice));
            LogUtil.LogInfo($"Saved pending update notice to {NoticePath} (exists: {File.Exists(NoticePath)})", "Update");
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo($"Could not save pending update notice to {NoticePath}: {ex.Message}", "Update");
        }
    }

    public static void ClearPending()
    {
        try
        {
            if (File.Exists(NoticePath))
            {
                LogUtil.LogInfo($"ClearPending deleting notice. Caller:\n{Environment.StackTrace}", "Update");
                File.Delete(NoticePath);
            }
        }
        catch
        {
            // best effort
        }
    }

    /// <summary>Reads the pending notice without consuming it. Returns null if none, unreadable, or stale.</summary>
    public static PendingUpdateNotice? PeekPending()
    {
        try
        {
            if (!File.Exists(NoticePath))
                return null;

            var notice = JsonSerializer.Deserialize<PendingUpdateNotice>(File.ReadAllText(NoticePath));
            if (notice is null || DateTime.UtcNow - notice.RequestedUtc > MaxAge)
            {
                ClearPending();
                return null;
            }
            return notice;
        }
        catch
        {
            ClearPending();
            return null;
        }
    }

    /// <summary>
    /// Call once the Discord client is ready. If this launch follows a Discord-triggered update,
    /// posts the result to the channel the command was run in. Consumes the notice.
    /// </summary>
    public static async Task AnnounceIfPendingAsync(DiscordSocketClient client)
    {
        var notice = PeekPending();
        if (notice is null)
            return;

        ClearPending();

        try
        {
            if (await client.GetChannelAsync(notice.ChannelId).ConfigureAwait(false) is not IMessageChannel channel)
            {
                LogUtil.LogInfo($"Update finished, but channel {notice.ChannelId} was not reachable.", "Update");
                return;
            }

            var stillOutdated = false;
            var version = notice.TargetVersion;
            if (CheckAsync is not null)
            {
                try
                {
                    var info = await CheckAsync().ConfigureAwait(false);
                    stillOutdated = !info.IsUpToDate;
                    version = info.IsUpToDate ? version : info.LatestVersion ?? version;
                }
                catch
                {
                    // Offline / rate-limited: just report the restart.
                }
            }

            var mention = notice.RequestedByUserId != 0 ? $"<@{notice.RequestedByUserId}> " : string.Empty;
            var botsLine = notice.BotsWereRunning ? "Bots have been resumed." : "Bots were idle before the update and have been left idle.";

            var message = stillOutdated
                ? $"{mention} ZenBot restarted, but it still reports that v{version} is available. The update may not have applied; check the logs."
                : $"{mention} **Update successful!** ZenBot is back online{(version is null ? "" : $" on v{version}")}. {botsLine}";

            await channel.SendMessageAsync(message).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            LogUtil.LogInfo($"Failed to post update notice: {ex.Message}", "Update");
        }
    }
}
