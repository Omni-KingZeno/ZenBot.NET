using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Discord.Helpers;
using static System.Net.WebRequestMethods;

namespace SysBot.Pokemon.Discord;

public class TradeStartModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private class TradeStartAction(ulong ChannelId, Action<PokeRoutineExecutorBase, PokeTradeDetail<T>> messager, string channel)
        : ChannelAction<PokeRoutineExecutorBase, PokeTradeDetail<T>>(ChannelId, messager, channel);

    private static readonly Dictionary<ulong, TradeStartAction> Channels = [];
    private static DiscordSocketClient Client = new();

    private static void Remove(TradeStartAction entry)
    {
        Channels.Remove(entry.ChannelID);
        SysCord<T>.Runner.Hub.Queues.Forwarders.Remove(entry.Action);
    }

    public static void RestoreTradeStarting(DiscordSocketClient discord)
    {
        var cfg = SysCordSettings.Settings;
        foreach (var ch in cfg.TradeStartingChannels)
        {
            if (discord.GetChannel(ch.ID) is ISocketMessageChannel c)
                AddLogChannel(c, ch.ID);
        }

        Client = discord;
        LogUtil.LogInfo("Added Trade Start Notification to Discord channel(s) on Bot startup.", "Discord");
    }

    public static bool IsStartChannel(ulong cid)
    {
        return Channels.TryGetValue(cid, out _);
    }

    [Command("startHere")]
    [Summary("Makes the bot log trade starts to the channel.")]
    [RequireSudo]
    public async Task AddLogAsync()
    {
        var c = Context.Channel;
        var cid = c.Id;
        if (Channels.TryGetValue(cid, out _))
        {
            await ReplyAsync("Already logging here.").ConfigureAwait(false);
            return;
        }

        AddLogChannel(c, cid);

        // Add to discord global loggers (saves on program close)
        SysCordSettings.Settings.TradeStartingChannels.AddIfNew([GetReference(Context.Channel)]);
        await ReplyAsync("Added Start Notification output to this channel!").ConfigureAwait(false);
    }

    private static void AddLogChannel(ISocketMessageChannel c, ulong cid)
    {
        void Logger(PokeRoutineExecutorBase bot, PokeTradeDetail<T> detail)
        {
            if (detail.Type == PokeTradeType.Random)
                return;
            if (SysCordSettings.Settings.UseTradeStartEmbeds)
                c.SendMessageAsync(embed: BuildTradeStartEmbed(detail));
            else
                c.SendMessageAsync(GetMessage(bot, detail));
        }

        Action<PokeRoutineExecutorBase, PokeTradeDetail<T>> l = Logger;
        SysCord<T>.Runner.Hub.Queues.Forwarders.Add(l);
        static string GetMessage(PokeRoutineExecutorBase bot, PokeTradeDetail<T> detail) => $"> [{DateTime.Now:hh:mm:ss}] - {bot.Connection.Label} is now trading (ID {detail.ID}) {detail.Trainer.TrainerName}";

        var entry = new TradeStartAction(cid, l, c.Name);
        Channels.Add(cid, entry);
    }

    private static Embed BuildTradeStartEmbed(PokeTradeDetail<T> detail)
    {
        PKMStringWrapper<T> Strings = new(detail.TradeData, SysCord<T>.Runner.Hub.Config.Discord.TradeEmbedSettings, detail.Type);

        var pokemonName = detail.Type switch
        {
            PokeTradeType.ItemTrade  => $"**Sending:** {Strings.HeldItem}",
            PokeTradeType.MysteryEgg => "**Sending:** Mystery Egg",
            PokeTradeType.Clone      => "**Activating:** Cloning Pod",
            PokeTradeType.Dump       => "**Activating:** Pokémon Scanner",
            PokeTradeType.Seed       => "**Activating:** Seed Checker",
            _                        => $"**Sending:** {Strings.Species}{(Strings.HasForm ? $"-{Strings.Form}" : "")}"
        };

        var color = detail.TradeData.Species == 0 ? Color.Purple
            : EmbedColorHelper.GetDiscordColor(detail.TradeData.IsShiny
            ? EmbedColorHelper.ShinyMap[((Species)detail.TradeData.Species, detail.TradeData.Form)]
            : (PersonalColor)detail.TradeData.PersonalInfo.Color);

        var avatarUrl = detail.IsTwitchTrade
            ? "https://raw.githubusercontent.com/Omni-KingZeno/Pokemon-Sprites/refs/heads/main/Bot/Twitch.png"
            : detail.Trainer.ID == 0 ? "https://raw.githubusercontent.com/Omni-KingZeno/Pokemon-Sprites/refs/heads/main/Bot/Discord.png"
            : Client.GetUser(detail.Trainer.ID).GetAvatarUrl();

        var thumbnailUrl = detail.Type == PokeTradeType.ItemTrade ? Strings.GetItemImgURL(Strings.HeldItem, false) : Strings.GetImageURL();
        var footerURL = detail.Type == PokeTradeType.ItemTrade ? Strings.GetImageURL() : detail.TradeData.Species == 0 ? null : Strings.GetBallImageURL();

        return new EmbedBuilder()
            .WithAuthor("Trade Started", iconUrl: avatarUrl)
            .WithDescription($"{pokemonName}\n**Trader:** {detail.Trainer.TrainerName}")
            .WithColor(color)
            .WithFooter($"Trade ID: {detail.ID}", footerURL)
            .WithThumbnailUrl(thumbnailUrl)
            .Build();
    }

    [Command("startInfo")]
    [Summary("Dumps the Start Notification settings.")]
    [RequireSudo]
    public async Task DumpLogInfoAsync()
    {
        foreach (var c in Channels)
            await ReplyAsync($"{c.Key} - {c.Value}").ConfigureAwait(false);
    }

    [Command("startClear")]
    [Summary("Clears the Start Notification settings in that specific channel.")]
    [RequireSudo]
    public async Task ClearLogsAsync()
    {
        var cfg = SysCordSettings.Settings;
        if (Channels.TryGetValue(Context.Channel.Id, out var entry))
            Remove(entry);
        cfg.TradeStartingChannels.RemoveAll(z => z.ID == Context.Channel.Id);
        await ReplyAsync($"Start Notifications cleared from channel: {Context.Channel.Name}").ConfigureAwait(false);
    }

    [Command("startClearAll")]
    [Summary("Clears all the Start Notification settings.")]
    [RequireSudo]
    public async Task ClearLogsAllAsync()
    {
        foreach (var l in Channels)
        {
            var entry = l.Value;
            await ReplyAsync($"Logging cleared from {entry.ChannelName} ({entry.ChannelID}!").ConfigureAwait(false);
            SysCord<T>.Runner.Hub.Queues.Forwarders.Remove(entry.Action);
        }
        Channels.Clear();
        SysCordSettings.Settings.TradeStartingChannels.Clear();
        await ReplyAsync("Start Notifications cleared from all channels!").ConfigureAwait(false);
    }

    private RemoteControlAccess GetReference(IChannel channel) => new()
    {
        ID = channel.Id,
        Name = channel.Name,
        Comment = $"Added by {Context.User.Username} on {DateTime.Now:yyyy.MM.dd-hh:mm:ss}",
    };
}
