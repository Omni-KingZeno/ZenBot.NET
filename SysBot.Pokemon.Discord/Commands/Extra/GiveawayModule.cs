using System.Text;
using System.Text.RegularExpressions;
using Discord;
using Discord.Commands;
using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon.Discord;

[Summary("Commands for Giveawy Pokémon.")]
public class GiveawayModule<T> : ModuleBase<SocketCommandContext> where T : PKM, new()
{
    private static TradeQueueInfo<T> Info => SysCord<T>.Runner.Hub.Queues.Info;

    [Command("GiveawayQueue")]
    [Alias("gaq")]
    [Summary("Prints the users in the giveway queues.")]
    [RequireSudo]
    public async Task GetGiveawayListAsync()
    {
        string msg = Info.GetTradeList(PokeRoutineType.LinkTrade);
        var embed = new EmbedBuilder();
        embed.AddField(x =>
        {
            x.Name = "Pending Giveaways";
            x.Value = msg;
            x.IsInline = false;
        });
        await ReplyAsync("These are the users who are currently waiting:", embed: embed.Build()).ConfigureAwait(false);
    }

    [Command("GiveawayPool")]
    [Alias("gpool", "gap")]
    [Summary("Show a list of Pokémon available for giveaway.")]
    [RequireQueueRole(nameof(DiscordManager.RolesGiveaway))]
    public async Task DisplayGiveawayPoolCountAsync()
    {
        var pool = Info.Hub.Giveaway.Pool;
        if (pool.Count == 0)
        {
            await ReplyAsync("Giveaway pool is empty.").ConfigureAwait(false);
        }

        var lines = pool.Files.Select((z, i) => $"{i + 1}: {z.Key} = {(Species)z.Value.RequestInfo.Species}").ToList();

        var embeds = new List<Embed>();
        var pages = lines.Chunk(20).ToList();

        for (int i = 0; i < pages.Count; i++)
        {
            var builder = new EmbedBuilder
            {
                Color = Color.Blue,
                Title = $"Giveaway Pool",
                Description = string.Join("\n", pages[i]),
                Footer = new EmbedFooterBuilder
                {
                    Text = $"Page {i + 1} of {pages.Count}",
                    IconUrl = "https://i.imgur.com/nXNBrlr.png"
                }
            };

            embeds.Add(builder.Build());
        }

        await PaginatedMessage.CreateAsync(Context, embeds).ConfigureAwait(false);
    }

    [Command("GAPoolReload")]
    [Alias("gpr")]
    [Summary("Reloads the bot pool from the setting's folder.")]
    [RequireSudo]
    public async Task ReloadGAPoolAsync()
    {
        var me = SysCord<T>.Runner;
        var hub = me.Hub;

        var pool = hub.Giveaway.Pool.Reload(hub.Config.Folder.GiveAwayFolder);
        if (!pool)
            await ReplyAsync("Failed to reload from folder.").ConfigureAwait(false);
        else
            await ReplyAsync($"Reloaded from Giveaway folder. Giveaway Pool count: {hub.Giveaway.Pool.Count}").ConfigureAwait(false);

    }

    [Command("Giveaway")]
    [Alias("ga", "giveme", "gimme")]
    [Summary("Makes the bot trade you the specified giveaway Pokémon.")]
    [RequireQueueRole(nameof(DiscordManager.RolesGiveaway))]
    public async Task GiveawayAsync([Summary("Giveaway Request")][Remainder] string content)
    {
        var code = Info.GetRandomTradeCode();
        await GiveawayAsync(code, content).ConfigureAwait(false);
    }

    [Command("Giveaway")]
    [Alias("ga", "giveme", "gimme")]
    [Summary("Makes the bot trade you the specified giveaway Pokémon.")]
    [RequireQueueRole(nameof(DiscordManager.RolesGiveaway))]
    public async Task GiveawayAsync([Summary("Giveaway Code")] int code, [Summary("Giveaway Request")][Remainder] string content)
    {
        T pk;
        content = ReusableActions.StripCodeBlock(content);
        var pool = Info.Hub.Giveaway.Pool;
        if (pool.Count == 0)
        {
            await ReplyAsync("Giveaway pool is empty.").ConfigureAwait(false);
            return;
        }
        else if (content.Equals("random", StringComparison.CurrentCultureIgnoreCase)) // Request a random giveaway prize.
        {
            var randomIndex = new Random().Next(pool.Count); // generate a random number between 0 and the number of items in the pool
            pk = pool[randomIndex]; // select the item at the randomly generated index
        }
        else if (Info.Hub.Giveaway.GiveAway.TryGetValue(content, out GiveAwayRequest<T>? val) && val is not null)
        {
            pk = val.RequestInfo;
        }
        else
        {
            await ReplyAsync($"Requested Pokémon not available, use \"{Info.Hub.Config.Discord.CommandPrefix}giveawaypool\" for a full list of available giveaways!").ConfigureAwait(false);
            return;
        }

        var sig = Context.User.GetFavor();
        await QueueHelper<T>.AddToQueueAsync(Context, code, Context.User.Username, sig, pk, PokeRoutineType.LinkTrade, PokeTradeType.Specific, Context.User).ConfigureAwait(false);
    }

    [Command("AddGiveawayPokemon")]
    [Alias("addgap", "agap")]
    [Summary("Adds supplied PKM file to the giveaway folder.")]
    [RequireQueueRole(nameof(DiscordManager.RolesGiveaway))]
    public async Task AddGiveawayAttachAsync()
    {
        await Context.Message.DeleteAsync(RequestOptions.Default).ConfigureAwait(false);
        await UploadGiveawayPokemonFile(Info.Hub.Config.Folder.GiveAwayFolder).ConfigureAwait(false);
    }

