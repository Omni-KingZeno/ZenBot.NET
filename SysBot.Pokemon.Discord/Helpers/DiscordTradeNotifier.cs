using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon.Discord;

public sealed record DiscordTradeNotifier<T>(T Data, PokeTradeTrainerInfo Info, int Code, SocketUser Trader, SocketCommandContext Context)
    : IPokeTradeNotifier<T>
    where T : PKM, new()
{
    private T Data { get; } = Data;
    private PokeTradeTrainerInfo Info { get; } = Info;
    private int Code { get; } = Code;
    public Action<PokeRoutineExecutor<T>>? OnFinish { private get; set; }
    public readonly PokeTradeHub<T> Hub = SysCord<T>.Runner.Hub;

    public async Task TradeInitialize(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        var receive = Data.Species == 0 ? string.Empty : $" ({Data.Nickname})";
        var code = Format.Bold($"{Code:0000 0000}");
        var message = $"Initializing trade{receive}. Please be ready. Your code is {code}.";

        if (typeof(T) == typeof(PB7))
        {
            var (attachment, embed) = PictoCodesEmbedBuilder.CreatePictoCodesEmbed(info.PictoCodes);
            await Trader.SendFileAsync(attachment, $"Initializing trade{receive}. Please be ready. Your code is ", false, embed.Build()).ConfigureAwait(false);
            return;
        }
        await SendNotification(message).ConfigureAwait(false);
    }

    public async Task TradeSearching(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info)
    {
        var name = Info.TrainerName;
        var trainer = string.IsNullOrEmpty(name) ? string.Empty : $", {name}";
        var code = Format.Bold($"{Code:0000 0000}");
        var myName = Format.Bold(routine.InGameName);
        var message = $"I'm waiting for you{trainer}! Your code is {code}. My IGN is {myName}.";

        if (typeof(T) == typeof(PB7))
        {
            var (attachment, embed) = PictoCodesEmbedBuilder.CreatePictoCodesEmbed(info.PictoCodes);
            await Trader.SendFileAsync(attachment, $"I'm waiting for you{trainer}! Your code is ", false, embed.Build()).ConfigureAwait(false);
            return;
        }
        await SendNotification(message).ConfigureAwait(false);
    }

    public async Task TradeCanceled(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeResult msg)
    {
        OnFinish?.Invoke(routine);
        var message = $"Trade canceled: {msg}";

        await SendNotification(message).ConfigureAwait(false);
    }

    public async Task TradeFinished(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result)
    {
        OnFinish?.Invoke(routine);
        var tradedToUser = Data.Species;
        var message = tradedToUser != 0 ? $"Trade finished. Enjoy your {(Species)tradedToUser}!" : "Trade finished!";

        await SendNotification(message).ConfigureAwait(false);
        if (result.Species != 0 && Hub.Config.Discord.ReturnPKMs)
            await Trader.SendPKMAsync(result, "Here's what you traded me!").ConfigureAwait(false);

        LogUtil.LogInfo($"Total time since queueing: {info.Age:g}");
        if (info.Age < TimeSpan.FromMinutes(15) && Hub.Config.Discord.UseTradeEmbeds is TradeEmbedDisplay.TradeComplete && info.Type is PokeTradeType.Specific)
        {
            var embed = new TradeEmbedBuilder<T>(Data, Hub, new QueueUser(Info.ID, Trader.Username, string.Empty), PokeRoutineType.LinkTrade, info.Type);
            await Context.Channel.SendMessageAsync("", embed: embed.Build()).ConfigureAwait(false);
        }
    }

    public async Task SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, string message)
        => await SendNotification(message).ConfigureAwait(false);

    private async Task SendNotification(string message, Embed? embed = null)
    {
        // Discord makes all interaction modals stale after 15 minutes.
        // Depending on how long we take to start and complete the trade (queued users), this might be called >= 15 minutes after command issued.
        // So, we do the standard behavior: direct message the user.
        await Trader.SendMessageAsync(message, embed: embed).ConfigureAwait(false);
    }

    public async Task SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, PokeTradeSummary trade)
    {
        if (trade.ExtraInfo is SeedSearchResult r)
        {
            await SendNotificationZ3(r).ConfigureAwait(false);
            return;
        }

        var message = trade.Summary;
        if (trade.Details.Count > 0)
            message += ", " + string.Join(", ", trade.Details.Select(z => $"{z.Heading}: {z.Detail}"));

        await SendNotification(message).ConfigureAwait(false);
    }

    public async Task SendNotification(PokeRoutineExecutor<T> routine, PokeTradeDetail<T> info, T result, string message)
    {
        if (result.Species != 0 && (Hub.Config.Discord.ReturnPKMs || info.Type == PokeTradeType.Dump))
            await Trader.SendPKMAsync(result, message).ConfigureAwait(false);
    }

    private async Task SendNotificationZ3(SeedSearchResult searchResult)
    {
        var message = $"Here are the details for `{searchResult.Seed:X16}`:";
        var embed = new EmbedBuilder { Color = Color.LighterGrey };
        embed.AddField(x =>
        {
            x.Name = $"Seed: {searchResult.Seed:X16}";
            x.Value = searchResult.ToString();
            x.IsInline = false;
        });

        // Seed check might be more than 15 minutes stale. Just DM them, not like the public needs to see their seeds.
        await SendNotification(message, embed: embed.Build()).ConfigureAwait(false);
    }
}
