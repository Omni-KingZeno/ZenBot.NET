using Discord;
using Discord.Commands;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord;

public class OwnerModule<T> : SudoModule<T> where T : PKM, new()
{
    [Command("addSudo")]
    [Summary("Adds mentioned user(s) to global sudo")]
    [RequireOwner]
    public async Task SudoUsers([Summary("Ignored text")][Remainder] string _)
    {
        var users = Context.Message.MentionedUsers.Where(u => u.Id != Context.Client.CurrentUser.Id);
        var objects = users.Select(GetReference);
        SysCordSettings.Settings.GlobalSudoList.AddIfNew(objects);
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("addSudoRole")]
    [Summary("Adds mentioned role(s) to role sudo")]
    [RequireOwner]
    public async Task SudoRoles([Summary("Ignored text")][Remainder] string _)
    {
        var roles = Context.Message.MentionedRoles;
        var objects = roles.Select(GetRoleReference);
        SysCordSettings.Settings.RoleSudo.AddIfNew(objects);
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("removeSudo")]
    [Summary("Removes mentioned user(s) from global sudo")]
    [RequireOwner]
    public async Task RemoveSudoUsers([Summary("Ignored text")][Remainder] string _)
    {
        var users = Context.Message.MentionedUsers.Where(u => u.Id != Context.Client.CurrentUser.Id);
        var objects = users.Select(GetReference);
        SysCordSettings.Settings.GlobalSudoList.RemoveAll(z => objects.Any(o => o.ID == z.ID));
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("removeSudorole")]
    [Summary("Removes mentioned role(s) from role sudo")]
    [RequireOwner]
    public async Task RemoveSudoRoles([Summary("Ignored text")][Remainder] string _)
    {
        var roles = Context.Message.MentionedRoles;
        var objects = roles.Select(GetRoleReference);
        SysCordSettings.Settings.RoleSudo.RemoveAll(z => objects.Any(o => o.ID == z.ID));
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("addChannel")]
    [Summary("Adds a channel to the list of channels that are accepting commands.")]
    [RequireOwner]
    public async Task AddChannel()
    {
        var obj = GetReference(Context.Message.Channel);
        SysCordSettings.Settings.ChannelWhitelist.AddIfNew([obj]);
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("removeChannel")]
    [Summary("Removes a channel from the list of channels that are accepting commands.")]
    [RequireOwner]
    public async Task RemoveChannel()
    {
        var obj = GetReference(Context.Message.Channel);
        SysCordSettings.Settings.ChannelWhitelist.RemoveAll(z => z.ID == obj.ID);
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("addDistributionChannel")]
    [Alias("adec")]
    [Summary("Adds a channel to the list of channels that are accepting distribution embeds.")]
    [RequireOwner]
    public async Task AddDistributionChannel()
    {
        var obj = GetReference(Context.Message.Channel);
        SysCordSettings.HubConfig.Distribution.DistributionEmbedChannels.AddIfNew([obj]);
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("removeDistributionChannel")]
    [Alias("rdec")]
    [Summary("Removes a channel from the list of channels that are accepting distribution embeds.")]
    [RequireOwner]
    public async Task RemoveDistributionChannel()
    {
        var obj = GetReference(Context.Message.Channel);
        SysCordSettings.HubConfig.Distribution.DistributionEmbedChannels.RemoveAll(z => z.ID == obj.ID);
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("BanTradeCode")]
    [Alias("btc")]
    [Summary("Adds provided code to banned Trade Code list")]
    [RequireOwner]
    public async Task BanTradeCodeAsync([Summary("Trade Code")] uint code)
    {
        if (SysCord<T>.Runner.Hub.Config.Trade.BannedTradeCodes.Contains(code))
        {
            await ReplyAsync("Code already banned.").ConfigureAwait(false);
            return;
        }

        SysCord<T>.Runner.Hub.Config.Trade.BannedTradeCodes.Add(code);
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("ReinitializeLegalitySettings")]
    [Alias("reinit", "rils")]
    [Summary("Reinitializes legality settings after changing Legality settings")]
    [RequireOwner]
    public async Task ReintializeLegality()
    {
        var message = await ReplyAsync("Re-Initializing Legality Settings...").ConfigureAwait(false);
        AutoLegalityWrapper.InitializeAutoLegality(SysCord<T>.Runner.Hub.Config.Legality);
        await message.ModifyAsync(msg => msg.Content = "Done").ConfigureAwait(false);
    }

    [Command("leave")]
    [Alias("bye")]
    [Summary("Leaves the current server.")]
    [RequireOwner]
    public async Task Leave()
    {
        await ReplyAsync("Goodbye.").ConfigureAwait(false);
        await Context.Guild.LeaveAsync().ConfigureAwait(false);
    }

    [Command("leaveguild")]
    [Alias("lg")]
    [Summary("Leaves guild based on supplied ID.")]
    [RequireOwner]
    public async Task LeaveGuild([Summary("Guild ID")]string userInput)
    {
        if (!ulong.TryParse(userInput, out ulong id))
        {
            await ReplyAsync("Please provide a valid Guild ID.").ConfigureAwait(false);
            return;
        }

        var guild = Context.Client.Guilds.FirstOrDefault(x => x.Id == id);
        if (guild is null)
        {
            await ReplyAsync($"Provided input ({userInput}) is not a valid guild ID or the bot is not in the specified guild.").ConfigureAwait(false);
            return;
        }

        await ReplyAsync($"Leaving {guild}.").ConfigureAwait(false);
        await guild.LeaveAsync().ConfigureAwait(false);
    }

