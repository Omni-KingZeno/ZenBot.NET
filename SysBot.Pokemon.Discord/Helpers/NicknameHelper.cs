using Discord;

namespace SysBot.Pokemon.Discord.Helpers;

public static class NicknameHelper
{
    public static string Get(IGuildUser user)
    {
        try
        {
            return user.DisplayName;
        }
        catch
        {
            return user.GlobalName;
        }
    }
}