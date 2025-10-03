using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon;

public class PokemonGAPool<T>(BaseConfig settings) : List<T> where T : PKM, new()
{
    private readonly int ExpectedSize = new T().Data.Length;
    private readonly BaseConfig Settings = settings;
    private bool Randomized => Settings.Shuffled;

    public readonly Dictionary<string, GiveAwayRequest<T>> Files = [];

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
                LogUtil.LogInfo("SKIPPED: Provided file is not valid: " + dest.FileName, nameof(PokemonGAPool<T>));
                continue;
            }

            (bool canBeTraded, string errorMessage) = dest.CanBeTraded();
            if (!canBeTraded)
            {
                LogUtil.LogInfo("SKIPPED: Provided file cannot be traded: " + dest.FileName + $" -- {errorMessage}", nameof(PokemonPool<T>));
                continue;
            }

            var la = new LegalityAnalysis(dest);
            if (!la.Valid)
            {
                var reason = la.Report();
                LogUtil.LogInfo($"SKIPPED: Provided file is not legal: {dest.FileName} -- {reason}", nameof(PokemonGAPool<T>));
                continue;
            }

            var fn = Path.GetFileNameWithoutExtension(file);
            fn = StringsUtil.Sanitize(fn);

            // Since file names can be sanitized to the same string, only add one of them.
            if (!Files.ContainsKey(fn))
            {
                Add(dest);
                Files.Add(fn, new GiveAwayRequest<T>(dest, fn));
            }
            else
            {
                LogUtil.LogInfo("Provided file was not added due to duplicate name: " + dest.FileName, nameof(PokemonGAPool<T>));
            }
            loadedAny = true;
        }
        return loadedAny;
    }
}
