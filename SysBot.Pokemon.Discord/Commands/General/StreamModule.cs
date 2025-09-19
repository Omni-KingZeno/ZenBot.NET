using System.Threading.Tasks;
using Discord.Commands;

namespace SysBot.Pokemon.Discord;

public class StreamModule : ModuleBase<SocketCommandContext>
{
    [Command("stream")]
    [Alias("streamlink", "twitch", "twitchlink")]
    [Summary("Returns the Host's Stream link.")]
    public async Task StreamMessageAsync()
    {
        if (!string.IsNullOrWhiteSpace(SysCordSettings.Settings.StreamLink))
            await ReplyAsync($"Here's the Stream link, enjoy :3 {SysCordSettings.Settings.StreamLink}").ConfigureAwait(false);
        else
            await ReplyAsync("Sorry, there is currently no stream link available.").ConfigureAwait(false);
    }
}
