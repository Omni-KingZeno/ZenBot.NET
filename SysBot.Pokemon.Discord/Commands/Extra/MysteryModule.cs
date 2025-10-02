using Discord.Commands;
using PKHeX.Core;

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
        Random rndm = new();
        var trainer = AutoLegalityWrapper.GetTrainerInfo<T>();
        var sav = BlankSaveFile.Get(trainer.Version, trainer.OT);
        var availSpec = Enumerable.Range(0, sav.Personal.MaxSpeciesID).Where(i => sav.Personal.IsSpeciesInGame((ushort)i)).Select(i => (ushort)i).ToList();
        var specIndex = rndm.Next(availSpec.Count);
        ushort speciesId = availSpec[specIndex];
        var content = GameInfo.GetStrings("en").specieslist[speciesId];
        int randomNumber = rndm.Next(0, 1365);
        var shiny = randomNumber == 0;
        content += $"\n.IVs=$rand\n" +
            $".Nature=$0,24\n{(shiny ? "Shiny: Yes\n" : "")}" +
            $".Moves=$suggest\n" +
            $".AbilityNumber=$0,2\n" +
            $".TeraTypeOverride=$rand\n" +
            $".Ball=$0,37\n" +
            $".DynamaxLevel=$0,10\n" +
            $".TrainerTID7=$0001,3559\n" +
            $".TrainerSID7=$000001,993401\n" +
            $".OriginalTrainerName=Surprise!\n" +
            $".GV_ATK=$0,7\n" +
            $".GV_DEF=$0,7\n" +
            $".GV_HP=$0,7\n" +
            $".GV_SPA=$0,7\n" +
            $".GV_SPD=$0,7\n" +
            $".GV_SPE=$0,7";

        var set = new ShowdownSet(content);
        var template = AutoLegalityWrapper.GetTemplate(set);
        var pkm = sav.GetLegal(template, out _);
        var la = new LegalityAnalysis(pkm);
        if (pkm is not T pk || set.InvalidLines.Count != 0 || !la.Valid)
        {
            await ReplyAsync("Oops! I had an issue generating a Pokémon for you!").ConfigureAwait(false);
            return;
        }
        pk.ResetPartyStats();
        var sig = Context.User.GetFavor();
        await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.Specific, Context.User).ConfigureAwait(false);
    }

    public static T MysteryEgg(out T pkm)
    {
        Random rndm = new();
        var trainer = AutoLegalityWrapper.GetTrainerInfo<T>();
        var sav = BlankSaveFile.Get(trainer.Version, trainer.OT);

        while (true)
        {
            bool foundValidSpecies = false;
            Species randomSpecies = Species.None;

            while (!foundValidSpecies)
            {
                var availSpec = Enumerable.Range(0, sav.Personal.MaxSpeciesID)
                    .Where(i => sav.Personal.IsSpeciesInGame((ushort)i))
                    .Select(i => (ushort)i)
                    .ToList();

                ushort speciesId = availSpec[rndm.Next(availSpec.Count)];

                if (Breeding.CanHatchAsEgg(speciesId))
                {
                    randomSpecies = (Species)speciesId;
                    foundValidSpecies = true;
                }
            }

            int randomNumber = rndm.Next(0, 1365);
            var shiny = randomNumber == 0;

            var content = randomSpecies + $"\n.Nature=$0,24\n{(shiny ? "Shiny: Yes\n" : "")}" +
                          ".Moves=$suggest\n" +
                          ".AbilityNumber=$0,2";
            var set = new ShowdownSet(content);
            pkm = (T)sav.GetLegalEgg(set, out _);

            var la = new LegalityAnalysis(pkm);
            if (!la.Valid)
                continue;

            pkm = (T)(EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm);
            pkm.ResetPartyStats();

            return pkm;
        }
    }
}
