using Discord.Commands;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace SysBot.Pokemon.Discord;

[Summary("Queues for Mystery Pokémon.")]
public class MysteryModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [Command("MysteryEgg")]
    [Alias("me", "randomegg", "re")]
    [Summary("Makes the bot trade you an egg of a random Pokemon.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task MysteryEggTradeAsync()
    {
        var code = Info.GetRandomTradeCode();
        await MysteryEggTradeAsync(code).ConfigureAwait(false);
    }

    [Command("MysteryEgg")]
    [Alias("me", "randomegg", "re")]
    [Summary("Makes the bot trade you an egg of a random Pokemon.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task MysteryEggTradeAsync([Summary("Trade Code")] int code)
    {
        if (typeof(T) != typeof(PA8) && typeof(T) != typeof(PB7))
        {
            _ = MysteryEgg(out var pk);
            var sig = Context.User.GetFavor();
            await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.MysteryEgg, Context.User).ConfigureAwait(false);
        }
        else
        {
            await ReplyAsync($"{(typeof(T) == typeof(PA8) ? "PLA" : "LGPE")} does not have eggs!").ConfigureAwait(false);
        }
    }

    [Command("MysteryTrade")]
    [Alias("randommon", "rm", "mt")]
    [Summary("Makes the bot trade you a random Pokemon.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task MysteryMonTradeAsync()

    {
        var code = Info.GetRandomTradeCode();
        await MysteryMonTradeAsync(code).ConfigureAwait(false);
    }

    [Command("MysteryTrade")]
    [Alias("randommon", "rm", "mt")]
    [Summary("Makes the bot trade you a random Pokemon.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task MysteryMonTradeAsync([Summary("Trade Code")] int code)
    {
        var sig = Context.User.GetFavor();
        var trainer = AutoLegalityWrapper.GetTrainerInfo<T>();
        var sav = BlankSaveFile.Get(trainer.Version, trainer.OT);
        var availSpec = Enumerable.Range(0, sav.Personal.MaxSpeciesID).Where(i => sav.Personal.IsSpeciesInGame((ushort)i)).Select(i => (ushort)i).ToList();

        while (true)
        {
            var species = availSpec[Util.Rand.Next(availSpec.Count)];
            var shiny = Util.Rand.Next(0, Info.Hub.Config.Trade.MysteryShinyOdds) == 0;
            var template = new RegenTemplate(new ShowdownSet($"{(Species)species}"));
            var pkm = (T)sav.GetLegal(template, out _);
            pkm.OriginalTrainerTrash.Clear();
            pkm.OriginalTrainerName = "Surprise!";
            pkm.SetSuggestedMoves();
            pkm.SetNature((Nature)Util.Rand.Next(0, 24));
            pkm.SetRandomIVs();
            pkm.SetAbility(Util.Rand.Next(0, 2));
            pkm.Ball = (byte)Util.Rand.Next(1, 26);

            if (pkm is IDynamaxLevel d)
                d.DynamaxLevel = (byte)Util.Rand.Next(0, 10);

            if (pkm is ITeraType t)
                t.TeraTypeOverride = (MoveType)Util.Rand.Next(0, TeraTypeUtil.MaxType + 1);

            if (shiny)
                pkm.SetShiny();

            var la = new LegalityAnalysis(pkm);
            if (!la.Valid)
                continue;

            pkm = (T)(EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm);
            pkm.ResetPartyStats();
            
            await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pkm, PokeRoutineType.LinkTrade, PokeTradeType.Specific, Context.User).ConfigureAwait(false);

            return;
        }        
    }

    private static T MysteryEgg(out T pkm)
    {
        var trainer = AutoLegalityWrapper.GetTrainerInfo<T>();
        var sav = BlankSaveFile.Get(trainer.Version, trainer.OT);
        var availSpec = Enumerable.Range(0, sav.Personal.MaxSpeciesID)
                .Where(i => sav.Personal.IsSpeciesInGame((ushort)i) && Breeding.CanHatchAsEgg((ushort)i))
                .Select(i => (ushort)i)
                .ToList();

        while (true)
        {
            var species = availSpec[Util.Rand.Next(availSpec.Count)];
            var shiny = Util.Rand.Next(0, Info.Hub.Config.Trade.MysteryShinyOdds) == 0;
            var template = new RegenTemplate(new ShowdownSet($"{(Species)species}"));
            pkm = (T)sav.GenerateEgg(template, out _);
            pkm.SetSuggestedMoves();
            pkm.SetNature((Nature)Util.Rand.Next(0, 25));
            pkm.SetAbility(Util.Rand.Next(0, 2));
            pkm.SetRandomIVs();
            pkm.Ball = (byte)Util.Rand.Next(0, 26);

            if (shiny)
                pkm.SetShiny();

            var la = new LegalityAnalysis(pkm);
            if (!la.Valid)
                continue;

            pkm = (T)(EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm);
            pkm.ResetPartyStats();

            return pkm;
        }
    }
}
