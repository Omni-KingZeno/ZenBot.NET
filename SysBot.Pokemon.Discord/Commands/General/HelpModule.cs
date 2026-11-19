using Discord;
using Discord.Commands;

namespace SysBot.Pokemon.Discord;

public class HelpModule(CommandService Service) : ModuleBase<SocketCommandContext>
{
    private const string PictoGuideUrl = "https://raw.githubusercontent.com/Omni-KingZeno/Pokemon-Sprites/refs/heads/main/Bot/PictoCodeChart.png";
    private static bool IsLGPE => SysCordSettings.HubConfig.Mode == ProgramMode.LGPE;

    private static readonly HashSet<string> TradeCommandNames = new(StringComparer.OrdinalIgnoreCase)
    {
        "trade", "tradeuser",
    };

    [Command("Legacyhelp")]
    [Summary("Lists available commands.")]
    public async Task HelpAsync()
    {
        var embeds = new List<EmbedBuilder>();
        var builder = new EmbedBuilder
        {
            Color = Color.Purple,
            Title = "These are the commands you can use:",
        };
        embeds.Add(builder);

        var mgr = SysCordSettings.Manager;
        var app = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
        var owner = app.Team?.OwnerUserId ?? app.Owner.Id;
        var uid = Context.User.Id;

        HashSet<string> mentioned = [];
        foreach (var module in Service.Modules.OrderBy(module => module.Name))
        {
            string? description = null;
            foreach (var cmd in module.Commands)
            {
                var name = cmd.Name;
                if (mentioned.Contains(name))
                    continue;
                if (ShouldHideCommand(cmd))
                    continue;
                if (cmd.Attributes.Any(z => z is RequireOwnerAttribute) && owner != uid)
                    continue;
                if (cmd.Attributes.Any(z => z is RequireSudoAttribute) && !mgr.CanUseSudo(uid))
                    continue;

                mentioned.Add(name);
                var result = await cmd.CheckPreconditionsAsync(Context).ConfigureAwait(false);
                if (result.IsSuccess)
                    description += $"{cmd.Aliases[0]}\n";
            }
            if (string.IsNullOrWhiteSpace(description))
                continue;

            var moduleName = module.Name;
            var gen = moduleName.IndexOf('`');
            if (gen != -1)
                moduleName = moduleName[..gen];

            if (builder.Fields.Count >= 24)
            {
                builder = new EmbedBuilder
                {
                    Color = Color.Purple,
                    Title = "Commands (continued):",
                };
                embeds.Add(builder);
            }

            builder.AddField(x =>
            {
                x.Name = moduleName;
                x.Value = description;
                x.IsInline = false;
            });
        }

        bool isFirst = true;
        foreach (var embed in embeds)
        {
            var message = isFirst ? "Help has arrived!" : null;
            await ReplyAsync(message, false, embed.Build()).ConfigureAwait(false);
            isFirst = false;
        }
    }

    [Command("help")]
    [Summary("Lists information about a specific command.")]
    public async Task HelpAsync([Summary("The command you want help for")] string command)
    {
        var result = Service.Search(Context, command);

        if (!result.IsSuccess)
        {
            await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.").ConfigureAwait(false);
            return;
        }

        var builder = new EmbedBuilder
        {
            Color = Color.Purple,
            Description = $"Here are some commands like **{command}**:",
        };

        bool includesTradeCmd = false;

        foreach (var match in result.Commands)
        {
            var cmd = match.Command;
            if (ShouldHideCommand(cmd))
                continue;

            if (cmd.Aliases.Any(a => TradeCommandNames.Contains(a)))
                includesTradeCmd = true;

            builder.AddField(x =>
            {
                x.Name = string.Join(", ", cmd.Aliases);
                x.Value = GetCommandSummary(cmd);
                x.IsInline = false;
            });
        }

        if (builder.Fields.Count == 0)
        {
            await ReplyAsync($"Sorry, I couldn't find a command like **{command}**.").ConfigureAwait(false);
            return;
        }

        if (IsLGPE && includesTradeCmd)
        {
            AddPictoGuide(builder);
        }

        await ReplyAsync("Help has arrived!", false, builder.Build()).ConfigureAwait(false);
    }

