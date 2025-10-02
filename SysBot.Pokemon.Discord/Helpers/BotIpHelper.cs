using PKHeX.Core;

namespace SysBot.Pokemon.Discord;

public static class BotIpHelper<T> where T : PKM, new()
{
    public static string Get(PokeBotRunner<T> runner)
    {
        var bot = runner.Bots.Find(_ => true);

        return bot == null ? "127.0.0.1" : bot.Bot.Config.Connection.IP;
    }
}
