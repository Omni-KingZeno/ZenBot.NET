using Discord;
using Discord.WebSocket;

namespace SysBot.Pokemon.Discord;

public static class DistributionEmbeds
{
    public static Func<ulong, ISocketMessageChannel?> ChannelLookup { get; set; } = _ => null;

    public static ITextChannel? GetChannel(ulong id) => ChannelLookup(id) as ITextChannel;

    public static Embed BuildEnded(IUserMessage message, string status, string? trader = null)
    {
        var builder = message.Embeds.First().ToEmbedBuilder();
        var footer = trader == null ? "Ended" : "Traded";
        builder.WithTitle($"{status}")
               .WithFooter(footer)
               .WithTimestamp(DateTimeOffset.UtcNow);

        var codeField = builder.Fields.FirstOrDefault(f => f.Name == "Trade Code");
        if (codeField is not null)
        {
            if (!string.IsNullOrWhiteSpace(trader))
            {
                codeField.Name = "Trader IGN";
                codeField.Value = trader;
            }
            else
            {
                codeField.Value = "~~expired~~";
            }
        }

        return builder.Build();
    }
}
