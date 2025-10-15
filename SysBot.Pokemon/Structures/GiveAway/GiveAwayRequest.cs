using PKHeX.Core;

namespace SysBot.Pokemon;

public class GiveawayRequest<T>(T requestInfo, string nickname) where T : PKM, new()
{
    public readonly string Nickname = nickname;
    public readonly T RequestInfo = requestInfo;
}