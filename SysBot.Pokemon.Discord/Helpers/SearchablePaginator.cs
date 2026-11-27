using Discord;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;

namespace SysBot.Pokemon.Discord;

/// <summary>
/// A <see cref="ComponentPaginator"/> that adds a custom Search button and modal to filter pages in place.
/// </summary>
public sealed class SearchablePaginator : ComponentPaginator
{
    public string SearchButtonId { get; }
    private readonly string _searchModalId;
    private readonly string _searchInputId = "search_query";
    private readonly Func<string, int> _onSearch;
    private readonly string _modalTitle;
    private readonly string _inputLabel;
    private readonly string _inputPlaceholder;
    private string _currentQuery = string.Empty;

    public SearchablePaginator(
        IComponentPaginatorBuilder builder,
        Func<string, int> onSearch,
        string modalTitle = "Search",
        string inputLabel = "Species Name",
        string inputPlaceholder = "e.g. Pikachu (leave blank to show all)")
        : base(builder)
    {
        var id = Guid.NewGuid().ToString("N");
        SearchButtonId = $"search_btn:{id}";
        _searchModalId = $"search_modal:{id}";
        _onSearch = onSearch;
        _modalTitle = modalTitle;
        _inputLabel = inputLabel;
        _inputPlaceholder = inputPlaceholder;
    }

    public static MessageComponent BuildNavigationComponents(IComponentPaginator paginator, string searchButtonId)
        => new ComponentBuilder()
            .AddFirstButton(paginator, style: ButtonStyle.Primary, emote: new Emoji("⏮"))
            .AddPreviousButton(paginator, style: ButtonStyle.Primary, emote: new Emoji("◀"))
            .WithButton(new ButtonBuilder()
                .WithCustomId(searchButtonId)
                .WithEmote(new Emoji("🔍"))
                .WithLabel("Search")
                .WithStyle(ButtonStyle.Primary)
                .WithDisabled(paginator, null))
            .AddNextButton(paginator, style: ButtonStyle.Primary, emote: new Emoji("▶"))
            .AddLastButton(paginator, style: ButtonStyle.Primary, emote: new Emoji("⏭"))
            .Build();

    public override bool OwnsComponent(string customId)
        => customId == SearchButtonId || customId == _searchModalId || base.OwnsComponent(customId);

    public override async ValueTask<InteractiveInputStatus> HandleInteractionAsync(IComponentInteraction interaction)
    {
        if (interaction.Data.CustomId != SearchButtonId)
            return await base.HandleInteractionAsync(interaction).ConfigureAwait(false);

        if (Users.Count > 0 && !Users.Any(u => u.Id == interaction.User.Id))
        {
            await interaction.RespondAsync("Only the user who ran this command can use this button.", ephemeral: true).ConfigureAwait(false);
            return InteractiveInputStatus.Ignored;
        }

        var modal = new ModalBuilder()
            .WithTitle(_modalTitle)
            .WithCustomId(_searchModalId)
            .AddTextInput(_inputLabel, _searchInputId, TextInputStyle.Short, _inputPlaceholder, required: false, value: _currentQuery)
            .Build();

        await interaction.RespondWithModalAsync(modal).ConfigureAwait(false);
        return InteractiveInputStatus.Success;
    }

    public override async ValueTask<InteractiveInputStatus> HandleModalInteractionAsync(IModalInteraction interaction)
    {
        if (interaction.Data.CustomId != _searchModalId)
            return await base.HandleModalInteractionAsync(interaction).ConfigureAwait(false);

        var query = interaction.Data.Components
            .FirstOrDefault(c => c.CustomId == _searchInputId)?.Value?.Trim() ?? string.Empty;

        _currentQuery = query;
        PageCount = Math.Max(1, _onSearch(query));
        SetPage(0);

        await RenderPageAsync(interaction, InteractionResponseType.UpdateMessage, false, null).ConfigureAwait(false);
        return InteractiveInputStatus.Success;
    }
}
