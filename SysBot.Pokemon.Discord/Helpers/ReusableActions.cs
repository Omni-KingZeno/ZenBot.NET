using Discord;
using Discord.WebSocket;
using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon.Discord;

public static class ReusableActions
{
    public static async Task SendPKMAsync(this IMessageChannel channel, PKM pkm, string msg = "")
    {
        var tmp = Path.Combine(Path.GetTempPath(), PathUtil.CleanFileName(pkm.FileName));
        await File.WriteAllBytesAsync(tmp, pkm.DecryptedPartyData);
        await channel.SendFileAsync(tmp, msg).ConfigureAwait(false);
        File.Delete(tmp);
    }

    public static async Task SendPKMAsync(this IUser user, PKM pkm, string msg = "")
    {
        var tmp = Path.Combine(Path.GetTempPath(), PathUtil.CleanFileName(pkm.FileName));
        await File.WriteAllBytesAsync(tmp, pkm.DecryptedPartyData);
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
        await channel.SendMessageAsync(txt).ConfigureAwait(false);
    }

    public static string GetFormattedShowdownText(PKM pkm)
    {
        var showdown = ShowdownParsing.GetShowdownText(pkm);
        return Format.Code(showdown);
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