    [Command("leaveall")]
    [Summary("Leaves all servers the bot is currently in.")]
    [RequireOwner]
    public async Task LeaveAll()
    {
        await ReplyAsync("Leaving all servers.").ConfigureAwait(false);
        foreach (var guild in Context.Client.Guilds)
            await guild.LeaveAsync().ConfigureAwait(false);
    }

    [Command("listguilds")]
    [Alias("guilds", "lsg")]
    [Summary("Lists all servers the bot is currently in, with name, ID, join date, and icon.")]
    [RequireSudo]
    public async Task ListGuilds()
    {
        var guilds = Context.Client.Guilds.OrderBy(g => g.Name).ToList();
        var embeds = guilds.Select(g =>
        {
            var joined = g.CurrentUser?.JoinedAt;
            var joinedText = joined.HasValue
                ? $"<t:{joined.Value.ToUnixTimeSeconds()}:F> (<t:{joined.Value.ToUnixTimeSeconds()}:R>)"
                : "Unknown";

            var builder = new EmbedBuilder()
                .WithTitle(g.Name)
                .WithColor(Color.Purple)
                .AddField("Guild ID", $"`{g.Id}`", inline: true)
                .AddField("Members", g.MemberCount.ToString(), inline: true)
                .AddField("Bot Joined", joinedText);

            var icon = g.IconUrl;
            if (!string.IsNullOrWhiteSpace(icon))
                builder.WithThumbnailUrl(icon);

            return builder.Build();
        }).ToList();

        // Discord 10 Embed limit per message
        var pages = embeds.Chunk(10).ToList();
        for (int i = 0; i < pages.Count; i++)
        {
            var header = i == 0 ? $"## Guilds ({guilds.Count} total)" : string.Empty;
            await ReplyAsync(header, embeds: pages[i]).ConfigureAwait(false);
        }
    }

    [Command("sudoku")]
    [Alias("kill", "shutdown")]
    [Summary("Causes the entire process to end itself!")]
    [RequireOwner]
    public async Task ExitProgram()
    {
        await Context.Channel.EchoAndReply("Shutting down... goodbye! **Bot services are going offline.**").ConfigureAwait(false);
        Environment.Exit(0);
    }

    [Command("checkForUpdate")]
    [Alias("updateCheck", "cfu")]
    [Summary("Checks whether a newer version of the bot is available.")]
    [RequireOwner]
    public async Task CheckUpdateAsync()
    {
        if (UpdateBridge.CheckAsync is null)
        {
            await ReplyAsync("Update checking isn't available in this build.").ConfigureAwait(false);
            return;
        }

        try
        {
            var info = await UpdateBridge.CheckAsync().ConfigureAwait(false);
            var msg = info.IsUpToDate
                ? "Already on the latest version."
                : $"Update available: v{info.LatestVersion}" + (info.CanAutoUpdate ? "." : " (no downloadable asset; manual update required).");
            await ReplyAsync(msg).ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await ReplyAsync($"Update check failed: {ex.Message}").ConfigureAwait(false);
        }
    }

    [Command("update")]
    [Alias("selfupdate")]
    [Summary("Checks for an update, installs it if available, restarts the bot, and reports back here when finished.")]
    [RequireOwner]
    public async Task UpdateAsync()
    {
        if (UpdateBridge.CheckAsync is null || UpdateBridge.ApplyAsync is null)
        {
            await ReplyAsync("Auto-update isn't available in this build.").ConfigureAwait(false);
            return;
        }

        if (!UpdateBridge.TryBeginUpdate())
        {
            await ReplyAsync("An update is already in progress.").ConfigureAwait(false);
            return;
        }

        try
        {
            var info = await UpdateBridge.CheckAsync().ConfigureAwait(false);
            if (info.IsUpToDate)
            {
                await ReplyAsync("Already on the latest version.").ConfigureAwait(false);
                return;
            }
            if (!info.CanAutoUpdate)
            {
                await ReplyAsync($"v{info.LatestVersion} is available, but there's no downloadable asset. Please update manually.").ConfigureAwait(false);
                return;
            }

            await Context.Channel.EchoAndReply($"Updating to v{info.LatestVersion}... **Bot services will go offline briefly while restarting.**").ConfigureAwait(false);

            var notice = new UpdateBridge.PendingUpdateNotice
            {
                ChannelId = Context.Channel.Id,
                RequestedByUserId = Context.User.Id,
            };

            var failure = await UpdateBridge.ApplyAsync(notice).ConfigureAwait(false);
            await ReplyAsync($"Update didn't complete: {failure} Bots were resumed if they had been running.").ConfigureAwait(false);
        }
        catch (Exception ex)
        {
            await ReplyAsync($"Update failed: {ex.Message}").ConfigureAwait(false);
        }
        finally
        {
            UpdateBridge.EndUpdate();
        }
    }

    private RemoteControlAccess GetReference(IUser user) => new()
    {
        ID = user.Id,
        Name = user.Username,
        Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };

    private RemoteControlAccess GetReference(IChannel channel) => new()
    {
        ID = channel.Id,
        Name = channel.Name,
        Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };

    private RemoteControlAccess GetRoleReference(IRole role) => new()
    {
        ID = role.Id,
        Name = $"{role.Name}",
        Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };
}
