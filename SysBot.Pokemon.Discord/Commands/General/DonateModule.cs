using Discord.Commands;

namespace SysBot.Pokemon.Discord;

public class DonateModule : ModuleBase<SocketCommandContext>
{
    [Command("donate")]
    [Alias("donation")]
    [Summary("Returns the Host's Donation link.")]
    public async Task DonateAsync()
    {
        if (!string.IsNullOrWhiteSpace(SysCordSettings.Settings.DonationLink))
            await ReplyAsync($"Here's the donation link! Thank you for your support :3 {SysCordSettings.Settings.DonationLink}").ConfigureAwait(false);
        else
            await ReplyAsync("Sorry, there is currently no donation link available.").ConfigureAwait(false);
    }
}
