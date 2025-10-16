using Discord;
using Discord.Commands;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord;

[Summary("Distribution Pool Module")]
public class PoolModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static PokeTradeHub<T> Hub => SysCord<T>.Runner.Hub;

    [Command("poolReload")]
    [Summary("Reloads the bot pool from the setting's folder.")]
    [RequireSudo]
    public async Task ReloadPoolAsync()
    {
        var pool = Hub.Ledy.Pool.Reload(Hub.Config.Folder.DistributeFolder);
        if (!pool)
            await ReplyAsync("Failed to reload from folder.").ConfigureAwait(false);
        else
            await ReplyAsync($"Reloaded from folder. Pool count: {Hub.Ledy.Pool.Count}").ConfigureAwait(false);
    }

    [Command("pool")]
    [Summary("Displays the details of Pokémon files in the random pool.")]
    public async Task DisplayPoolCountAsync()
    {
        var pool = Hub.Ledy.Pool;
        if (pool.Count == 0)
        {
            await ReplyAsync("Distribution pool is empty.").ConfigureAwait(false);
        }

        var lines = pool.Files.Select((z, i) => $"{i + 1:00}: {z.Key} = {(Species)z.Value.RequestInfo.Species}");
        var embeds = new List<Embed>();
        var pages = lines.Chunk(20).ToList();

        for (int i = 0; i < pages.Count; i++)
        {
            var builder = new EmbedBuilder
            {
                Color = Color.Blue,
                Title = $"Distribution Pool",
                Description = string.Join("\n", pages[i])
            };

            embeds.Add(builder.Build());
        }

        await PaginatedMessage.CreateAsync(Context, embeds).ConfigureAwait(false);
    }
}
