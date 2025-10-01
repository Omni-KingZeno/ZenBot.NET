using Discord;
using Discord.Commands;
using Discord.Net;
using Discord.WebSocket;
using PKHeX.Core;
using SysBot.Pokemon.Discord.Helpers;

namespace SysBot.Pokemon.Discord;

public static class QueueHelper<T> where T : PKM, new()
{
    private const uint MaxTradeCode = 9999_9999;

    public static async Task AddToQueueAsync(SocketCommandContext context, int code, string trainer, RequestSignificance sig, T trade, PokeRoutineType routine, PokeTradeType type, SocketUser trader)
    {
        if ((uint)code > MaxTradeCode)
        {
            await context.Channel.SendMessageAsync("Trade code should be 00000000-99999999!").ConfigureAwait(false);
            return;
        }

        try
        {
            const string helper = "I've added you to the queue! I'll message you here when your trade is starting.";
            IUserMessage test = await trader.SendMessageAsync(helper).ConfigureAwait(false);

            // Try adding
            var result = AddToTradeQueue(context, trade, code, trainer, sig, routine, type, trader, out var msg, out var embed);

            // Notify in channel
            var hub = SysCord<T>.Runner.Hub;
            if (hub.Config.Discord.UseTradeEmbeds is TradeEmbedDisplay.TradeInitialize)
            {
                _ = embed?.Build();
                embed?.Builder.AddField("** **", msg, inline: false);
                await context.Channel.SendMessageAsync(embed: embed?.Build()).ConfigureAwait(false);
            }
            else
            {
                await context.Channel.SendMessageAsync(msg).ConfigureAwait(false);
            }

            // Notify in PM to mirror what is said in the channel.
            if (typeof(T) == typeof(PB7))
            {
                var codes = PictoCodesExtensions.GetPictoCodesFromLinkCode(code);
                var (attachment, embedPicto) = PictoCodesEmbedBuilder.CreatePictoCodesEmbed(codes);
                await trader.SendFileAsync(attachment, $"{msg}\nYour trade code will be ", false, embedPicto.Build()).ConfigureAwait(false);
            }
            else
            {
                await trader.SendMessageAsync($"{msg}\nYour trade code will be **{code:0000 0000}**.").ConfigureAwait(false);
            }

            // Clean Up
            if (result)
            {
                // Delete the user's join message for privacy
                if (!context.IsPrivate)
                    await context.Message.DeleteAsync(RequestOptions.Default).ConfigureAwait(false);
            }
            else
            {
                // Delete our "I'm adding you!", and send the same message that we sent to the general channel.
                await test.DeleteAsync().ConfigureAwait(false);
            }
        }
        catch (HttpException ex)
        {
            await HandleDiscordExceptionAsync(context, trader, ex).ConfigureAwait(false);
        }
    }

    public static Task AddToQueueAsync(SocketCommandContext context, int code, string trainer, RequestSignificance sig, T trade, PokeRoutineType routine, PokeTradeType type)
    {
        return AddToQueueAsync(context, code, trainer, sig, trade, routine, type, context.User);
    }

    private static bool AddToTradeQueue(SocketCommandContext context, T pk, int code, string trainerName, RequestSignificance sig, PokeRoutineType type, PokeTradeType t, SocketUser trader, out string msg, out TradeEmbedBuilder<T>? embed)
    {
        var user = trader;
        var userID = user.Id;
        var name = NicknameHelper.Get((IGuildUser)trader);

        var trainer = new PokeTradeTrainerInfo(trainerName, userID);
        var notifier = new DiscordTradeNotifier<T>(pk, trainer, code, user, context);
        var detail = new PokeTradeDetail<T>(pk, trainer, notifier, t, code, sig == RequestSignificance.Favored);
        var trade = new TradeEntry<T>(detail, userID, type, name);

        var hub = SysCord<T>.Runner.Hub;
        var Info = hub.Queues.Info;
        var added = Info.AddToTradeQueue(trade, userID, sig == RequestSignificance.Owner);

        if (added == QueueResultAdd.AlreadyInQueue)
        {
            msg = "Sorry, you are already in the queue.";
            embed = null;
            return false;
        }

        var position = Info.CheckPosition(userID, type);

        var ticketID = "";
        if (TradeStartModule<T>.IsStartChannel(context.Channel.Id))
            ticketID = $", unique ID: {detail.ID}";

        var pokeName = "";
        if (hub.Config.Discord.UseTradeEmbeds is TradeEmbedDisplay.None && t == PokeTradeType.Specific && pk.Species != 0)
            pokeName = $" Receiving: {GameInfo.GetStrings("en").Species[pk.Species]}.";
        msg = $"{user.Mention} - Added to the {type} queue{ticketID}. {pokeName} ";

        embed = new TradeEmbedBuilder<T>(pk, hub, new QueueUser(trainer.ID, name));

        if (!(hub.Config.Discord.UseTradeEmbeds is TradeEmbedDisplay.TradeInitialize && t is PokeTradeType.Specific))
        {
            msg += $"Current Position: {position.Position}.";
            var botct = Info.Hub.Bots.Count;
            if (position.Position > botct)
            {
                var eta = Info.Hub.Config.Queues.EstimateDelay(position.Position, botct);
                msg += $" Estimated: {eta:F1} minutes.";
            }
        }

        return true;
    }

