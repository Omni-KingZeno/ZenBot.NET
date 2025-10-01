using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord;

public class DiscordTradeNotifier<T>(T Data, PokeTradeTrainerInfo Info, int Code, SocketUser Trader, SocketCommandContext Context)
    : IPokeTradeNotifier<T>
    where T : PKM, new()
{
    public Action<PokeRoutineExecutor<T>>? OnFinish { private get; set; }
    public readonly PokeTradeHub<T> Hub = SysCord<T>.Runner.Hub;

    public void TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        // Notify user in private messages
        var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";

        if (typeof(T) == typeof(PB7))
        {
            var (attachment, embed) = PictoCodesEmbedBuilder.CreatePictoCodesEmbed(info.PictoCodes);
            Trader.SendFileAsync(attachment, $"Initializing trade{receive}. Please be ready. Your code is ", false, embed.Build()).ConfigureAwait(false);
            return;
        }
        Trader.SendMessageAsync($"Initializing trade{receive}. Please be ready. Your code is **{Code:0000 0000}**.").ConfigureAwait(false);
    }

    public void TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        var name = Info.TrainerName;
        var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", {name}";

        if (typeof(T) == typeof(PB7))
        {
            var (attachment, embed) = PictoCodesEmbedBuilder.CreatePictoCodesEmbed(info.PictoCodes);
            Trader.SendFileAsync(attachment, $"I'm waiting for you{trainer}! Your code is ", false, embed.Build()).ConfigureAwait(false);
            return;
        }
        Trader.SendMessageAsync($"I'm waiting for you{trainer}! Your code is **{Code:0000 0000}**. My IGN is **{routine.InGameName}**.").ConfigureAwait(false);
    }

    public void TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
    {
        OnFinish?.Invoke(routine);
        Trader.SendMessageAsync($"Trade canceled: {msg}").ConfigureAwait(false);
    }

    public void TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
    {
        OnFinish?.Invoke(routine);
        var tradedToUser = Data.Species;
        var message = tradedToUser != 0 ? $"Trade finished. Enjoy your {(Species)tradedToUser}!" : "Trade finished!";
        Trader.SendMessageAsync(message).ConfigureAwait(false);

        if (result.Species != 0 && Hub.Config.Discord.ReturnPKMs)
            Trader.SendPKMAsync(result, "Here's what you traded me!").ConfigureAwait(false);

        if (Hub.Config.Discord.UseTradeEmbeds is TradeEmbedDisplay.TradeComplete && info.Type is PokeTradeType.Specific)
        {
            var embed = new TradeEmbedBuilder<T>(Data, Hub, new QueueUser(Info.ID, Trader.Username));
            Context.Channel.SendMessageAsync("", embed: embed.Build()).ConfigureAwait(false);
        }
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
    {
        Trader.SendMessageAsync(message).ConfigureAwait(false);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary message)
    {
        if (message.ExtraInfo is SeedSearchResult r)
        {
            SendNotificationZ3(r);
            return;
        }

        var msg = message.Summary;
        if (message.Details.Count > 0)
            msg += ", " + string.Join(", ", message.Details.Select(z => $"{z.Heading}: {z.Detail}"));
        Trader.SendMessageAsync(msg).ConfigureAwait(false);
    }

    public void SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
    {
        if (result.Species != 0 && (Hub.Config.Discord.ReturnPKMs || info.Type == PokeTradeType.Dump))
            Trader.SendPKMAsync(result, message).ConfigureAwait(false);
    }

    private void SendNotificationZ3(SeedSearchResult r)
    {
        var lines = r.ToString();

        var embed = new EmbedBuilder { Color = Color.LighterGrey };
        embed.AddField(x =>
        {
            x.Name = $"Seed: {r.Seed:X16}";
            x.Value = lines;
            x.IsInline = false;
        });
        var msg = $"Here are the details for `{r.Seed:X16}`:";
        Trader.SendMessageAsync(msg, embed: embed.Build()).ConfigureAwait(false);
    }
}
