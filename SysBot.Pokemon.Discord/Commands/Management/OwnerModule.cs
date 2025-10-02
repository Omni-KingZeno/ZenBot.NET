using Discord;
using Discord.Commands;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord;

public class OwnerModule<T> : SudoModule<T> where T : PKM, new()
{
    [Command("addSudo")]
    [Summary("Adds mentioned user to global sudo")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task SudoUsers([Summary("Mentioned User(s)")][Remainder] string _)
    {
        var users = Context.Message.MentionedUsers;
        var objects = users.Select(GetReference);
        SysCordSettings.Settings.GlobalSudoList.AddIfNew(objects);
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("addSudoRole")]
    [Summary("Adds mentioned role to role sudo")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task SudoRoles([Summary("Mentioned Role(s)")][Remainder] string _)
    {
        var users = Context.Message.MentionedRoles;
        var objects = users.Select(GetRoleReference);
        SysCordSettings.Settings.RoleSudo.AddIfNew(objects);
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("removeSudo")]
    [Summary("Removes mentioned user from global sudo")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task RemoveSudoUsers([Summary("Mentioned User(s)")][Remainder] string _)
    {
        var users = Context.Message.MentionedUsers;
        var objects = users.Select(GetReference);
        SysCordSettings.Settings.GlobalSudoList.RemoveAll(z => objects.Any(o => o.ID == z.ID));
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("removeSudorole")]
    [Summary("Removes mentioned role from role sudo")]
    [RequireOwner]
    public async Task RemoveSudoRoles([Summary("Mentioned Role(s)")] string _)
    {
        var users = Context.Message.MentionedRoles;
        var objects = users.Select(GetRoleReference);
        SysCordSettings.Settings.RoleSudo.RemoveAll(z => objects.Any(o => o.ID == z.ID));
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("addChannel")]
    [Summary("Adds a channel to the list of channels that are accepting commands.")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task AddChannel()
    {
        var obj = GetReference(Context.Message.Channel);
        SysCordSettings.Settings.ChannelWhitelist.AddIfNew([obj]);
        await ReplyAsync("Done.").ConfigureAwait(false);
    }

    [Command("removeChannel")]
    [Summary("Removes a channel from the list of channels that are accepting commands.")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task RemoveChannel()
    {
        var obj = GetReference(Context.Message.Channel);
        SysCordSettings.Settings.ChannelWhitelist.RemoveAll(z => z.ID == obj.ID);
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

    [Command("leave")]
    [Alias("bye")]
    [Summary("Leaves the current server.")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task Leave()
    {
        await ReplyAsync("Goodbye.").ConfigureAwait(false);
        await Context.Guild.LeaveAsync().ConfigureAwait(false);
    }

    [Command("leaveguild")]
    [Alias("lg")]
    [Summary("Leaves guild based on supplied ID.")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task LeaveGuild(string userInput)
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
    // ReSharper disable once UnusedParameter.Global
    public async Task LeaveAll()
    {
        await ReplyAsync("Leaving all servers.").ConfigureAwait(false);
        foreach (var guild in Context.Client.Guilds)
            await guild.LeaveAsync().ConfigureAwait(false);
    }

    [Command("sudoku")]
    [Alias("kill", "shutdown")]
    [Summary("Causes the entire process to end itself!")]
    [RequireOwner]
    // ReSharper disable once UnusedParameter.Global
    public async Task ExitProgram()
    {
        await Context.Channel.EchoAndReply("Shutting down... goodbye! **Bot services are going offline.**").ConfigureAwait(false);
        Environment.Exit(0);
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
