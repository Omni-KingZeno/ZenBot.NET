using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon;

public class GiveawayPool<T> : List<T> where T : PKM, new()
{
    private readonly int ExpectedSize = new T().Data.Length;

    public readonly Dictionary<string, GiveawayRequest<T>> Files = [];

    public bool Reload(string path, SearchOption opt = SearchOption.AllDirectories)
    {
        if (!Directory.Exists(path))
            return false;
        Clear();
        Files.Clear();
        return LoadFolder(path, opt);
    }

    public bool LoadFolder(string path, SearchOption opt = SearchOption.AllDirectories)
    {
        if (!Directory.Exists(path))
            return false;

        var loadedAny = false;
        var files = Directory.EnumerateFiles(path, "*", opt);
        var matchFiles = LoadUtil.GetFilesOfSize(files, ExpectedSize);
        foreach (var file in matchFiles)
        {
            var data = File.ReadAllBytes(file);
            var prefer = EntityFileExtension.GetContextFromExtension(file, EntityContext.None);
            var pkm = EntityFormat.GetFromBytes(data, prefer);
            if (pkm is null)
                continue;
            if (pkm is not T)
                pkm = EntityConverter.ConvertToType(pkm, typeof(T), out _);
            if (pkm is not T dest)
                continue;

            if (dest.Species == 0)
            {
                LogUtil.LogInfo("SKIPPED: Provided file is not valid: " + dest.FileName, nameof(GiveawayPool<T>));
                continue;
            }

            (bool canBeTraded, string errorMessage) = dest.CanBeTraded();
            if (!canBeTraded)
            {
                LogUtil.LogInfo("SKIPPED: Provided file cannot be traded: " + dest.FileName + $" -- {errorMessage}", nameof(GiveawayPool<T>));
                continue;
            }

            var la = new LegalityAnalysis(dest);
            if (!la.Valid)
            {
                var reason = la.Report();
                LogUtil.LogInfo($"SKIPPED: Provided file is not legal: {dest.FileName} -- {reason}", nameof(GiveawayPool<T>));
                continue;
            }

            var fn = Path.GetFileNameWithoutExtension(file);
            fn = StringsUtil.Sanitize(fn);

            // Since file names can be sanitized to the same string, only add one of them.
            if (!Files.ContainsKey(fn))
            {
                Add(dest);
                Files.Add(fn, new GiveawayRequest<T>(dest, fn));
            }
            else
            {
                LogUtil.LogInfo("Provided file was not added due to duplicate name: " + dest.FileName, nameof(GiveawayPool<T>));
            }

            if (Count > 0)
                LogUtil.LogInfo($"{Count} Pokémon loaded files to the Giveaway pool", nameof(GiveawayPool<T>));

            loadedAny = true;
        }
        return loadedAny;
    }
}
