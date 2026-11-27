using Discord;
using Discord.Commands;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;

namespace SysBot.Pokemon.Discord;

public static class PaginationExtensions
{
    private const string FooterIconUrl = "https://i.imgur.com/nXNBrlr.png";

    public static async Task SendPaginatedEmbedsAsync(this InteractiveService interactive, ICommandContext context, IList<Embed> embeds, int timeoutSeconds = 30)
    {
        if (embeds.Count == 0)
        {
            await context.Channel.SendMessageAsync("No pages to display.").ConfigureAwait(false);
            return;
        }

        var pages = new PageBuilder[embeds.Count];
        for (int i = 0; i < embeds.Count; i++)
        {
            pages[i] = PageBuilder.FromEmbed(embeds[i])
                .WithFooter($"Page {i + 1} of {embeds.Count}", FooterIconUrl);
        }

        var paginator = new StaticPaginatorBuilder()
            .AddUser(context.User)
            .WithPages(pages)
            .WithFooter(PaginatorFooter.None)
            .WithInputType(InputType.Buttons)
            .WithActionOnTimeout(ActionOnStop.DisableInput)
            .WithActionOnCancellation(ActionOnStop.DisableInput)
            .AddOption(PaginatorAction.SkipToStart, new Emoji("⏮"), "First", ButtonStyle.Primary)
            .AddOption(PaginatorAction.Backward, new Emoji("◀"), "Prev", ButtonStyle.Primary)
            .AddOption(PaginatorAction.Forward, new Emoji("▶"), "Next", ButtonStyle.Primary)
            .AddOption(PaginatorAction.SkipToEnd, new Emoji("⏭"), "Last", ButtonStyle.Primary)
            .Build();

        await interactive.SendPaginatorAsync(paginator, context.Channel, TimeSpan.FromSeconds(timeoutSeconds), resetTimeoutOnInput: true).ConfigureAwait(false);
    }
}

