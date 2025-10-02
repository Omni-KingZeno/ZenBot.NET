using Discord;
using Discord.Commands;
using Discord.WebSocket;

namespace SysBot.Pokemon.Discord;

public static class ReactionUtil
{
    private static readonly Dictionary<ulong, Func<SocketReaction, Task>> Handlers = [];

    public static async Task OnReactionAddedAsync(Cacheable<IUserMessage, ulong> cachedMessage, Cacheable<IMessageChannel, ulong> _, SocketReaction reaction)
    {
        var message = await cachedMessage.GetOrDownloadAsync();
        if (reaction.UserId == message.Author.Id)
            return;

        if (Handlers.TryGetValue(reaction.MessageId, out var handler))
            await handler(reaction);
    }

    public static void AddHandler(ulong messageId, Func<SocketReaction, Task> handler)
        => Handlers[messageId] = handler;

    public static void RemoveHandler(ulong messageId)
        => Handlers.Remove(messageId);
}

/// <summary>
/// Handles paginated embeds with reaction navigation.
/// </summary>
public class PaginatedMessage
{
    private readonly IUserMessage Message;
    private readonly IList<Embed> Pages;
    private readonly ulong UserId;
    private int Page = 0;
    private DateTime LastActivity = DateTime.Now;

    private static readonly Emoji First = new("⏪");
    private static readonly Emoji Prev = new("⬅️");
    private static readonly Emoji Next = new("➡️");
    private static readonly Emoji Last = new("⏩");
    private static readonly IEmote[] Controls = [First, Prev, Next, Last];

    private PaginatedMessage(IUserMessage message, IList<Embed> pages, ulong userId)
    {
        Message = message;
        Pages = pages;
        UserId = userId;
    }

    public static async Task CreateAsync(ICommandContext ctx, IList<Embed> pages, int timeoutSeconds = 20)
    {
        if (pages.Count == 0)
        {
            await ctx.Channel.SendMessageAsync("No pages to display.").ConfigureAwait(false);
            return;
        }

        if (ctx.Channel is not IGuildChannel guildChannel)
        {
            await ctx.Channel.SendMessageAsync("I can only use reactions in server channels. Displaying only the first page.", embed: pages[0]).ConfigureAwait(false);
            return;
        }

        var botUser = await ctx.Guild.GetCurrentUserAsync().ConfigureAwait(false);
        var perms = botUser.GetPermissions(guildChannel);

        if (!perms.AddReactions)
        {
            await ctx.Channel.SendMessageAsync("I am missing the required \"Add Reactions\" Permissions. Displaying only the first page.", embed: pages[0]).ConfigureAwait(false);
            return;
        }

        var msg = await ctx.Channel.SendMessageAsync(embed: pages[0]).ConfigureAwait(false);
        foreach (var emote in Controls)
            await msg.AddReactionAsync(emote).ConfigureAwait(false);

        var paginator = new PaginatedMessage(msg, pages, ctx.User.Id);
        ReactionUtil.AddHandler(msg.Id, paginator.HandleReaction);

        _ = paginator.MonitorAsync(timeoutSeconds);
    }

    private async Task HandleReaction(SocketReaction reaction)
    {
        if (reaction.UserId != UserId) return;

        switch (reaction.Emote.Name)
        {
            case "⬅️" when Page > 0: Page--; break;
            case "➡️" when Page < Pages.Count - 1: Page++; break;
            case "⏪": Page = 0; break;
            case "⏩": Page = Pages.Count - 1; break;
            default: return;
        }

        await Message.ModifyAsync(m => m.Embed = Pages[Page]).ConfigureAwait(false);
        await Message.RemoveReactionAsync(reaction.Emote, reaction.User.Value).ConfigureAwait(false);
        LastActivity = DateTime.Now;
    }

    private async Task MonitorAsync(int timeoutSeconds)
    {
        while (true)
        {
            await Task.Delay(1000).ConfigureAwait(false);
            if ((DateTime.Now - LastActivity).TotalSeconds > timeoutSeconds)
            {
                ReactionUtil.RemoveHandler(Message.Id);
                await Message.RemoveAllReactionsAsync().ConfigureAwait(false);
                break;
            }
        }
    }
}
