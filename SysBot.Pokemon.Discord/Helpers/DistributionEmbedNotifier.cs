using Discord;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Discord.Helpers;

namespace SysBot.Pokemon.Discord;

public sealed class DistributionEmbedNotifier<T>(IPokeTradeNotifier<T> inner, PokeTradeHub<T> hub)
    : IPokeTradeNotifier<T> where T : PKM, new()
{
    public Action<PokeRoutineExecutor<T>>? OnFinish { set => inner.OnFinish = value; }

    private readonly List<IUserMessage> _messages = [];
    private Task _post = Task.CompletedTask;

    public Task TradeInitialize(PokeRoutineExecutor<T> r, PokeTradeDetail<T> info) => inner.TradeInitialize(r, info);

    public async Task TradeSearching(PokeRoutineExecutor<T> r, PokeTradeDetail<T> info)
    {
        await inner.TradeSearching(r, info).ConfigureAwait(false);
        _post = PostAsync(r, info); // fire and forget, tracked so EndAsync can wait for it
    }

    public async Task TradeFinished(PokeRoutineExecutor<T> r, PokeTradeDetail<T> info, T result)
    {
        await inner.TradeFinished(r, info, result).ConfigureAwait(false);
        var trader = !string.IsNullOrWhiteSpace(info.PartnerName)
            ? info.PartnerName
            : result.Species != 0 ? result.OriginalTrainerName : null;
        _ = EndAsync("✅ Distribution completed", trader: trader);
    }

    public async Task TradeCanceled(PokeRoutineExecutor<T> r, PokeTradeDetail<T> info, PokeTradeResult result)
    {
        await inner.TradeCanceled(r, info, result).ConfigureAwait(false);
        var del = result == PokeTradeResult.NoTrainerFound && hub.Config.Distribution.DeleteEmbedIfNoPartner;
        _ = EndAsync("❎ Distribution Ended", del);
    }

    public Task SendNotification(PokeRoutineExecutor<T> r, PokeTradeDetail<T> info, string message)
        => inner.SendNotification(r, info, message);

    public Task SendNotification(PokeRoutineExecutor<T> r, PokeTradeDetail<T> info, PokeTradeSummary message)
        => inner.SendNotification(r, info, message);

    public Task SendNotification(PokeRoutineExecutor<T> r, PokeTradeDetail<T> info, T result, string message)
        => inner.SendNotification(r, info, result, message);

    private async Task PostAsync(PokeRoutineExecutor<T> r, PokeTradeDetail<T> info)
    {
        var pk = info.TradeData;
        var code = typeof(T) == typeof(PB7)
            ? string.Join(" ", info.PictoCodes)
            : $"{info.Code:0000 0000}";
        var ign = string.IsNullOrWhiteSpace(r?.InGameName) ? "Unknown" : r.InGameName;
        var color = EmbedColorHelper.GetDiscordColor(pk.IsShiny ? EmbedColorHelper.ShinyMap[((Species)pk.Species, pk.Form)]
            : (PersonalColor)pk.PersonalInfo.Color);

        var embed = new EmbedBuilder()
            .WithTitle($"✳️ Active Distribution")
            .WithColor(color)
            .AddField("Trade Code", $"**{code}**", true)
            .AddField("Bot IGN", ign, true)
            .AddField("Details", $"```{ShowdownParsing.GetShowdownText(pk)}```")
            .WithFooter("Searching now...")
            .WithThumbnailUrl(TradeExtensions<T>.GetPokemonImageURL(pk, false, true, pk.IsEgg))
            .Build();

        foreach (var channel in hub.Config.Distribution.DistributionEmbedChannels)
        {
            if (DistributionEmbeds.GetChannel(channel.ID) is not { } ch) continue;
            try
            {
                var msg = await ch.SendMessageAsync(embed: embed).ConfigureAwait(false);
                _messages.Add(msg);
            }
            catch (Exception ex) { LogUtil.LogError($"Distribution embed post failed: {ex.Message}", nameof(DistributionEmbedNotifier<>)); }
        }
    }

    private async Task EndAsync(string status, bool delete = false, string? trader = null)
    {
        try { await _post.ConfigureAwait(false); } catch { /* logged in PostAsync */ }

        foreach (var m in _messages)
        {
            try
            {
                if (delete)
                    await m.DeleteAsync().ConfigureAwait(false);
                else
                    await m.ModifyAsync(p => p.Embed = DistributionEmbeds.BuildEnded(m, status, trader)).ConfigureAwait(false);
            }
            catch (Exception ex) { LogUtil.LogError($"Distribution embed update failed: {ex.Message}", nameof(DistributionEmbedNotifier<T>)); }
        }
        _messages.Clear();
    }
}
