using Discord.Commands;
using Discord.WebSocket;

namespace SysBot.Pokemon.Discord;

public class RoleModule : ModuleBase<SocketCommandContext>
{
    private readonly DiscordSettings Settings = SysCordSettings.Settings;

    [Command("AddTradeRole")]
    [Alias("art")]
    [Summary("Adds the mentioned Role to the \"RoleCanTrade\" list.")]
    [RequireOwner]
    public async Task AddTradeRole([Summary("Mentioned Role")] SocketRole role)
    {
        if (Settings.RoleCanTrade.Contains(role.Id))
        {
            await ReplyAsync("Role Already exists in settings.").ConfigureAwait(false);
            return;
        }

        Settings.RoleCanTrade.AddIfNew([GetRoleReference(role)]);
        await ReplyAsync($"Added {role.Name} to the list!").ConfigureAwait(false);
    }

    [Command("RemoveTradeRole")]
    [Alias("rrt")]
    [Summary("Removes the mentioned Role from the \"RoleCanTrade\" list")]
    [RequireOwner]
    public async Task RemoveTradeRole([Summary("Mentioned Role")] SocketRole role)
    {
        if (!Settings.RoleCanTrade.Contains(role.Id))
        {
            await ReplyAsync("Role does not exists in settings.").ConfigureAwait(false);
            return;
        }
        Settings.RoleCanTrade.RemoveAll(z => z.ID == role.Id);
        await ReplyAsync($"Removed {role.Name} from the list.").ConfigureAwait(false);
    }

    [Command("AddSeedCheckRole")]
    [Alias("arsc")]
    [Summary("Adds the mentioned Role to the \"RoleCanSeedCheck\" list.")]
    [RequireOwner]
    public async Task AddSeedCheckRole([Summary("Mentioned Role")] SocketRole role)
    {
        if (Settings.RoleCanSeedCheck.Contains(role.Id))
        {
            await ReplyAsync("Role Already exists in settings.").ConfigureAwait(false);
            return;
        }

        Settings.RoleCanSeedCheck.AddIfNew([GetRoleReference(role)]);
        await ReplyAsync($"Added {role.Name} to the list!").ConfigureAwait(false);
    }

    [Command("RemoveSeedCheckRole")]
    [Alias("rrsc")]
    [Summary("Removes the mentioned Role from the \"RoleCanSeedCheck\" list")]
    [RequireOwner]
    public async Task RemoveSeedCheckRole([Summary("Mentioned Role")] SocketRole role)
    {
        if (!Settings.RoleCanSeedCheck.Contains(role.Id))
        {
            await ReplyAsync("Role does not exists in settings.").ConfigureAwait(false);
            return;
        }
        Settings.RoleCanSeedCheck.RemoveAll(z => z.ID == role.Id);
        await ReplyAsync($"Removed {role.Name} from the list.").ConfigureAwait(false);
    }

    [Command("AddcloneRole")]
    [Alias("arc")]
    [Summary("Adds the mentioned Role to the \"RoleCanClone\" list.")]
    [RequireOwner]
    public async Task AddCloneRole([Summary("Mentioned Role")] SocketRole role)
    {
        if (Settings.RoleCanClone.Contains(role.Id))
        {
            await ReplyAsync("Role Already exists in settings.").ConfigureAwait(false);
            return;
        }

        Settings.RoleCanClone.AddIfNew([GetRoleReference(role)]);
        await ReplyAsync($"Added {role.Name} to the list!").ConfigureAwait(false);
    }

    [Command("RemoveCloneRole")]
    [Alias("rrc")]
    [Summary("Removes the mentioned Role from the \"RoleCanClone\" list")]
    [RequireOwner]
    public async Task RemoveCloneRole([Summary("Mentioned Role")] SocketRole role)
    {
        if (!Settings.RoleCanClone.Contains(role.Id))
        {
            await ReplyAsync("Role does not exists in settings.").ConfigureAwait(false);
            return;
        }
        Settings.RoleCanClone.RemoveAll(z => z.ID == role.Id);
        await ReplyAsync($"Removed {role.Name} from the list.").ConfigureAwait(false);
    }

