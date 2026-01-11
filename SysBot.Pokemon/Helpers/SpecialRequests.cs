using System.Collections.Frozen;
using PKHeX.Core;
using PKHeX.Core.AutoMod;

namespace SysBot.Pokemon;

public static class SpecialRequests
{
    private static readonly FrozenDictionary<int, LanguageID> LanguageItems = new Dictionary<int, LanguageID>
    {
        [Items.GuardSpec] = LanguageID.Japanese,
        [Items.DireHit]   = LanguageID.English,
        [Items.XAtk]      = LanguageID.German,
        [Items.XDef]      = LanguageID.French,
        [Items.XSpe]      = LanguageID.Spanish,
        [Items.XAcc]      = LanguageID.Korean,
        [Items.XSpAtk]    = LanguageID.ChineseT,
        [Items.XSpDef]    = LanguageID.ChineseS
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<int, MoveType> TeraShardTypes = new Dictionary<int, MoveType>
    {
        [Items.NormalTeraShard]   = MoveType.Normal,
        [Items.FireTeraShard]     = MoveType.Fire,
        [Items.WaterTeraShard]    = MoveType.Water,
        [Items.ElectricTeraShard] = MoveType.Electric,
        [Items.GrassTeraShard]    = MoveType.Grass,
        [Items.IceTeraShard]      = MoveType.Ice,
        [Items.FightingTeraShard] = MoveType.Fighting,
        [Items.PoisonTeraShard]   = MoveType.Poison,
        [Items.GroundTeraShard]   = MoveType.Ground,
        [Items.FlyingTeraShard]   = MoveType.Flying,
        [Items.PsychicTeraShard]  = MoveType.Psychic,
        [Items.BugTeraShard]      = MoveType.Bug,
        [Items.RockTeraShard]     = MoveType.Rock,
        [Items.GhostTeraShard]    = MoveType.Ghost,
        [Items.DragonTeraShard]   = MoveType.Dragon,
        [Items.DarkTeraShard]     = MoveType.Dark,
        [Items.SteelTeraShard]    = MoveType.Steel,
        [Items.FairyTeraShard]    = MoveType.Fairy
    }.ToFrozenDictionary();

    private static readonly FrozenSet<ushort> MaxLairLegendaries = new HashSet<ushort>
    {
        144, 145, 146, 150, 243, 244, 245, 249, 250, 380, 381, 382, 383, 384,
        480, 481, 482, 483, 484, 485, 487, 488, 641, 642, 643, 644, 645, 646,
        716, 717, 718, 785, 786, 787, 788, 791, 792, 793, 794, 795, 796, 797,
        798, 799, 800, 805, 806
    }.ToFrozenSet();

    private static readonly FrozenDictionary<string, Nature> NatureMap = new Dictionary<string, Nature>
    {
        ["adamant"] = Nature.Adamant,
        ["bold"]    = Nature.Bold,
        ["brave"]   = Nature.Brave,
        ["calm"]    = Nature.Calm,
        ["careful"] = Nature.Careful,
        ["gentle"]  = Nature.Gentle,
        ["hasty"]   = Nature.Hasty,
        ["impish"]  = Nature.Impish,
        ["jolly"]   = Nature.Jolly,
        ["lax"]     = Nature.Lax,
        ["lonely"]  = Nature.Lonely,
        ["mild"]    = Nature.Mild,
        ["modest"]  = Nature.Modest,
        ["naive"]   = Nature.Naive,
        ["naughty"] = Nature.Naughty,
        ["quiet"]   = Nature.Quiet,
        ["rash"]    = Nature.Rash,
        ["relaxed"] = Nature.Relaxed,
        ["sassy"]   = Nature.Sassy,
        ["serious"] = Nature.Serious,
        ["timid"]   = Nature.Timid
    }.ToFrozenDictionary();

    private static readonly FrozenDictionary<string, LanguageID> LanguageNicknameMap = new Dictionary<string, LanguageID>
    {
        ["JPN"] = LanguageID.Japanese,
        ["ENG"] = LanguageID.English,
        ["GER"] = LanguageID.German,
        ["FRE"] = LanguageID.French,
        ["ESP"] = LanguageID.Spanish,
        ["ESL"] = LanguageID.SpanishL,
        ["ITA"] = LanguageID.Italian,
        ["KOR"] = LanguageID.Korean,
        ["CHT"] = LanguageID.ChineseT,
        ["CHS"] = LanguageID.ChineseS
    }.ToFrozenDictionary();

    public static SpecialTradeType CheckItemRequest<T>(ref T pk, PokeRoutineExecutor<T> caller, PokeTradeDetail<T> detail, string trainerName, uint tid, uint sid) where T : PKM, new()
    {
        LogHeldItem(pk, caller);

        var result = pk.HeldItem switch
        {
            Items.UltraBall or Items.GreatBall or Items.PokeBall           => HandleOTChange(ref pk, trainerName, tid, sid),
            >= Items.Antidote and <= Items.ParalyzeHeal when pk is not PA9 => HandleShinify(ref pk),
            >= Items.FullHeal and <= Items.Lemonade
            or Items.PokeDoll or Items.Revive
            or Items.FreshWater or Items.SodaPop when pk is not PA9        => HandleStatChange(ref pk),
            >= Items.GuardSpec and <= Items.XSpDef                         => HandleLanguageChange(ref pk),
            >= Items.LonelyMint and <= Items.SeriousMint                   => HandleNatureChange(ref pk, detail, caller),
            >= Items.NormalTeraShard and <= Items.FairyTeraShard
            or Items.StellarTeraShard                                      => HandleTeraChange(ref pk),
            _                                                              => CheckNicknameRequests(ref pk, trainerName, tid, sid, detail, caller)
        };

        if (result is not SpecialTradeType.None)
            FinalizePokemon(ref pk, caller, detail, result);

        return result;
    }

    private static SpecialTradeType HandleOTChange<T>(ref T pk, string trainerName, uint tid, uint sid) where T : PKM, new()
    {
        if (pk.HeldItem is Items.UltraBall or Items.PokeBall)
            pk.ClearNickname();

        if (pk.HeldItem is Items.UltraBall or Items.GreatBall)
        {
            pk.OriginalTrainerName = trainerName;
            if (pk is not PA9)
                pk.DisplayTID = tid;
            pk.DisplaySID = sid;
        }

        return SpecialTradeType.SanitizeReq;
    }

    private static SpecialTradeType HandleShinify<T>(ref T pk) where T : PKM, new()
    {
        if (pk.HeldItem == Items.ParalyzeHeal)
        {
            pk.SetUnshiny();
            return SpecialTradeType.Shinify;
        }

        var shinyType = pk.HeldItem is Items.BurnHeal or Items.Awakening || pk.IsEgg
            ? Shiny.AlwaysSquare
            : Shiny.AlwaysStar;

        if (pk.HeldItem is Items.Awakening)
            pk.IVs = [31, 31, 31, 31, 31, 31];

        if (pk is PK8 && !pk.IsEgg)
            ApplyPK8ShinyLogic(pk, shinyType);
        else
            CommonEdits.SetShiny(pk, shinyType);

        return SpecialTradeType.Shinify;
    }

    private static SpecialTradeType HandleStatChange<T>(ref T pk) where T : PKM, new()
    {
        pk.IVs = pk.HeldItem switch
        {
            Items.FullHeal   => [31, 31, 31, 31, 31, 31],
            Items.Revive     => [31,  0, 31,  0, 31, 31],
            Items.FreshWater => [31,  0, 31, 31, 31, 31],
            Items.PokeDoll   => [31, 31, 31,  0, 31, 31],
            _                => [31, 31, 31, 31, 31, 31]
        };

        if (pk.HeldItem is Items.SodaPop or Items.Lemonade)
            pk.CurrentLevel = 100;

        if (pk is IHyperTrain iht)
            iht.HyperTrainClear();

        return SpecialTradeType.StatChange;
    }

    private static SpecialTradeType HandleLanguageChange<T>(ref T pk) where T : PKM, new()
    {
        if (LanguageItems.TryGetValue(pk.HeldItem, out var language))
        {
            pk.Language = (int)language;
            pk.ClearNickname();
            return SpecialTradeType.SanitizeReq;
        }
        return SpecialTradeType.None;
    }

    private static SpecialTradeType HandleNatureChange<T>(ref T pk, PokeTradeDetail<T> detail, PokeRoutineExecutor<T> caller) where T : PKM, new()
    {
        var items = GameInfo.GetStrings(GameLanguage.DefaultLanguage).GetItemStrings((EntityContext)8, GameVersion.SWSH);
        var itemName = items[pk.HeldItem];
        var natureName = itemName.Split(' ')[0];

        if (!Enum.TryParse<Nature>(natureName, out var nature))
        {
            detail.SendNotification(caller, "Nature request was not found in the db.");
            return SpecialTradeType.FailReturn;
        }

        if (pk is PA9 pa)
            pa.StatNature = nature;
        else
            pk.Nature = pk.StatNature = nature;
        return SpecialTradeType.StatChange;
    }

    private static SpecialTradeType HandleTeraChange<T>(ref T pk) where T : PKM, new()
    {
        if (pk is not PK9 pk9) return SpecialTradeType.None;

        if (pk.HeldItem == Items.StellarTeraShard)
            pk9.TeraTypeOverride = (MoveType)TeraTypeUtil.Stellar;
        else if (TeraShardTypes.TryGetValue(pk.HeldItem, out var teraType))
            pk9.TeraTypeOverride = teraType;

        return SpecialTradeType.TeraChange;
    }

    private static SpecialTradeType CheckNicknameRequests<T>(ref T pk, string trainerName, uint tid, uint sid, PokeTradeDetail<T> detail, PokeRoutineExecutor<T> caller) where T : PKM, new()
    {
        if (pk is not PA8)
            return HandleSpecialCharacterRequests(ref pk, detail, caller);

        var nickname = pk.Nickname.ToLower();

        return nickname switch
        {
            var n when n.StartsWith("clear") => HandleClearRequest(ref pk, trainerName, tid, sid),
            var n when n.Contains("male")    => HandleGenderRequest(ref pk),
            var n when n.StartsWith("make")  => HandleMakeShinyRequest(ref pk),
            var n when n.StartsWith("stats") => HandleStatsRequest(ref pk),
            var n when n.Contains("nat")     => HandleNatureNicknameRequest(ref pk),
            var n when n.Contains("lang")    => HandleLanguageNicknameRequest(ref pk),
            _ => SpecialTradeType.None
        };
    }

    private static SpecialTradeType HandleClearRequest<T>(ref T pk, string trainerName, uint tid, uint sid) where T : PKM, new()
    {
        var nickname = pk.Nickname.ToLower();

        if (nickname.Contains("both") || nickname.Contains("ot"))
        {
            pk.ClearNickname();
            pk.OriginalTrainerName = trainerName;
            pk.DisplayTID = tid;
            pk.DisplaySID = sid;
        }
        else if (nickname.Contains("nick"))
        {
            pk.ClearNickname();
        }

        return SpecialTradeType.SanitizeReq;
    }

    private static SpecialTradeType HandleGenderRequest<T>(ref T pk) where T : PKM, new()
    {
        pk.Gender = (byte)(pk.Nickname.Contains("!female", StringComparison.OrdinalIgnoreCase) ? 1 : 0);
        return SpecialTradeType.StatChange;
    }

    private static SpecialTradeType HandleMakeShinyRequest<T>(ref T pk) where T : PKM, new()
    {
        var nickname = pk.Nickname.ToLower();
        pk.ClearNickname();

        if (nickname.Contains("normal"))
        {
            pk.SetUnshiny();
        }
        else
        {
            if (nickname.Contains("shiny6"))
                pk.IVs = [31, 31, 31, 31, 31, 31];
            CommonEdits.SetShiny(pk, Shiny.AlwaysSquare);
        }

        return SpecialTradeType.Shinify;
    }

    private static SpecialTradeType HandleStatsRequest<T>(ref T pk) where T : PKM, new()
    {
        var nickname = pk.Nickname.ToLower();
        pk.ClearNickname();

        pk.IVs = nickname switch
        {
            var s when s.Contains("6iv")  => [31, 31, 31, 31, 31, 31],
            var s when s.Contains("5ivs") => [31, 31, 31,  0, 31, 31],
            var s when s.Contains("5iva") => [31,  0, 31, 31, 31, 31],
            var s when s.Contains("4iv")  => [31,  0, 31,  0, 31, 31],
            _                             => [31, 31, 31, 31, 31, 31]
        };

        if (nickname.Contains("lvl100") || nickname.Contains("max"))
            pk.CurrentLevel = 100;

        if (pk is IHyperTrain iht)
            iht.HyperTrainClear();

        return SpecialTradeType.StatChange;
    }

    private static SpecialTradeType HandleNatureNicknameRequest<T>(ref T pk) where T : PKM, new()
    {
        var nickname = pk.Nickname.ToLower();

        foreach (var (key, nature) in NatureMap)
        {
            if (nickname.Contains(key))
            {
                pk.ClearNickname();
                pk.Nature = pk.StatNature = nature;
                return SpecialTradeType.StatChange;
            }
        }

        return SpecialTradeType.StatChange;
    }

    private static SpecialTradeType HandleLanguageNicknameRequest<T>(ref T pk) where T : PKM, new()
    {
        var nickname = pk.Nickname.ToUpper();

        foreach (var (key, lang) in LanguageNicknameMap)
        {
            if (nickname.Contains(key))
            {
                pk.ClearNickname();
                pk.Language = (int)lang;
                return SpecialTradeType.SanitizeReq;
            }
        }

        return SpecialTradeType.SanitizeReq;
    }

    private static SpecialTradeType HandleSpecialCharacterRequests<T>(ref T pk, PokeTradeDetail<T> detail, PokeRoutineExecutor<T> caller) where T : PKM, new()
    {
        if (pk.Nickname.StartsWith('?') || pk.Nickname.StartsWith('？'))
            return HandleBallRequest(ref pk, detail, caller);

        if (IsTechnicalMachine<T>(pk.HeldItem))
        {
            if (pk is ITechRecord t)
                t.SetRecordFlagsAll();

            if (pk is PA9 pa)
            {
                var la = new LegalityAnalysis(pa);
                if (pa is IPlusRecord pr && pa.PersonalInfo is IPermitPlus p)
                    pr.SetPlusFlags(p, PlusRecordApplicatorOption.LegalSeedTM, la);
            }

            return SpecialTradeType.StatChange;
        }

        return SpecialTradeType.None;
    }

    private static SpecialTradeType HandleBallRequest<T>(ref T pk, PokeTradeDetail<T> detail, PokeRoutineExecutor<T> caller) where T : PKM, new()
    {
        var itemLookup = pk.Nickname[1..].Replace(" ", string.Empty);
        var balls = GameInfo.GetStrings(GameLanguage.DefaultLanguage).balllist;

        int ball = Array.FindIndex(balls, z => z.Replace(" ", string.Empty).StartsWith(itemLookup, StringComparison.OrdinalIgnoreCase));

        if (ball < 0)
        {
            detail.SendNotification(caller, "Ball request was invalid. Check spelling & generation.");
            return SpecialTradeType.None;
        }

        pk.Ball = (byte)ball;
        return SpecialTradeType.BallReq;
    }

    private static bool IsTechnicalMachine<T>(int heldItem) where T : PKM =>
    (typeof(T) == typeof(PA9) && ItemStorage9ZA.GetInventoryPouch((ushort)heldItem) is InventoryType.TMHMs) ||
    (typeof(T) == typeof(PK9) && ItemStorage9SV.GetInventoryPouch((ushort)heldItem) is InventoryType.TMHMs) ||
    (typeof(T) == typeof(PK8) && ItemStorage8SWSH.IsTechRecord((ushort)heldItem)) ||
    (typeof(T) == typeof(PB8) && ItemStorage8BDSP.GetInventoryPouch((ushort)heldItem) is InventoryType.TMHMs);

    private static void ApplyPK8ShinyLogic<T>(T pk, Shiny shinyType) where T : PKM
    {
        try
        {
            int tidsid = int.Parse($"{pk.DisplaySID:D4}{pk.DisplayTID:D6}");
            int tid5 = Math.Abs(tidsid % 65536);
            int sid5 = Math.Abs(tidsid / 65536);

            if (shinyType == Shiny.AlwaysStar)
                CommonEdits.SetShiny(pk, shinyType);
            else
                pk.PID = (uint)(((tid5 ^ sid5 ^ (pk.PID & 0xFFFF)) << 16) | (pk.PID & 0xFFFF));

            // Special star shiny for Max Lair legendaries
            uint shinyForm = (uint)(pk.TID16 ^ pk.SID16 ^ ((pk.PID >> 16) ^ (pk.PID & 0xFFFF)));
            if (MaxLairLegendaries.Contains(pk.Species) && shinyForm < 16 && pk.Form != 1)
                pk.PID = (uint)(((tid5 ^ sid5 ^ (pk.PID & 0xFFFF) ^ 1) << 16) | (pk.PID & 0xFFFF));
        }
        catch
        {
            CommonEdits.SetShiny(pk, shinyType);
        }
    }

    private static void FinalizePokemon<T>(ref T pk, PokeRoutineExecutor<T> caller, PokeTradeDetail<T> detail, SpecialTradeType tradeType) where T : PKM, new()
    {
        if (pk is ITechRecord t && !pk.IsEgg)
            t.SetRecordFlags(pk.Moves);

        if (tradeType is not SpecialTradeType.ItemReq && !pk.IsEgg && pk is not (PB7 or PA8))
            pk.HeldItem = Items.MasterBall;

        LegalizeIfNotLegal(ref pk, caller, detail);
    }

    private static void LegalizeIfNotLegal<T>(ref T pkm, PokeRoutineExecutor<T> caller, PokeTradeDetail<T> detail) where T : PKM, new()
    {
        var tempPk = pkm.Clone();
        var la = new LegalityAnalysis(pkm);

        if (la.Valid) return;

        detail.SendNotification(caller, "This request isn't legal! Attemping to legalize...");
        caller.Log(la.Report());

        try
        {
            pkm = (T)pkm.LegalizePokemon();
            pkm.OriginalTrainerName = tempPk.OriginalTrainerName;

            la = new LegalityAnalysis(pkm);
            if (!la.Valid)
                pkm = (T)pkm.LegalizePokemon();
        }
        catch (Exception e)
        {
            caller.Log("Legalization failed: " + e.Message);
        }
    }

    private static void LogHeldItem<T>(T pk, PokeRoutineExecutor<T> caller) where T : PKM, new()
    {
        var items = GameInfo.GetStrings(GameLanguage.DefaultLanguage).GetItemStrings((EntityContext)8, GameVersion.SWSH);

        var message = pk.HeldItem > Items.None && pk.HeldItem < items.Length
                ? $"Item held: {items[pk.HeldItem]}"
                : $"Held item was outside bounds or nothing held: {pk.HeldItem}";

        caller.Log(message);
    }

    public class Items
    {
        public const int None = 0;
        public const int MasterBall = 1;
        public const int UltraBall = 2;
        public const int GreatBall = 3;
        public const int PokeBall = 4;
        public const int Antidote = 18;
        public const int BurnHeal = 19;
        public const int Awakening = 21;
        public const int ParalyzeHeal = 22;
        public const int FullHeal = 27;
        public const int Revive = 28;
        public const int FreshWater = 30;
        public const int SodaPop = 31;
        public const int Lemonade = 32;
        public const int PokeDoll = 63;
        public const int GuardSpec = 55;
        public const int DireHit = 56;
        public const int XAtk = 57;
        public const int XDef = 58;
        public const int XSpe = 59;
        public const int XAcc = 60;
        public const int XSpAtk = 61;
        public const int XSpDef = 62;
        public const int LonelyMint = 1231;
        public const int SeriousMint = 1251;
        public const int NormalTeraShard = 1862;
        public const int FireTeraShard = 1863;
        public const int WaterTeraShard = 1864;
        public const int ElectricTeraShard = 1865;
        public const int GrassTeraShard = 1866;
        public const int IceTeraShard = 1867;
        public const int FightingTeraShard = 1868;
        public const int PoisonTeraShard = 1869;
        public const int GroundTeraShard = 1870;
        public const int FlyingTeraShard = 1871;
        public const int PsychicTeraShard = 1872;
        public const int BugTeraShard = 1873;
        public const int RockTeraShard = 1874;
        public const int GhostTeraShard = 1875;
        public const int DragonTeraShard = 1876;
        public const int DarkTeraShard = 1877;
        public const int SteelTeraShard = 1878;
        public const int FairyTeraShard = 1879;
        public const int StellarTeraShard = 2549;
    }
}