    private static async Task HandleDiscordExceptionAsync(SocketCommandContext context, SocketUser trader, HttpException ex)
    {
        var hub = SysCord<T>.Runner.Hub;
        var app = await context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
        var owner = app.Team != null ? app?.Team?.TeamMembers?.FirstOrDefault(member => member.Role == TeamRole.Owner)?.User.Id : app.Owner.Id;
        string message = string.Empty;
        EmbedBuilder embedBuilder = new();
        switch (ex.DiscordCode)
        {
            case DiscordErrorCode.UnknownMessage:
                {
                    // The message was deleted before we could delete it.
                    message = "The message was deleted before I could delete it!";
                    embedBuilder.Title = "Message Deletion Error";
                }
                break;
            case DiscordErrorCode.InsufficientPermissions or DiscordErrorCode.MissingPermissions:
                {
                    // Check if the exception was raised due to missing "Send Messages" or "Manage Messages" permissions. Nag the bot owner if so.
                    var permissions = context.Guild.CurrentUser.GetPermissions(context.Channel as IGuildChannel);
                    if (!permissions.SendMessages)
                    {
                        // Nag the owner in logs.
                        message = "You must grant me \"Send Messages\" permissions!";
                        Base.LogUtil.LogError(message, "QueueHelper");
                        return;
                    }
                    if (!permissions.ManageMessages)
                    {
                        message = "I must be granted \"Manage Messages\" permissions!";
                        embedBuilder.Title = "Permissions Error";
                    }
                }
                break;
            case DiscordErrorCode.CannotSendMessageToUser:
                {
                    // The user either has DMs turned off, or Discord thinks they do.
                    message = context.User == trader ? $"{context.User.Mention}\nYou must enable Direct Messages in order for me to DM your trade code!" : "The mentioned user must enable private messages in order for me to DM them their trade code!";
                    if (context.User == trader)
                        hub.Queues.Info.ClearTrade(context.User.Id);
                    else
                        hub.Queues.Info.ClearTrade(trader.Id);
                    embedBuilder.Title = "Privacy Error";
                }
                break;
            default:
                {
                    // Send a generic error message.
                    message = ex.DiscordCode != null ? $"Discord error {(int)ex.DiscordCode}: {ex.Reason}" : $"Http error {(int)ex.HttpCode}: {ex.Message}";
                }
                break;
        }
        embedBuilder.Description = message;
        embedBuilder.Color = Color.Red;
        embedBuilder.ThumbnailUrl = context.Client.CurrentUser.GetAvatarUrl();
        var pingOwner = ex.DiscordCode == (DiscordErrorCode.InsufficientPermissions | DiscordErrorCode.MissingPermissions);
        var embed = embedBuilder.Build();

        try
        {
            // Get the bots permissions in the channel
            var currentUser = context.Guild.GetUser(context.Client.CurrentUser.Id);
            var channelPerms = currentUser.GetPermissions(context.Channel as IGuildChannel);

            // Check embed links and attach files perms
            bool canSendEmbed = channelPerms.Has(ChannelPermission.EmbedLinks);
            bool canAttachFiles = channelPerms.Has(ChannelPermission.AttachFiles);

            if (!canSendEmbed && !canAttachFiles || !canSendEmbed)
            {
                await context.Message.ReplyAsync(pingOwner ? $"<@{owner}> - {message}" : message).ConfigureAwait(false);
            }
            else
            {
                await context.Message.ReplyAsync(pingOwner ? $"<@{owner}>" : "", false, embed: embed).ConfigureAwait(false);
            }
        }
        catch
        {
            await context.Channel.SendMessageAsync(pingOwner ? $"<@{owner}> {message}" : message).ConfigureAwait(false);
        }
    }
}
