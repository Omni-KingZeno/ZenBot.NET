using Discord;
using Discord.WebSocket;
using PKHeX.Core;
using SysBot.Base;
using SysBot.Pokemon.Discord.Helpers;

namespace SysBot.Pokemon.Discord;

public static class ReusableActions
{
    public static async Task SendPKMAsync(this IMessageChannel channel, PKM pkm, string msg = "")
    {
        var tmp = Path.Combine(Path.GetTempPath(), PathUtil.CleanFileName(pkm.FileName));
        Span<byte> data = stackalloc byte[pkm.SIZE_PARTY];
        pkm.WriteDecryptedDataStored(data);
        await File.WriteAllBytesAsync(tmp, data.ToArray());
        await channel.SendFileAsync(tmp, msg).ConfigureAwait(false);
        File.Delete(tmp);
    }

    public static async Task SendPKMAsync(this IUser user, PKM pkm, string msg = "")
    {
        var tmp = Path.Combine(Path.GetTempPath(), PathUtil.CleanFileName(pkm.FileName));
        Span<byte> data = stackalloc byte[pkm.SIZE_PARTY];
        pkm.WriteDecryptedDataStored(data);
        await File.WriteAllBytesAsync(tmp, data.ToArray());
        await user.SendFileAsync(tmp, msg).ConfigureAwait(false);
        File.Delete(tmp);
    }

    public static async Task RepostPKMAsShowdownAsync(this ISocketMessageChannel channel, IAttachment att)
    {
        if (!EntityDetection.IsSizePlausible(att.Size))
            return;
        var result = await NetUtil.DownloadAttachmentAsync(att).ConfigureAwait(false);
        if (!result.Success)
            return;
        if (result.Data is not PKM pkm)
            return;

        await channel.SendPKMAsShowdownSetAsync(pkm).ConfigureAwait(false);
    }

    public static RequestSignificance GetFavor(this IUser user)
    {
        var mgr = SysCordSettings.Manager;
        if (user.Id == mgr.Owner || SysCordSettings.Admins.Contains(user.Id))
            return RequestSignificance.Owner;
        if (mgr.CanUseSudo(user.Id))
            return RequestSignificance.Favored;
        if (user is SocketGuildUser g)
            return mgr.GetSignificance(g.Roles.Select(z => z.Name));
        return RequestSignificance.None;
    }

    public static async Task EchoAndReply(this ISocketMessageChannel channel, string msg)
    {
        // Announce it in the channel the command was entered only if it's not already an echo channel.
        EchoUtil.Echo(msg);
        if (!EchoModule.IsEchoChannel(channel))
            await channel.SendMessageAsync(msg).ConfigureAwait(false);
    }

    public static async Task SendPKMAsShowdownSetAsync(this ISocketMessageChannel channel, PKM pkm)
    {
        var txt = GetFormattedShowdownText(pkm);
        var color = EmbedColorHelper.GetDiscordColor(pkm.IsShiny ? EmbedColorHelper.ShinyMap[((Species)pkm.Species, pkm.Form)] : (PersonalColor)pkm.PersonalInfo.Color);
        var pkmType = pkm.GetType();
        var tradeExtensionsType = typeof(TradeExtensions<>).MakeGenericType(pkmType);
        var method = tradeExtensionsType.GetMethod("GetPokemonImageURL", System.Reflection.BindingFlags.Public | System.Reflection.BindingFlags.Static);
        var url = (string)method!.Invoke(null, [pkm, pkm is IGigantamax g && g.CanGigantamax, false, false])!;
        var la = new LegalityAnalysis(pkm);
        var embed = new EmbedBuilder()
            .WithTitle("Here's what you showed me!")
            .WithDescription(txt)
            .WithColor(color)
            .WithThumbnailUrl(url)
            .WithFooter(la.Valid ? "This Pokémon is legal" : "This Pokémon is not legal", la.Valid ? "https://raw.githubusercontent.com/Omni-KingZeno/Pokemon-Sprites/refs/heads/main/Bot/check.png" : "https://raw.githubusercontent.com/Omni-KingZeno/Pokemon-Sprites/refs/heads/main/Bot/x.png");

        await channel.SendMessageAsync(embed: embed.Build()).ConfigureAwait(false);
    }

    public static string GetFormattedShowdownText(PKM pkm)
    {
        var showdown = ShowdownParsing.GetShowdownText(pkm);
        var lines = showdown.Split('\n').ToList();

        int natureIndex = lines.FindIndex(z => z.Contains("Nature"));
        if (pkm.Ball > (int)Ball.None && natureIndex != -1)
            lines.Insert(natureIndex, $"Ball: {(Ball)pkm.Ball} Ball");

        int abilityIndex = lines.FindIndex(z => z.Contains("Ability:"));
        if (pkm is IAlpha alpha && alpha.IsAlpha && abilityIndex != -1)
            lines.Insert(abilityIndex + 1, "Alpha: Yes");

        int shinyIndex = lines.FindIndex(x => x.Contains("Shiny: Yes"));
        if (pkm is PK8 && pkm.IsShiny && shinyIndex != -1)
            lines[shinyIndex] = pkm.ShinyXor == 0 || pkm.FatefulEncounter ? "Shiny: Square" : "Shiny: Star";

        var trainerInfo = new List<string>
        {
            $"OT: {pkm.OriginalTrainerName}",
            $"TID: {pkm.GetDisplayTID()}",
            $"SID: {pkm.GetDisplaySID()}"
        };
        if (pkm.IsEgg)
            trainerInfo.Add("IsEgg: Yes");

        lines.InsertRange(1, trainerInfo);

        return Format.Code(string.Join("\n", lines));
    }

    private static readonly string[] separator = [",", ", ", " "];

    public static IReadOnlyList<string> GetListFromString(string str)
    {
        // Extract comma separated list
        return str.Split(separator, StringSplitOptions.RemoveEmptyEntries);
    }

    public static string StripCodeBlock(string str) => str
        .Replace("`\n", "")
        .Replace("\n`", "")
        .Replace("`", "")
        .Trim();
}
