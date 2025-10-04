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
                .Select((ev, i) => $"{page * perPage + i + 1}. {(ev.IsShiny ? "★ " : "")}{SpeciesName.GetSpeciesName(ev.Species, 2)}{(string.IsNullOrEmpty(ev.OriginalTrainerName) ? " - Players OT" : $" - {ev.OriginalTrainerName}")}");

            var embed = new EmbedBuilder
            {
                Title = "Available Mystery Gift Pokémon",
                Description = string.Join("\n", chunk),
                Color = Color.DarkPurple
            }.Build();

            embeds.Add(embed);
        }

        await PaginatedMessage.CreateAsync(Context, embeds);
    }

    [Command("getEvent")]
    [Summary("Generates an event Pokemon by its Mystery Gift list number.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task GetEvent(int number)
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

    [Command("getEvent")]
    [Summary("Generates an event Pokemon by its Mystery Gift list number for another player.")]
    [RequireSudo]
    public async Task GetEvent([Summary("Mentioned User")]SocketUser user, [Remainder][Summary("Selection from Event List")]int number)
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
            await ReplyAsync("Failed to generate the requested Pokemon.");
            return;
        }

        var code = Info.GetRandomTradeCode();
        var sig = user.GetFavor();
        await QueueHelper<T>.AddToQueueAsync(Context, code, user.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.Specific, user).ConfigureAwait(false);
    }

    private static T? ConvertMysteryGiftToPKM(DataMysteryGift mg)
    {
        var trainer = AutoLegalityWrapper.GetTrainerInfo<T>();
        var pkm = mg.ConvertToPKM(trainer);
        return EntityConverter.ConvertToType(pkm, typeof(T), out _) as T;
    }

    private static List<dynamic> GetAvailablePokemon()
    {
        List<dynamic> events;

        if (typeof(T) == typeof(PK9))
            events = [.. EncounterEvent.MGDB_G9.Where(e => e.CardType is WC9.GiftType.Pokemon)];
        else if (typeof(T) == typeof(PK8))
            events = [.. EncounterEvent.MGDB_G8.Where(e => e.CardType is WC8.GiftType.Pokemon)];
        else if (typeof(T) == typeof(PA8))
            events = [.. EncounterEvent.MGDB_G8A.Where(e => e.CardType is WA8.GiftType.Pokemon)];
        else if (typeof(T) == typeof(PB8))
            events = [.. EncounterEvent.MGDB_G8B.Where(e => e.CardType is WB8.GiftType.Pokemon)];
        else
            events = [.. EncounterEvent.MGDB_G7GG.Where(e => e.CardType is 0)];

        // Collapse duplicates by species
        return [.. events
            .GroupBy(e => e.Species)
            .Select(g => g.First()).Reverse()];
    }
}