    [Command("AddDumpRole")]
    [Alias("ard")]
    [Summary("Adds the mentioned Role to the \"RoleCanDump\" list.")]
    [RequireOwner]
    public async Task AddDumpRole([Summary("Mentioned Role")] SocketRole role)
    {
        if (Settings.RoleCanDump.Contains(role.Id))
        {
            await ReplyAsync("Role Already exists in settings.").ConfigureAwait(false);
            return;
        }

        Settings.RoleCanDump.AddIfNew([GetRoleReference(role)]);
        await ReplyAsync($"Added {role.Name} to the list!").ConfigureAwait(false);
    }

    [Command("RemoveDumpRole")]
    [Alias("rrd")]
    [Summary("Removes the mentioned Role from the \"RoleCanDump\" list")]
    [RequireOwner]
    public async Task RemoveDumpRole([Summary("Mentioned Role")] SocketRole role)
    {
        if (!Settings.RoleCanDump.Contains(role.Id))
        {
            await ReplyAsync("Role does not exists in settings.").ConfigureAwait(false);
            return;
        }
        Settings.RoleCanDump.RemoveAll(z => z.ID == role.Id);
        await ReplyAsync($"Removed {role.Name} from the list.").ConfigureAwait(false);
    }

    [Command("AddRemoteControlRole")]
    [Alias("arrc")]
    [Summary("Adds the mentioned Role to the \"RoleRemoteControl\" list.")]
    [RequireOwner]
    public async Task AddRemoteControlRole([Summary("Mentioned Role")] SocketRole role)
    {
        if (Settings.RoleRemoteControl.Contains(role.Id))
        {
            await ReplyAsync("Role Already exists in settings.").ConfigureAwait(false);
            return;
        }

        Settings.RoleRemoteControl.AddIfNew([GetRoleReference(role)]);
        await ReplyAsync($"Added {role.Name} to the list!").ConfigureAwait(false);
    }

    [Command("RemoveRemoteControlRole")]
    [Alias("rrrc")]
    [Summary("Removes the mentioned Role from the \"RoleRemoteControl\" list")]
    [RequireOwner]
    public async Task RemoveRemoteControlRole([Summary("Mentioned Role")] SocketRole role)
    {
        if (!Settings.RoleRemoteControl.Contains(role.Id))
        {
            await ReplyAsync("Role does not exists in settings.").ConfigureAwait(false);
            return;
        }
        Settings.RoleRemoteControl.RemoveAll(z => z.ID == role.Id);
        await ReplyAsync($"Removed {role.Name} from the list.").ConfigureAwait(false);
    }

    [Command("AddFavoredRole")]
    [Alias("arf")]
    [Summary("Adds the mentioned Role to the \"RoleCanTrade\" list.")]
    [RequireOwner]
    public async Task AddFavoredRole([Summary("Mentioned Role")] SocketRole role)
    {
        if (Settings.RoleFavored.Contains(role.Id))
        {
            await ReplyAsync("Role Already exists in settings.").ConfigureAwait(false);
            return;
        }

        Settings.RoleFavored.AddIfNew([GetRoleReference(role)]);
        await ReplyAsync($"Added {role.Name} to the list!").ConfigureAwait(false);
    }

    [Command("RemoveFavoredRole")]
    [Alias("rrf")]
    [Summary("Removes the mentioned Role from the \"RoleCanTrade\" list")]
    [RequireOwner]
    public async Task RemoveFavoredRole([Summary("Mentioned Role")] SocketRole role)
    {
        if (!Settings.RoleFavored.Contains(role.Id))
        {
            await ReplyAsync("Role does not exists in settings.").ConfigureAwait(false);
            return;
        }
        SysCordSettings.Settings.RoleFavored.RemoveAll(z => z.ID == role.Id);
        await ReplyAsync($"Removed {role.Name} from the list.").ConfigureAwait(false);
    }

    private RemoteControlAccess GetRoleReference(SocketRole role) => new()
    {
        ID = role.Id,
        Name = $"{role.Name}",
        Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };
}