    [Command("AddGiveawayPokemon")]
    [Alias("addgap", "agap")]
    [Summary("Adds Pokémon to the giveaway folder based on supplied showdown set.")]
    [RequireQueueRole(nameof(DiscordManager.RolesGiveaway))]
    public async Task AddGiveawayAsync([Summary("Showdown Set")][Remainder] string content)
    {
        Match match = Regex.Match(content, @"\""([^\""]+)\""");

        if (match.Success)
        {
            string filename = match.Groups[1].Value;
            string trimmed = content[(match.Index + match.Length)..].Trim();

            trimmed = ReusableActions.StripCodeBlock(trimmed);
            var set = new ShowdownSet(trimmed);
            var template = AutoLegalityWrapper.GetTemplate(set);
            if (set.InvalidLines.Count != 0 || set.Species is 0)
            {
                var sb = new StringBuilder(128);
                sb.AppendLine("Unable to parse Showdown Set.");
                var invalidlines = set.InvalidLines;
                if (invalidlines.Count != 0)
                {
                    var localization = BattleTemplateParseErrorLocalization.Get();
                    sb.AppendLine("Invalid lines detected:\n```");
                    foreach (var line in invalidlines)
                    {
                        var error = line.Humanize(localization);
                        sb.AppendLine(error);
                    }
                    sb.AppendLine("```");
                }
                if (set.Species is 0)
                    sb.AppendLine("Species could not be identified. Check your spelling.");

                var msg = sb.ToString();
                await ReplyAsync(msg).ConfigureAwait(false);
                return;
            }

            try
            {
                var sav = AutoLegalityWrapper.GetTrainerInfo<T>();
                var pkm = sav.GetLegal(template, out var result);
                var la = new LegalityAnalysis(pkm);
                var spec = GameInfo.Strings.Species[template.Species];
                pkm = EntityConverter.ConvertToType(pkm, typeof(T), out _) ?? pkm;
                if (pkm is not T pk || !la.Valid)
                {
                    var reason = result switch
                    {
                        "Timeout" => $"That {spec} set took too long to generate.",
                        "VersionMismatch" => "Request refused: PKHeX and Auto-Legality Mod version mismatch.",
                        _ => $"I wasn't able to create a {spec} from that set.",
                    };
                    var imsg = $"Oops! {reason}";
                    if (result == "Failed")
                        imsg += $"\n{AutoLegalityWrapper.GetLegalizationHint(template, sav, pkm)}";
                    await ReplyAsync(imsg).ConfigureAwait(false);
                    return;
                }
                pk.ResetPartyStats();
                await Context.Message.DeleteAsync().ConfigureAwait(false);
                await UploadGiveawayPokemonSet(Info.Hub.Config.Folder.GiveAwayFolder, filename, pk, out var msg).ConfigureAwait(false);
                await ReplyAsync(msg).ConfigureAwait(false);
            }
            catch (Exception ex)
            {
                if (set != null)
                {
                    LogUtil.LogSafe(ex, nameof(TradeModule<T>));
                    var msg = $"Oops! An unexpected problem happened with this Showdown Set:\n```{string.Join("\n", set.GetSetLines())}```";
                    await ReplyAsync(msg).ConfigureAwait(false);
                }
                else
                {
                    LogUtil.LogSafe(ex, nameof(TradeModule<T>));
                    var msg = "Oops! An unexpected problem happened with this Showdown Set. Unable to Parse set";
                    await ReplyAsync(msg).ConfigureAwait(false);
                }
            }
        }
        else
        {
            await ReplyAsync("You must include a comment in \" \" before the Showdown set!").ConfigureAwait(false);
        }
    }

    public Task UploadGiveawayPokemonSet(string folder, string fileName, T pk, out string msg)
    {
        if (string.IsNullOrEmpty(folder))
            Info.Hub.Config.Folder.GiveAwayFolder = folder = "giveaway";
        if (!Directory.Exists(folder))
            Directory.CreateDirectory(folder);
        var fn = Path.Combine(folder, fileName + Path.GetExtension(pk.FileName));
        File.WriteAllBytes(fn, pk.DecryptedPartyData);
        LogUtil.LogInfo($"Saved file: {fn}", $"{folder}");
        msg = $"{Format.Bold(fileName)} added to the giveaway folder.";

        return Task.CompletedTask;
    }

    public async Task UploadGiveawayPokemonFile(string folder)
    {
        if (!Directory.Exists(folder))
        {
            await ReplyAsync($"No giveaway folder found.");
            return;
        }
        var attachment = Context.Message.Attachments.FirstOrDefault();
        if (attachment == default)
        {
            await ReplyAsync("No attachment provided!").ConfigureAwait(false);
            return;
        }

        var att = await NetUtil.DownloadAttachmentAsync(attachment).ConfigureAwait(false);
        var pk = GetRequest(att);
        if (pk == null)
        {
            await ReplyAsync("Attachment provided is not compatible with this module!").ConfigureAwait(false);
            return;
        }
        Directory.CreateDirectory(folder);
        var gaName = attachment.Filename.Replace("_", " ");
        var path = Path.Combine(folder, gaName);
        File.WriteAllBytes(path, pk.DecryptedPartyData);
        LogUtil.LogInfo($"Saved file: {path}", $"{folder}");
        await ReplyAsync($"{Format.Bold(gaName[..^4])} added to the {folder} folder.");
        await ReloadGAPoolAsync().ConfigureAwait(false);
    }

    private static T? GetRequest(Download<ISpeciesForm> dl)
    {
        if (!dl.Success)
            return null;

        return dl.Data switch
        {
            T entity => entity,
            PKM pkm => EntityConverter.ConvertToType(pkm, typeof(T), out _) as T,
            _ => null,
        };
    }
}
