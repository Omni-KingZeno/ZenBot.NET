using Discord.Commands;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord;

[Summary("Special Requests Commands.")]
public class SpecialRequestModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [Command("SpecialRequest")]
    [Alias("sr")]
    [Summary("Adds the user to the Special Request Queue")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task SpecialRequestAsync([Summary("Trade Code")] int code)
    {
        var sig = Context.User.GetFavor();
        await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, new T(), PokeRoutineType.LinkTrade, PokeTradeType.SpecialRequest, Context.User).ConfigureAwait(false);
    }

    [Command("SpecialRequest")]
    [Alias("sr")]
    [Summary("Adds the user to the Special Request Queue.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task SpecialRequestAsync()
    {
        var code = Info.GetRandomTradeCode();
        await SpecialRequestAsync(code).ConfigureAwait(false);
    }
}
