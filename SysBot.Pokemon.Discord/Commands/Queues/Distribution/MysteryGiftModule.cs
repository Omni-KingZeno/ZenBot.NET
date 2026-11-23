using Discord;
using Discord.Commands;
using Discord.WebSocket;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord;

[Summary("Queues for Mystery Gift Pokémon.")]
public class MysteryGiftModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [Command("listMysteryGifts")]
    [Alias("eventList", "lmg")]
    [Summary("Lists available Mystery Gift pokemon for the current game.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task ListEvents()
    {
        var availablePokemon = GetAvailablePokemon();

        var embeds = new List<Embed>();
        int total = availablePokemon.Count;
        int perPage = 20;
        int pages = (int)Math.Ceiling(total / (double)perPage);

        for (int page = 0; page < pages; page++)
        {
            var chunk = availablePokemon
                .Skip(page * perPage)
                .Take(perPage)
                .ToList();

            var embedBuilder = new EmbedBuilder
            {
                Color = Color.DarkPurple
            };

            var leftColumn = chunk.Take(10).ToList();
            var rightColumn = chunk.Skip(10).Take(10).ToList();

            var leftColumnText = string.Join("\n", leftColumn.Select((ev, i) =>
            {
                string dateStr = GetDistributionDate(ev);
                int number = page * perPage + i + 1;
                return $"{number}. **{(ev.IsShiny ? "★ " : "")}{GetSpeciesDisplayName(ev)}**\n" +
                $"  -# - **OT:** {(string.IsNullOrEmpty(ev.OriginalTrainerName) ? "Players" : ev.OriginalTrainerName)}\n" +
                $"  -# - **Date:** {dateStr}\n" +
                $"  -# - **Wondercard ID:** {ev.CardID.ToString("0000")}\n";
            }));

            var rightColumnText = string.Join("\n", rightColumn.Select((ev, i) =>
            {
                string dateStr = GetDistributionDate(ev);
                int number = page * perPage + 10 + i + 1;
                return $"{number}. **{(ev.IsShiny ? "★ " : "")}{GetSpeciesDisplayName(ev)}**\n" +
                $"  -# - **OT:** {(string.IsNullOrEmpty(ev.OriginalTrainerName) ? "Players" : ev.OriginalTrainerName)}\n" +
                $"  -# - **Date:** {dateStr}\n" +
                $"  -# - **Wondercard ID:** {ev.CardID.ToString("0000")}\n";
            }));

            if (leftColumnText.Length > 1024)
                leftColumnText = leftColumnText[..1024];

            embedBuilder.AddField("__Available Mystery Gift Pokémon__", leftColumnText, inline: true);

            if (rightColumn.Count > 0)
            {
                if (rightColumnText.Length > 1024)
                    rightColumnText = rightColumnText[..1024];

                embedBuilder.AddField("\u200B", rightColumnText, inline: true);
            }

            embeds.Add(embedBuilder.Build());
        }

        await PaginatedMessage.CreateAsync(Context, embeds);
    }

    [Command("getEvent")]
    [Summary("Generates an event Pokemon by its Mystery Gift list number.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task GetEvent([Summary("Selection from Event List")]int number)
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

    [Command("eventSearch")]
    [Alias("es", "searchEvent")]
    [Summary("Searches the Mystery Gift list for a Pokémon and shows its <GetEvent> number.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task SearchEvents([Remainder][Summary("Species name")] string species)
    {
        var spec = Normalize(species);
        if (spec.Length == 0)
        {
            await ReplyAsync("Please provide a Pokémon name to search for.").ConfigureAwait(false);
            return;
        }

        var available = GetAvailablePokemon();
        var matches = new List<string>();

        for (int i = 0; i < available.Count; i++)
        {
            DataMysteryGift mg = available[i];
            if (!Normalize(GetSpeciesDisplayName(mg)).Contains(spec))
                continue;

            matches.Add(FormatEntry(i + 1, mg));
        }

        if (matches.Count == 0)
        {
            await ReplyAsync("No Mystery Gifts matched that name in this game.").ConfigureAwait(false);
            return;
        }

        var embeds = new List<Embed>();
        foreach (var page in matches.Chunk(20))
        {
            var builder = new EmbedBuilder
            {
                Color = Color.DarkPurple,
                Footer = new EmbedFooterBuilder { Text = $"{matches.Count} result(s). Use getEvent <number> to request one." },
            };

            var left = string.Join("\n", page.Take(10));
            var right = string.Join("\n", page.Skip(10));

            builder.AddField("__Search Results__", Truncate(left), inline: true);
            if (right.Length > 0)
                builder.AddField("\u200B", Truncate(right), inline: true);

            embeds.Add(builder.Build());
        }

        await PaginatedMessage.CreateAsync(Context, embeds).ConfigureAwait(false);
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
