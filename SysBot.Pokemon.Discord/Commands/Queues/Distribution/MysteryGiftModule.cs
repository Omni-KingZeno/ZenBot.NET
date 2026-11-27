using Discord;
using Discord.Commands;
using Fergun.Interactive;
using Fergun.Interactive.Pagination;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord;

[Summary("Queues for Mystery Gift Pokémon.")]
public class MysteryGiftModule<T>(InteractiveService Interactive) : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private const string FooterIconUrl = "https://i.imgur.com/nXNBrlr.png";
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [Command("listMysteryGifts")]
    [Alias("eventList", "lmg")]
    [Summary("Lists available Mystery Gift pokemon for the current game.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task ListEvents()
    {
        const int perPage = 10;
        var allEntries = GetAvailablePokemon()
            .Select((ev, i) => (Number: i + 1, Gift: (DataMysteryGift)ev))
            .ToList();

        if (allEntries.Count == 0)
        {
            await ReplyAsync("No Mystery Gifts available for this game.").ConfigureAwait(false);
            return;
        }

        var filtered = allEntries;
        string activeQuery = string.Empty;
        SearchablePaginator? paginator = null;

        int initialPages = (int)Math.Ceiling(allEntries.Count / (double)perPage);

        var builder = new ComponentPaginatorBuilder()
            .AddUser(Context.User)
            .WithPageCount(initialPages)
            .WithActionOnTimeout(ActionOnStop.DisableInput)
            .WithActionOnCancellation(ActionOnStop.DisableInput)
            .WithPageFactory(p => BuildMysteryGiftPage(p, paginator!.SearchButtonId, filtered, activeQuery, perPage));

        paginator = new SearchablePaginator(
            builder,
            onSearch: query =>
            {
                activeQuery = query;
                var spec = Normalize(query);
                filtered = spec.Length == 0
                    ? allEntries
                    : allEntries.Where(e => Normalize(GetSpeciesDisplayName(e.Gift)).Contains(spec)).ToList();

                return (int)Math.Ceiling(filtered.Count / (double)perPage);
            },
            modalTitle: "Search Mystery Gifts",
            inputLabel: "Pokémon Species Name",
            inputPlaceholder: "e.g. Pikachu (leave blank to show all)");

        await Interactive.SendPaginatorAsync(paginator, Context.Channel, TimeSpan.FromSeconds(30), resetTimeoutOnInput: true).ConfigureAwait(false);
    }

    private static IPage BuildMysteryGiftPage(
        IComponentPaginator paginator,
        string searchButtonId,
        List<(int Number, DataMysteryGift Gift)> entries,
        string activeQuery,
        int perPage)
    {
        var pageBuilder = new PageBuilder()
            .WithColor(Color.DarkPurple)
            .WithFooter($"Page {paginator.CurrentPageIndex + 1} of {paginator.PageCount}", FooterIconUrl);

        if (entries.Count == 0)
        {
            pageBuilder.WithDescription($"No Mystery Gifts matched **\"{activeQuery}\"**.\nClick **Search** and submit an empty box to view all.");
        }
        else
        {
            var chunk = entries
                .Skip(paginator.CurrentPageIndex * perPage)
                .Take(perPage)
                .ToList();

            var leftColumn = chunk.Take(5).ToList();
            var rightColumn = chunk.Skip(5).Take(5).ToList();

            var leftText = string.Join("\n", leftColumn.Select(e => FormatEntry(e.Number, e.Gift)));
            var header = string.IsNullOrEmpty(activeQuery)
                ? "__Available Mystery Gift Pokémon__"
                : $"__Search: \"{activeQuery}\" ({entries.Count})__";

            pageBuilder.AddField(header, Truncate(leftText), inline: true);

            if (rightColumn.Count > 0)
            {
                var rightText = string.Join("\n", rightColumn.Select(e => FormatEntry(e.Number, e.Gift)));
                pageBuilder.AddField("\u200B", Truncate(rightText), inline: true);
            }
        }

        var components = SearchablePaginator.BuildNavigationComponents(paginator, searchButtonId);

        return pageBuilder.WithComponents(components).Build();
    }

    [Command("getEvent")]
    [Summary("Generates an event Pokemon by its Mystery Gift list number.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task GetEvent([Summary("Selection from Event List")] int number)
    {
        var availablePokemon = GetAvailablePokemon();

        if (number < 1 || number > availablePokemon.Count)
        {
            await ReplyAsync("Invalid choice.");
            return;
        }

        var ev = availablePokemon[number - 1];
        var pk = ConvertMysteryGiftToPKM(ev);
        if (pk == null)
        {
            await ReplyAsync("Failed to generate the Pokemon for this event.");
            return;
        }

        var code = Info.GetRandomTradeCode();
        var sig = Context.User.GetFavor();
        await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.Specific, Context.User).ConfigureAwait(false);
    }

    [Command("giveEvent")]
    [Summary("Generates an event Pokemon by its Mystery Gift list number for another player.")]
    [RequireSudo]
    public async Task GiveEvent([Summary("Mentioned User")] string target, [Remainder][Summary("Selection from Event List")] int number)
    {
        var botId = Context.Client.CurrentUser.Id;
        IUser? user = Context.Message.MentionedUsers.FirstOrDefault(u => u.Id != botId);
        if (user is null &&
            (MentionUtils.TryParseUser(target.Trim(), out ulong userId) || ulong.TryParse(target.Trim(), out userId)))
        {
            user = await Context.Client.GetUserAsync(userId).ConfigureAwait(false);
        }
        if (user is null)
        {
            await ReplyAsync("A valid user must be mentioned (or user ID provided) in order to do this.").ConfigureAwait(false);
            return;
        }

        var availablePokemon = GetAvailablePokemon();

        if (number < 1 || number > availablePokemon.Count)
        {
            await ReplyAsync("Invalid choice.");
            return;
        }

        var ev = availablePokemon[number - 1];
        var pk = ConvertMysteryGiftToPKM(ev);
        if (pk == null)
        {
            await ReplyAsync("Failed to generate the requested Pokemon.");
            return;
        }

        var code = Info.GetRandomTradeCode();
        var sig = user.GetFavor();
        await QueueHelper<T>.AddToQueueAsync(Context, code, user.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.Specific, user).ConfigureAwait(false);
    }

    private static string FormatEntry(int number, DataMysteryGift mg) =>
        $"{number}. **{(mg.IsShiny ? "★ " : "")}{GetSpeciesDisplayName(mg)}**\n" +
        $"  -# - **OT:** {(string.IsNullOrEmpty(mg.OriginalTrainerName) ? "Players" : mg.OriginalTrainerName)}\n" +
        $"  -# - **Date:** {GetDistributionDate(mg)}\n" +
        $"  -# - **Wondercard ID:** {mg.CardID:0000}\n";

    private static string Truncate(string text) => text.Length > 1024 ? text[..1024] : text;

    private static string Normalize(string text) =>
        string.Concat(text.Where(char.IsLetterOrDigit)).ToLowerInvariant();

    private static T? ConvertMysteryGiftToPKM(DataMysteryGift mg)
    {
        var trainer = AutoLegalityWrapper.GetTrainerInfo<T>();
        var pkm = mg.ConvertToPKM(trainer);
        if (TradeRestrictions.IsUntradableHeld(pkm.Context, pkm.HeldItem))
            pkm.HeldItem = 0;
        return EntityConverter.ConvertToType(pkm, typeof(T), out _) as T;
    }

    private static List<dynamic> GetAvailablePokemon()
    {
        List<dynamic> events;

        if (typeof(T) == typeof(PA9))
            events = [.. EncounterEvent.MGDB_G9A.Where(e => e.CardType is WA9.GiftType.Pokemon)];
        else if (typeof(T) == typeof(PK9))
            events = [.. EncounterEvent.MGDB_G9.Where(e => e.CardType is WC9.GiftType.Pokemon)];
        else if (typeof(T) == typeof(PK8))
            events = [.. EncounterEvent.MGDB_G8.Where(e => e.CardType is WC8.GiftType.Pokemon)];
        else if (typeof(T) == typeof(PA8))
            events = [.. EncounterEvent.MGDB_G8A.Where(e => e.CardType is WA8.GiftType.Pokemon)];
        else if (typeof(T) == typeof(PB8))
            events = [.. EncounterEvent.MGDB_G8B.Where(e => e.CardType is WB8.GiftType.Pokemon)];
        else if (typeof(T) == typeof(PB7))
            events = [.. EncounterEvent.MGDB_G7GG.Where(e => e.CardType is 0)];
        else
            events = [];

        // Collapse duplicates by species + form (different forms are kept as separate entries)
        return [.. events
            .GroupBy(e => ((int)e.Species, (int)e.Form))
            .Select(g => g.First())
            .OrderBy(e => GetDistributionDate(e))
            .Reverse()];
    }

    private static string GetSpeciesDisplayName(dynamic ev)
    {
        DataMysteryGift mg = ev;
        string name = SpeciesName.GetSpeciesName(mg.Species, 2);

        if (mg.Form == 0)
            return name;

        var strings = GameInfo.Strings;
        var forms = FormConverter.GetFormList(mg.Species, strings.types, strings.forms, GameInfo.GenderSymbolASCII, mg.Context);
        string formName = mg.Form < forms.Length ? forms[mg.Form] : mg.Form.ToString();

        return string.IsNullOrWhiteSpace(formName) ? name : $"{name}-{formName}";
    }

    private static string GetDistributionDate(dynamic ev)
    {
        DateOnly? startDate = null;

        if (ev is WA9 wa9 && wa9.GetDistributionWindow(out var window9a))
            startDate = window9a.Start;
        else if (ev is WC9 wc9 && wc9.GetDistributionWindow(out var window9))
            startDate = window9.Start;
        else if (ev is WC8 wc8 && wc8.GetDistributionWindow(out var window8))
            startDate = window8.Start;
        else if (ev is WA8 wa8 && wa8.GetDistributionWindow(out var window8a))
            startDate = window8a.Start;
        else if (ev is WB8 wb8 && wb8.GetDistributionWindow(out var window8b))
            startDate = window8b.Start;
        else if (ev is WB7 wb7 && wb7.GetDistributionWindow(out var window7))
            startDate = window7.Start;

        return startDate.HasValue ? startDate.Value.ToString("yy-MM-dd") : "No Date";
    }
}
