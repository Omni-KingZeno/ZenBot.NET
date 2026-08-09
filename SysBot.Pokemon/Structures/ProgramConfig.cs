using System.Text.Json.Serialization;
using SysBot.Base;

namespace SysBot.Pokemon;

public class ProgramConfig : BotList<PokeBotState>
{
    public PokeTradeHubConfig Hub { get; set; } = new();
    public int Width { get; set; }
    public int Height { get; set; }
    public static Version Version { get; } = new Version(5, 6, 1);
}

[JsonSerializable(typeof(ProgramConfig))]
[JsonSourceGenerationOptions(WriteIndented = true, DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull)]
public sealed partial class ProgramConfigContext : JsonSerializerContext;