    [Command("ListCommands")]
    [Alias("help", "commands", "cmds")]
    [Summary("Lists available commands by modules in paged format.")]
    public async Task ListCommands()
    {
        var modules = Service.Modules.OrderBy(module => !module.Name.StartsWith("TradeM")).ThenBy(module => module.Name).ToList();
        var embeds = new List<Embed>();

        foreach (var module in modules)
        {
            var moduleName = module.Name;
            if (moduleName.Contains("`1"))
            {
                moduleName = moduleName.Replace("`1", string.Empty);
            }
            if (moduleName.Contains("Module"))
            {
                moduleName = moduleName.Replace("Module", string.Empty);
            }
            var currentBuilder = new EmbedBuilder
            {
                Color = Color.Purple,
                Description = $"## Commands in the {moduleName} module:",
                ThumbnailUrl = "https://static.wikia.nocookie.net/pokemonfireash/images/c/c2/Professor_Oak.png/revision/latest?cb=20220702133216"
            };

            var commands = module.Commands.OrderBy(cmd => cmd.Name).ToList();
            HashSet<string> mentioned = [];
            int fieldCount = 0;

            foreach (var cmd in commands)
            {
                var name = cmd.Name;
                if (mentioned.Contains(name))
                    continue;
                if (ShouldHideCommand(cmd))
                    continue;
                if (cmd.Attributes.Any(z => z is RequireOwnerAttribute) && Context.Client.GetApplicationInfoAsync().Result.Owner.Id != Context.User.Id)
                    continue;
                if (cmd.Attributes.Any(z => z is RequireSudoAttribute) && !SysCordSettings.Manager.CanUseSudo(Context.User.Id))
                    continue;
                if (module.Name.Contains("Sudo") && Context.User.Id == Context.Client.GetApplicationInfoAsync().Result.Owner.Id)
                    continue;

                mentioned.Add(name);
                var result = cmd.CheckPreconditionsAsync(Context).Result;
                if (result.IsSuccess)
                {
                    if (currentBuilder == null || fieldCount >= 10)
                    {
                        if (currentBuilder != null)
                        {
                            embeds.Add(currentBuilder.Build());
                        }

                        currentBuilder = new EmbedBuilder
                        {
                            Color = Color.Purple,
                            Description = fieldCount == 0 ?
                                $"## Commands in the {moduleName} module:" :
                                $"## Commands in the {moduleName} module (Cont'd):",
                            ThumbnailUrl = "https://static.wikia.nocookie.net/pokemonfireash/images/c/c2/Professor_Oak.png/revision/latest?cb=20220702133216"
                        };
                        fieldCount = 0;
                    }

                    currentBuilder.AddField(x =>
                    {
                        x.Name = string.Join(", ", cmd.Aliases);
                        x.Value = GetCommandSummary(cmd) + "\n";
                        x.IsInline = false;
                    });
                    fieldCount++;
                }
            }

            if (currentBuilder != null && currentBuilder.Fields.Count > 0)
            {
                if (IsLGPE && moduleName.Equals("Trade", StringComparison.OrdinalIgnoreCase))
                {
                    AddPictoGuide(currentBuilder);
                }

                embeds.Add(currentBuilder.Build());
            }
        }

        if (embeds.Count == 0)
        {
            await ReplyAsync("No commands available for you.").ConfigureAwait(false);
            return;
        }

        await PaginatedMessage.CreateAsync(Context, embeds);
    }

    private void AddPictoGuide(EmbedBuilder builder)
    {
        builder.AddField(x =>
        {
            x.Name = "Choosing the Trade Code in Let's Go";
            x.Value =
                "Let's Go uses three pictures as the link code. Look up each picture's number on the chart below and type the three digits as your code.\n" +
                $"- Example: `@BotMention trade 072 Snorlax` => Code would be **`Pikachu, Rattata, Bulbasaur`**\n" +
                "- With no code provided, a random one is chosen for you";
            x.IsInline = false;
        });
        builder.WithImageUrl(PictoGuideUrl);
    }

    private static string GetCommandSummary(CommandInfo cmd)
    {
        return $"-# **Summary:**\n-  -# {cmd.Summary}\n-# **Parameters:** {GetParameterSummary(cmd.Parameters)}";
    }

    private static string GetParameterSummary(IReadOnlyList<ParameterInfo> p)
    {
        if (p.Count == 0)
            return "None";
        return $"{p.Count}\n  - -# " + string.Join("\n  - -# ", p.Select(GetParameterSummary));
    }

    private static string GetParameterSummary(ParameterInfo z)
    {
        var result = z.Name;
        if (!string.IsNullOrWhiteSpace(z.Summary))
            result += $" ({z.Summary})";
        return result;
    }

    private static bool ShouldHideCommand(CommandInfo cmd)
    {
        return IsLGPE && (cmd.Name.Equals("eggtrade", StringComparison.OrdinalIgnoreCase) ||
                          cmd.Aliases.Any(a => a.Equals("eggtrade", StringComparison.OrdinalIgnoreCase)));
    }

}
