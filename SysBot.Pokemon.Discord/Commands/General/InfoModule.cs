using System.Diagnostics;
using System.Globalization;
using System.Reflection;
using System.Runtime.InteropServices;
using Discord;
using Discord.Commands;

namespace SysBot.Pokemon.Discord;

// src: https://github.com/foxbot/patek/blob/master/src/Patek/Modules/InfoModule.cs
// ISC License (ISC)
// Copyright 2017, Christopher F. <foxbot@protonmail.com>
public class InfoModule : ModuleBase<SocketCommandContext>
{
    private const string detail = "I am an open-source Discord bot powered by PKHeX.Core and other open-source software.";
    private const string pkhexRepo = "https://github.com/kwsch/PKHeX";
    private const string baseRepo = "https://github.com/kwsch/SysBot.NET";
    private const string baseALMRepo = "https://github.com/architdate/PKHeX-Plugins";
    private const string almForkRepo = "https://github.com/santacrab2/PKHeX-Plugins";
    private const string forkRepo = "https://github.com/Manu098vm/ManuBot.NET";
    private const string thisRepo = "https://github.com/Omni-KingZeno/ZenBot.NET";
    private const string version = "v5.0.1";


    [Command("info")]
    [Alias("about", "whoami", "owner")]
    [Summary("Shows information about the bot")]
    public async Task InfoAsync()
    {
        var app = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
        var owner = app.Team?.TeamMembers.FirstOrDefault(member => member.Role == TeamRole.Owner)?.User ?? app.Owner;

        var builder = new EmbedBuilder
        {
            Title = "Here's a bit about me!",
            Color = new Color(90, 199, 250),
        };

        builder.AddField($"{Format.Underline("Info")}",
            $"- {Format.Bold("ZenBot.NET")}: [Source Code]({thisRepo})\n" +
            $"- {Format.Bold("Owner")}: {owner}\n" +
            $"- {Format.Bold("Uptime")}: {GetFormattedUptime(DateTime.Now - Process.GetCurrentProcess().StartTime)}\n" +
            $"- {Format.Bold("ZenBot Version")}: {version}\n" +
            $"- {Format.Bold("PKHeX.Core Version")}: {GetSimpleVersionInfo("PKHeX.Core")}\n" +
            $"- {Format.Bold("AutoLegality Version")}: {GetSimpleVersionInfo("PKHeX.Core.AutoMod")}\n"
        );

        builder.AddField($"{Format.Underline("Credits")}",
            $"- {Format.Bold("[Kurt](https://github.com/kwsch)")}: Creation of [PKHeX]({pkhexRepo}) and [SysBot.NET]({baseRepo})\n" +
            $"- {Format.Bold("[Architdate](https://github.com/architdate)")}: Creation of [Auto Legality Mod(ALM)]({baseALMRepo})\n" +
            $"- {Format.Bold("[Anubis](https://github.com/Lusamine)")}: Continued Support of SysBot.NET\n" +
            $"- {Format.Bold("[santacrab](https://github.com/santacrab2)")}: Continued Support of [ALM]({almForkRepo})\n" +
            $"- {Format.Bold("[Manu](https://github.com/Manu098vm)")}: Creation of the Orginal [Fork]({forkRepo})\n" +
            $"- {Format.Bold("Countless Others")}: For their support and contributions to these projects!\n"
        );

        await ReplyAsync(embed: builder.Build()).ConfigureAwait(false);
    }

    [Command("Legacyinfo")]
    [Summary("Shows information about the bot")]
    public async Task LegacyInfoAsync()
    {
        var app = await Context.Client.GetApplicationInfoAsync().ConfigureAwait(false);
        var owner = app.Team?.TeamMembers.FirstOrDefault(member => member.Role == TeamRole.Owner)?.User ?? app.Owner;

        var builder = new EmbedBuilder
        {
            Color = new Color(114, 137, 218),
            Description = detail,
        };

        builder.AddField("Info",
            $"- [This forks Source Code]({thisRepo})\n" +
            $"- [Upstream fork Source Code]({forkRepo}) by manu098vm\n" +
            $"- [Upstream Source Code]({baseRepo}) by kwsch\n" +
            $"- Special thanks to Manu and his contributors for the original fork this bot is based on\n" +
            $"- Credit to Kurt, Anubis, and Architdate for developing the original SysBot code.\n" +
            $"- {Format.Bold("Owner")}: {owner} ({owner.Id})\n" +
            $"- {Format.Bold("Library")}: Discord.Net ({DiscordConfig.Version})\n" +
            $"- {Format.Bold("Uptime")}: {GetUptime()}\n" +
            $"- {Format.Bold("Runtime")}: {RuntimeInformation.FrameworkDescription} {RuntimeInformation.ProcessArchitecture} " +
            $"({RuntimeInformation.OSDescription} {RuntimeInformation.OSArchitecture})\n" +
            $"- {Format.Bold("Buildtime")}: {GetVersionInfo("SysBot.Base", false)}\n" +
            $"- {Format.Bold("Core Version")}: {GetVersionInfo("PKHeX.Core")}\n" +
            $"- {Format.Bold("AutoLegality Version")}: {GetVersionInfo("PKHeX.Core.AutoMod")}\n"
        );

        builder.AddField("Stats",
            $"- {Format.Bold("Heap Size")}: {GetHeapSize()}MiB\n" +
            $"- {Format.Bold("Guilds")}: {Context.Client.Guilds.Count}\n" +
            $"- {Format.Bold("Channels")}: {Context.Client.Guilds.Sum(g => g.Channels.Count)}\n" +
            $"- {Format.Bold("Users")}: {Context.Client.Guilds.Sum(g => g.MemberCount)}\n"
        );

        await ReplyAsync("Here's a bit about me!", embed: builder.Build()).ConfigureAwait(false);
    }

    private static string GetUptime() => (DateTime.Now - Process.GetCurrentProcess().StartTime).ToString(@"dd\.hh\:mm\:ss");
    private static string GetHeapSize() => Math.Round(GC.GetTotalMemory(true) / (1024.0 * 1024.0), 2).ToString(CultureInfo.CurrentCulture);

    private static string GetVersionInfo(string assemblyName, bool inclVersion = true)
    {
        const string _default = "Unknown";
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assembly = Array.Find(assemblies, x => x.GetName().Name == assemblyName);

        var attribute = assembly?.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute is null)
            return _default;

        var info = attribute.InformationalVersion;
        var split = info.Split('+');
        if (split.Length < 2)
            return _default;

        var version = split[0];
        var revision = split[1];
        if (DateTime.TryParseExact(revision, "yyMMddHHmmss", CultureInfo.InvariantCulture, DateTimeStyles.AssumeUniversal, out var buildTime))
            return (inclVersion ? $"{version} " : "") + $@"{buildTime:yy-MM-dd\.hh\:mm}";
        return !inclVersion ? _default : version;
    }

    public static string GetSimpleVersionInfo(string assemblyName)
    {
        const string _default = "Unknown";
        var assemblies = AppDomain.CurrentDomain.GetAssemblies();
        var assembly = assemblies.FirstOrDefault(x => x.GetName().Name == assemblyName);
        if (assembly is null)
            return _default;

        var attribute = assembly.GetCustomAttribute<AssemblyInformationalVersionAttribute>();
        if (attribute is null)
            return _default;

        var info = attribute.InformationalVersion;
        var split = info.Split('+');
        if (split.Length >= 2)
        {
            var versionParts = split[0].Split('.');
            if (versionParts.Length == 3)
            {
                var major = versionParts[0].PadLeft(2, '0');
                var minor = versionParts[1].PadLeft(2, '0');
                var patch = versionParts[2].PadLeft(2, '0');
                return $"{major}.{minor}.{patch}";
            }
        }
        return _default;
    }

    public static string GetFormattedUptime(TimeSpan uptime)
    {
        string formattedUptime;
        if (uptime.TotalDays >= 7)
        {
            int weeks = (int)uptime.TotalDays / 7;
            int remainingDays = (int)uptime.TotalDays % 7;

            formattedUptime = $"{weeks}w {remainingDays}d {uptime.Hours}h {uptime.Minutes}m";
        }
        else if (uptime.TotalDays >= 1)
        {
            formattedUptime = $"{uptime.Days}d {uptime.Hours}h {uptime.Minutes}m";
        }
        else if (uptime.TotalHours >= 1)
        {
            formattedUptime = $"{uptime.Hours}h {uptime.Minutes}m";
        }
        else if (uptime.TotalMinutes >= 1)
        {
            formattedUptime = $"{uptime.Minutes} minute{(uptime.Minutes > 1 ? "s" : "")}";
        }
        else
        {
            formattedUptime = $"{uptime.Seconds} seconds";
        }
        return formattedUptime;
    }
}
