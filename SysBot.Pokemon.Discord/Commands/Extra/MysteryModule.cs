using Discord.Commands;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord;

[Summary("Queues for Mystery Pokémon.")]
public class MysteryModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [Command("MysteryEgg")]
    [Alias("me", "randomegg", "re")]
    [Summary("Makes the bot trade you an egg of a random Pokemon.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task MysteryEggTradeAsync()
    {
        var code = Info.GetRandomTradeCode();
        await MysteryEggTradeAsync(code).ConfigureAwait(false);
    }

    [Command("MysteryEgg")]
    [Alias("me", "randomegg", "re")]
    [Summary("Makes the bot trade you an egg of a random Pokemon.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task MysteryEggTradeAsync([Summary("Trade Code")] int code)
    {
        if (TradeExtensions<T>.HasEggs(Info.Hub.Config.Mode))
        {
            _ = TradeExtensions<T>.GenerateMysteryEgg(Info.Hub.Config.Trade.MysteryShinyOdds, out var pkm);
            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pkm, PokeRoutineType.LinkTrade, PokeTradeType.MysteryEgg, Context.User).ConfigureAwait(false);
        }
        else
        {
            var mode = Info.Hub.Config.Mode;
            await ReplyAsync($"{mode} does not have eggs!").ConfigureAwait(false);
        }
    }

    [Command("MysteryTrade")]
    [Alias("randommon", "rm", "mt")]
    [Summary("Makes the bot trade you a random Pokemon.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task MysteryMonTradeAsync()
    {
        var code = Info.GetRandomTradeCode();
        await MysteryMonTradeAsync(code).ConfigureAwait(false);
    }

    [Command("MysteryTrade")]
    [Alias("randommon", "rm", "mt")]
    [Summary("Makes the bot trade you a random Pokemon.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task MysteryMonTradeAsync([Summary("Trade Code")] int code)
    {
        var sig = Context.User.GetFavor();
        _ = TradeExtensions<T>.GenerateMysteryMon(Info.Hub.Config.Trade.MysteryShinyOdds, out var pkm);
        await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pkm, PokeRoutineType.LinkTrade, PokeTradeType.Specific, Context.User).ConfigureAwait(false);
    }
}
