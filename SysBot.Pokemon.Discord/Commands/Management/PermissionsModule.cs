using Discord;
using Discord.Commands;
using PKHeX.Core;
using System.Data;

namespace SysBot.Pokemon.Discord;

public class PermissionsModule<T> : SudoModule<T> where T : PKM, new()
{
    [Command("PermissionCheck")]
    [Alias("PermCheck")]
    [Summary("Checks if the bot has the required permissions in whitelisted channels.")]
    [RequireOwner]
    public async Task ChannelPermissionTest()
    {
        var missingPermissionsEmbed = new EmbedBuilder()
                  .WithTitle("Channels with Missing Permissions")
                  .WithColor(Color.Red);

        bool allCorrect = true;
        var requiredPermissions = new List<ChannelPermission>
        {
            ChannelPermission.ViewChannel,
            ChannelPermission.SendMessages,
            ChannelPermission.ManageMessages,
            ChannelPermission.EmbedLinks,
            ChannelPermission.AttachFiles,
            ChannelPermission.ReadMessageHistory,
            ChannelPermission.AddReactions,
            ChannelPermission.UseExternalEmojis
        };

        foreach (var guild in Context.Client.Guilds)
        {
            foreach (var channel in guild.TextChannels)
            {
                if (SysCordSettings.Settings.ChannelWhitelist.Contains(channel.Id))
                {
                    var botPermissions = channel.Guild.CurrentUser.GetPermissions(channel);
                    var missingPerms = requiredPermissions.Where(p => !botPermissions.Has(p)).ToList();

                    if (missingPerms.Count != 0)
                    {
                        allCorrect = false;
                        if (missingPerms.Contains(ChannelPermission.EmbedLinks))
                        {
                            await Context.Channel.SendMessageAsync("You must enable \"EmbedLinks\" Permission to run this command");
                            return;
                        }
                        else
                        {
                            missingPermissionsEmbed.AddField(
                               name: channel.Name,
                               value: string.Join(", ", missingPerms.Select(p => p.ToString())),
                               inline: false
                           );
                        }                           
                    }
                }
            }
        }

        if (!allCorrect)
        {
            await Context.Channel.SendMessageAsync(embed: missingPermissionsEmbed.Build());
        }
        else
        {
            await ReplyAsync("All permissions for whitelisted channels are correct.").ConfigureAwait(false);
        }
    }
}
