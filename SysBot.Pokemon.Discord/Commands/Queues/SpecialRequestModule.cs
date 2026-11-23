using Discord;
using Discord.Commands;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord;

[Summary("Special Requests Commands.")]
public class SpecialRequestModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [Command("SpecialRequest")]
    [Alias("sr")]
    [Summary("Adds the user to the Special Request Queue")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task SpecialRequestAsync([Summary("Trade Code")] int code)
    {
        var sig = Context.User.GetFavor();
        await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, new T(), PokeRoutineType.LinkTrade, PokeTradeType.SpecialRequest, Context.User).ConfigureAwait(false);
    }

    [Command("SpecialRequest")]
    [Alias("sr")]
    [Summary("Adds the user to the Special Request Queue.")]
    [RequireQueueRole(nameof(DiscordManager.RolesTrade))]
    public async Task SpecialRequestAsync()
    {
        var code = Info.GetRandomTradeCode();
        await SpecialRequestAsync(code).ConfigureAwait(false);
    }

    [Command("SpecialRequestHelp")]
    [Alias("specialhelp", "srhelp", "srh")]
    [Summary("Shows the held item and nickname requests available with Special Requests.")]
    public async Task SpecialHelpAsync()
    {
        bool isPA8 = typeof(T) == typeof(PA8);

        var embed = new EmbedBuilder
        {
            Color = Color.Gold,
            Title = "Special Request Guide",
            Description = "Use `@BotMention sr` to queue, then offer a Pokémon with the item held or the nickname set. " +
                          "The bot applies the change, legalizes it, and trades it back.",
        };

        if (!isPA8)
        {
            embed.AddField("**__Ownership__** (Held item)",
            "- -# **Ultra Ball**: Your OT/TID/SID and clears the nickname\n" +
            "- -# **Great Ball**: Your OT/TID/SID, keeps the nickname\n" +
            "- -# **Poké Ball**: Clears the nickname");


            embed.AddField("**__Shiny__** (Held item)",
                "- -# **Antidote / Ice Heal**: Star shiny\n" +
                "- -# **Burn Heal**: Square shiny\n" +
                "- -# **Awakening**: Square shiny + 6 IVs\n" +
                "- -# **Paralyze Heal**: Removes shiny");

            embed.AddField("**__IVs and Level__** (Held item)",
                "- -# **Full Heal / Max Revive**: 6 IVs\n" +
                "- -# **Revive**: 0 Atk, 0 Spe\n" +
                "- -# **Fresh Water**: 0 Atk\n" +
                "- -# **Poké Doll**: 0 Spe\n" +
                "- -# **Soda Pop / Lemonade**: 6 IVs + level 100\n" +
                "  - -# **NOTE**: Hyper training is cleared.");

            embed.AddField("**__Language__** (Held item)",
                "- -# **JPN**: Guard Spec\n" +
                "- -# **ENG**: Dire Hit\n" +
                "- -# **GER**: X Attack\n" +
                "- -# **FRE**: X Defense\n" +
                "- -# **ESP**: X Speed\n" +
                "- -# **KOR**: X Accuracy\n" +
                "- -# **CHT**: X Sp. Atk\n" +
                "- -# **CHS**: X Sp. Def\n" +
                "  - -# **NOTE**: The Nickname is cleared.");

            embed.AddField("**__Nature__** (Held item)",
                "- -# Any **Mint** sets the matching nature.");

            embed.AddField("**__TMs__** (Held item)",
                "- -# Any **TM/TR** held: Unlocks all TM/TR move flags.");

            if (typeof(T) == typeof(PK9))
            {
                embed.AddField("**__Tera Type__** (Held item)",
                    "- -# Any Tera Shard (including Stellar) sets the Tera Type.");
            }
        }

        if (isPA8)
        {
            embed.AddField("Nickname commands (Legends: Arceus)",
                "- -# `clear ot` / `clear both`: reset nickname + OT to yours\n" +
                "- -# `clear nick`: reset the nickname\n" +
                "- -# `male` / `!female`: set gender\n" +
                "- -# `make shiny` (`shiny6` adds 6 IVs, `make normal` removes shiny)\n" +
                "- -# `stats 6iv` / `5ivs` / `5iva` / `4iv` (add `lvl100` for level 100)\n" +
                "- -# `nat <nature>`: set nature\n" +
                "- -# `lang <JPN/ENG/GER/FRE/ESP/ESL/ITA/KOR/CHT/CHS>`: set language");
        }

        if (!isPA8)
        {
            embed.AddField("**__Ball__** (Nickname)", "- -# `?<BallName> Ball` sets the specified ball\n" +
                "  - -# **NOTE**: Spelling and availability must be valid.");
        }

        embed.AddField("**__Form__** (Nickname)", "- -# `Form5` sets the form to 5 (`Form 5` works too)\n" +
            "  - -# **NOTE**: Requested Form must be valid.");

        await ReplyAsync(embed: embed.Build()).ConfigureAwait(false);
    }
}
