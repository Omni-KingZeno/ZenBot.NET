using Discord.Commands;
using FuzzySharp;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace SysBot.Pokemon.Discord;

[Summary("Queues for Item Trades.")]
public class ItemTradeModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [Command("itemTrade")]
    [Alias("it", "item")]
    [Summary("Makes the bot trade you a Pokémon holding the requested item.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task ItemTrade([Remainder] string item)
    {
        var code = Info.GetRandomTradeCode();
        await ItemTrade(code, item).ConfigureAwait(false);
    }

    [Command("itemTrade")]
    [Alias("it", "item")]
    [Summary("Makes the bot trade you a Pokémon holding the requested item.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task ItemTrade([Summary("Trade Code")] int code, [Remainder] string item)
    {
        var mode = Info.Hub.Config.Mode;
        var spec = Info.Hub.Config.Trade.ItemTradeSpecies;
        if (mode is ProgramMode.LGPE or ProgramMode.LA)
        {
            await ReplyAsync($"{Context.User.Mention}, Item Trades are not supported in {mode}.").ConfigureAwait(false);
            return;
        }

        var defaultSpecies = mode switch
        {
            ProgramMode.SWSH => Species.Yamper,
            ProgramMode.BDSP => Species.Pelipper,
            ProgramMode.SV   => Species.Greavard,
            _ => Species.Delibird,
        };
        var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
        var personal = GameData.GetPersonal(sav.Version);
        var tradeSpec = spec is Species.None || !personal.IsSpeciesInGame((ushort)spec) ? defaultSpecies : spec;
        item = SpellCheckItem(item);
        var set = new ShowdownSet($"{SpeciesName.GetSpeciesName((ushort)tradeSpec, 2)} @ {item.Trim()}\nShiny: Yes");
        var template = AutoLegalityWrapper.GetTemplate(set);
        var pkm = sav.GetLegal(template, out var result);
        pkm = EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm;

        var la = new LegalityAnalysis(pkm);
        if (pkm is not T pk || !la.Valid)
        {
            var reason = result switch
            {
                LegalizationResult.Timeout => $"That {tradeSpec} set took too long to generate.",
                LegalizationResult.VersionMismatch => "Request refused: PKHeX and Auto-Legality Mod version mismatch.",
                _ => $"I wasn't able to create a {tradeSpec} from that set.",
            };
            var imsg = $"Oops! {reason}";
            await ReplyAsync(imsg).ConfigureAwait(false);
            return;
        }

        if (pk.HeldItem == 0)
        {
            await ReplyAsync("I didn't recognize the requested Item.").ConfigureAwait(false);
            return;
        }

        if (TradeRestrictions.IsUntradableHeld(pk.Context, pk.HeldItem))
        {
            await ReplyAsync("The requested Item cannot be traded.").ConfigureAwait(false);
            return;
        }

        pk.ResetPartyStats();

        var sig = Context.User.GetFavor();
        await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.ItemTrade).ConfigureAwait(false);
    }

    public static string SpellCheckItem(string toCheck)
    {
        var items = GameInfo.GetStrings("en").itemlist;
        var correctedItem = items.OrderByDescending(w => Fuzz.Ratio(toCheck, w)).First();
        return correctedItem;
    }
}
