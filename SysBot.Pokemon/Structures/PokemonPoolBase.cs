using PKHeX.Core;
using SysBot.Base;

namespace SysBot.Pokemon;

public abstract class PokemonPoolBase<T, TRequest>
    : List<T>
    where T : PKM, new()
    where TRequest : class
{
    protected readonly int ExpectedSize = new T().Data.Length;
    public readonly Dictionary<string, TRequest> Files = [];

    protected int Counter;
    private bool InitialStart = true;
    protected abstract string PoolName { get; }
    protected virtual bool Randomized => false;

    public T GetRandomPoke()
    {
        if (InitialStart && Randomized)
        {
            Shuffle(this, 0, Count, Util.Rand);
            InitialStart = false;
        }

        var choice = this[Counter];
        Counter = (Counter + 1) % Count;

        if (Counter == 0 && Randomized)
            Shuffle(this, 0, Count, Util.Rand);

        return choice;
    }

    public T GetRandomSurprise()
    {
        while (true)
        {
            var rand = GetRandomPoke();
            if (DisallowRandomRecipientTrade(rand))
                continue;
            return rand;
        }
    }

    protected static void Shuffle(IList<T> items, int start, int end, Random rnd)
    {
        for (int i = start; i < end; i++)
        {
            int index = i + rnd.Next(end - i);
            (items[index], items[i]) = (items[i], items[index]);
        }
    }

    protected virtual bool ShouldAdd(T pkm, LegalityAnalysis la) => true;

    protected virtual TRequest CreateRequest(T pkm, string fn) =>
        throw new NotImplementedException("Derived class must implement CreateRequest.");

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

        int surpriseBlocked = 0;
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
                LogUtil.LogInfo($"SKIPPED: Provided file is not valid: {dest.FileName}", PoolName);
                continue;
            }

            (bool canBeTraded, string errorMessage) = dest.CanBeTraded();
            if (!canBeTraded)
            {
                LogUtil.LogInfo($"SKIPPED: Provided file cannot be traded: {dest.FileName} -- {errorMessage}", PoolName);
                continue;
            }

            var la = new LegalityAnalysis(dest);
            if (!la.Valid)
            {
                var reason = la.Report();
                LogUtil.LogInfo($"SKIPPED: Provided file is not legal: {dest.FileName} -- {reason}", PoolName);
                continue;
            }

            if (DisallowRandomRecipientTrade(dest, la.EncounterMatch))
            {
                LogUtil.LogInfo("Provided file was loaded but can't be Surprise Traded: " + dest.FileName, nameof(PokemonPool<T>));
                surpriseBlocked++;
            }

            if (!ShouldAdd(dest, la))
                continue;

            var fn = Path.GetFileNameWithoutExtension(file);
            fn = StringsUtil.Sanitize(fn);

            if (!Files.ContainsKey(fn))
            {
                Add(dest);
                Files.Add(fn, CreateRequest(dest, fn));
            }
            else
            {
                LogUtil.LogInfo("Provided file was not added due to duplicate name:  " + dest.FileName, PoolName);
            }

            if (surpriseBlocked == Count)
                LogUtil.LogInfo("Surprise trading will fail; failed to load any compatible files.", nameof(PokemonPool<T>));
            loadedAny = true;
        }

        if (Count > 0)
            LogUtil.LogInfo($"{Count} Pokémon loaded to the {PoolName}", PoolName);

        return loadedAny;
    }

    private static bool DisallowRandomRecipientTrade(T pk, IEncounterTemplate enc)
    {
        // Anti-spam
        if (pk.IsNicknamed)
        {
            Span<char> nick = stackalloc char[pk.TrashCharCountNickname];
            int len = pk.LoadString(pk.NicknameTrash, nick);
            if (len > 6 && enc is not IFixedNickname { IsFixedNickname: true })
                return true;
            nick = nick[..len];
            if (StringsUtil.IsSpammyString(nick))
                return true;
        }
        {
            Span<char> ot = stackalloc char[pk.TrashCharCountTrainer];
            int len = pk.LoadString(pk.OriginalTrainerTrash, ot);
            ot = ot[..len];
            if (StringsUtil.IsSpammyString(ot) && !AutoLegalityWrapper.IsFixedOT(enc, pk))
                return true;
        }

        return DisallowRandomRecipientTrade(pk);
    }

    public static bool DisallowRandomRecipientTrade(T pk)
    {
        // Surprise Trade currently bans Mythicals and Legendaries, not Sub-Legendaries.
        if (SpeciesCategory.IsLegendary(pk.Species))
            return true;
        if (SpeciesCategory.IsMythical(pk.Species))
            return true;

        // Can't surprise trade fused stuff.
        if (FormInfo.IsFusedForm(pk.Species, pk.Form, pk.Format))
            return true;

        return false;
    }
}
