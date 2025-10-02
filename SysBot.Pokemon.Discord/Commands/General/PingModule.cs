using System.Diagnostics;
using Discord;
using Discord.Commands;

namespace SysBot.Pokemon.Discord;

public class PingModule : ModuleBase<SocketCommandContext>
{
    [Command("Ping")]
    [Summary("Makes the bot respond, indicating that it is running.")]
    public async Task PingAsync()
    {
        var stopwatch = new Stopwatch();
        stopwatch.Start();

        var embed = new EmbedBuilder()
            .WithTitle("Ping")
            .WithDescription("Pinging...")
            .WithColor(Color.Blue)
            .Build();

        var message = await ReplyAsync(embed: embed).ConfigureAwait(false);

        stopwatch.Stop();
        var responseTime = stopwatch.ElapsedMilliseconds;

        embed = new EmbedBuilder()
            .WithTitle("Pong!")
            .WithColor(Color.Green)
            .WithFooter($"Response Time: {responseTime}ms")
            .Build();

        await message.ModifyAsync(msg => msg.Embed = embed).ConfigureAwait(false);
    }
}
