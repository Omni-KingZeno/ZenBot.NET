using System;
using System.Collections.Generic;
using System.Linq;
using FluentAssertions;
using PKHeX.Core;
using SysBot.Pokemon;
using Xunit;

namespace SysBot.Tests;

public class GenerateTests
{
    static GenerateTests() => AutoLegalityWrapper.EnsureInitialized(new Pokemon.LegalitySettings());

    [Theory]
    [InlineData(Gengar)]
    [InlineData(Braviary)]
    [InlineData(Drednaw)]
    public void CanGenerate(string set)
    {
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK8>();
        var s = new ShowdownSet(set);
        var template = AutoLegalityWrapper.GetTemplate(s);
        var pk = sav.GetLegal(template, out _);
        pk.Should().NotBeNull();
    }

    [Theory]
    [InlineData(InvalidSpec)]
    public void ShouldNotGenerate(string set)
    {
        _ = AutoLegalityWrapper.GetTrainerInfo<PK8>();
        var s = ShowdownUtil.ConvertToShowdown(set);
        s.Should().BeNull();
    }

    [Theory]
    [InlineData(Torkoal2, 2)]
    [InlineData(Charizard4, 4)]
    public void TestAbility(string set, int abilNumber)
    {
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK8>();
        for (int i = 0; i < 10; i++)
        {
            var s = new ShowdownSet(set);
            var template = AutoLegalityWrapper.GetTemplate(s);
            var pk = sav.GetLegal(template, out _);
            pk.AbilityNumber.Should().Be(abilNumber);
        }
    }

    [Theory]
    [InlineData(Torkoal2, 2)]
    [InlineData(Charizard4, 4)]
    public void TestAbilityTwitch(string set, int abilNumber)
    {
        var sav = AutoLegalityWrapper.GetTrainerInfo<PK8>();
        for (int i = 0; i < 10; i++)
        {
            var twitch = set.Replace("\r\n", " ").Replace("\n", " ");
            var s = ShowdownUtil.ConvertToShowdown(twitch);
            var template = s == null ? null : AutoLegalityWrapper.GetTemplate(s);
            var pk = template == null ? null : sav.GetLegal(template, out _);
            pk.Should().NotBeNull();
            pk.AbilityNumber.Should().Be(abilNumber);
        }
    }

    private const string Gengar =
        @"Gengar-Gmax @ Life Orb 
Ability: Cursed Body 
Shiny: Yes 
EVs: 252 SpA / 4 SpD / 252 Spe 
Timid Nature 
- Dream Eater 
- Fling 
- Giga Impact 
- Headbutt";

    private const string Braviary =
        @"Braviary (F) @ Master Ball
Ability: Defiant
EVs: 252 Atk / 4 SpD / 252 Spe
Jolly Nature
- Brave Bird
- Close Combat
- Tailwind
- Iron Head";

    private const string Drednaw =
        @"Drednaw-Gmax @ Fossilized Drake 
Ability: Shell Armor 
Level: 60 
EVs: 252 Atk / 4 SpD / 252 Spe 
Adamant Nature 
- Earthquake 
- Liquidation 
- Swords Dance 
- Head Smash";

    private const string Torkoal2 =
        @"Torkoal (M) @ Assault Vest
IVs: 0 Atk
EVs: 248 HP / 8 Atk / 252 SpA
Ability: Drought
Quiet Nature
- Body Press
- Earth Power
- Eruption
- Fire Blast";

    private const string Charizard4 =
        @"Charizard @ Choice Scarf 
Ability: Solar Power 
Level: 50 
Shiny: Yes 
EVs: 252 SpA / 4 SpD / 252 Spe 
Timid Nature 
- Heat Wave 
- Air Slash 
- Solar Beam 
- Beat Up";

    private const string InvalidSpec =
        "(Pikachu)";
}

public class GenerateValidSpeciesTests
{
    static GenerateValidSpeciesTests() => AutoLegalityWrapper.EnsureInitialized(new Pokemon.LegalitySettings());
    private static GameStrings Strings => GameInfo.GetStrings("en");

    [Theory]
    [InlineData(GameVersion.GG)]
    [InlineData(GameVersion.SWSH)]
    [InlineData(GameVersion.BDSP)]
    [InlineData(GameVersion.PLA)]
    [InlineData(GameVersion.SV)]
    [InlineData(GameVersion.ZA)]
    public void CanGenerateAllSpeciesForVersion(GameVersion version)
    {
        var sav = GetSaveForVersion(version);
        var personalTable = GetPersonalTableForVersion(version);
        var availableSpecies = GetAvailableSpeciesAndForms(version, personalTable, sav);

        var failedSpecies = availableSpecies
            .Select(sf => TryGenerateLegal(sf, version, sav))
            .Where(result => result != null)
            .ToList();

        if (failedSpecies.Count != 0)
        {
            var failureMessage = $"The following species/forms failed to generate legally for {version}:\n"
                + string.Join("\n", failedSpecies);
            Assert.Fail(failureMessage);
        }
    }

    private static string? TryGenerateLegal((ushort species, byte form) speciesForm, GameVersion version, ITrainerInfo sav)
    {
        var (species, form) = speciesForm;
        var speciesName = Strings.specieslist[species];

        if (string.IsNullOrWhiteSpace(speciesName))
            return null;

        var formName = ShowdownParsing.GetStringFromForm(form, Strings, species, version.GetContext());
        var fullName = string.IsNullOrEmpty(formName) ? speciesName : $"{speciesName}-{formName}";

        try
        {
            var showdownSet = new ShowdownSet(fullName);
            var template = AutoLegalityWrapper.GetTemplate(showdownSet);
            var pk = sav.GetLegal(template, out _);
            var la = new LegalityAnalysis(pk);

            return pk == null || !la.Valid ? $"{fullName} ({species}-{form})" : null;
        }
        catch (Exception ex)
        {
            return $"{fullName} ({species}-{form}): {ex.Message}";
        }
    }

    private static ITrainerInfo GetSaveForVersion(GameVersion version)
    {
        return version switch
        {
            GameVersion.GG => AutoLegalityWrapper.GetTrainerInfo<PB7>(),
            GameVersion.SWSH => AutoLegalityWrapper.GetTrainerInfo<PK8>(),
            GameVersion.BDSP => AutoLegalityWrapper.GetTrainerInfo<PB8>(),
            GameVersion.PLA => AutoLegalityWrapper.GetTrainerInfo<PA8>(),
            GameVersion.SV => AutoLegalityWrapper.GetTrainerInfo<PK9>(),
            GameVersion.ZA => AutoLegalityWrapper.GetTrainerInfo<PA9>(),
            _ => throw new ArgumentException($"Unsupported game version: {version}")
        };
    }

    private static IPersonalTable GetPersonalTableForVersion(GameVersion version)
    {
        return version switch
        {
            GameVersion.GG => PersonalTable.GG,
            GameVersion.SWSH => PersonalTable.SWSH,
            GameVersion.BDSP => PersonalTable.BDSP,
            GameVersion.PLA => PersonalTable.LA,
            GameVersion.SV => PersonalTable.SV,
            GameVersion.ZA => PersonalTable.ZA,
            _ => throw new ArgumentException($"Unsupported game version: {version}")
        };
    }

    private static List<(ushort species, byte form)> GetAvailableSpeciesAndForms(
        GameVersion version, IPersonalTable personalTable, ITrainerInfo sav)
    {
        var context = version.GetContext();
        var strings = GameInfo.Strings;
        var homeTransfers = GetHomeTransfers(version);

        return [.. Enumerable.Range(1, personalTable.MaxSpeciesID)
            .Where(s => personalTable.IsSpeciesInGame((ushort)s))
            .SelectMany(s => GetFormsForSpecies((ushort)s, personalTable, context, strings))
            .Where(sf => personalTable.IsPresentInGame(sf.species, sf.form)
                && !IsInvalidForm(sf.species, sf.form, sav)
                && !homeTransfers.Contains(sf))];
    }

    private static IEnumerable<(ushort species, byte form)> GetFormsForSpecies(
        ushort species, IPersonalTable personalTable, EntityContext context, GameStrings strings)
    {
        var baseEntry = personalTable.GetFormEntry(species, 0);
        var numForms = baseEntry.FormCount;

        if (numForms == 1)
            numForms = (byte)FormConverter.GetFormList(species, strings.types, strings.forms, GameInfo.GenderSymbolUnicode, context).Length;

        return Enumerable.Range(0, numForms).Select(f => (species, (byte)f));
    }

    private static bool IsInvalidForm(ushort species, byte form, ITrainerInfo sav)
    {
        return FormInfo.IsLordForm(species, form, sav.Context)
        || FormInfo.IsBattleOnlyForm(species, form, sav.Generation)
        || FormInfo.IsFusedForm(species, form, sav.Generation)
        || (FormInfo.IsTotemForm(species, form) && sav.Context is not EntityContext.Gen7);
    }

    private static HashSet<(ushort, int)> GetHomeTransfers(GameVersion version)
    {
        return version switch
        {
            GameVersion.SWSH =>
        [
            ((ushort)Species.Celebi, 0),
            ((ushort)Species.Diancie, 0),
            ((ushort)Species.Magearna, 0),
            ((ushort)Species.Zeraora, 0),
            ((ushort)Species.Meltan, 0),
            ((ushort)Species.Melmetal, 0),
        ],
            GameVersion.BDSP =>
        [
            ((ushort)Species.Celebi, 0),
            ((ushort)Species.Deoxys, 0),
            ((ushort)Species.Deoxys, 1),
            ((ushort)Species.Deoxys, 2),
            ((ushort)Species.Deoxys, 3),
            ((ushort)Species.Giratina, 1),
        ],
            GameVersion.SV => CreateSVHomeTransfers(),
            GameVersion.ZA =>
        [
            ((ushort)Species.Magearna, 1),
            ((ushort)Species.Gimmighoul, 1),
        ],
            _ => []
        };
    }

    private static HashSet<(ushort, int)> CreateSVHomeTransfers()
    {
        HashSet<(ushort, int)> transfers =
        [
            ((ushort)Species.Articuno, 1),
            ((ushort)Species.Zapdos, 1),
            ((ushort)Species.Moltres, 1),
            ((ushort)Species.Regirock, 0),
            ((ushort)Species.Regice, 0),
            ((ushort)Species.Registeel, 0),
            ((ushort)Species.Jirachi, 0),
            ((ushort)Species.Uxie, 0),
            ((ushort)Species.Mesprit, 0),
            ((ushort)Species.Azelf, 0),
            ((ushort)Species.Heatran, 0),
            ((ushort)Species.Regigigas, 0),
            ((ushort)Species.Giratina, 0),
            ((ushort)Species.Giratina, 1),
            ((ushort)Species.Cresselia, 0),
            ((ushort)Species.Manaphy, 0),
            ((ushort)Species.Shaymin, 0),
            ((ushort)Species.Shaymin, 1),
            ((ushort)Species.Tornadus, 0),
            ((ushort)Species.Tornadus, 1),
            ((ushort)Species.Thundurus, 0),
            ((ushort)Species.Thundurus, 1),
            ((ushort)Species.Landorus, 0),
            ((ushort)Species.Landorus, 1),
            ((ushort)Species.Diancie, 0),
            ((ushort)Species.Hoopa, 0),
            ((ushort)Species.Hoopa, 1),
            ((ushort)Species.Volcanion, 0),
            ((ushort)Species.Cosmog, 0),
            ((ushort)Species.Cosmoem, 0),
            ((ushort)Species.Magearna, 0),
            ((ushort)Species.Magearna, 1),
            ((ushort)Species.Zacian, 0),
            ((ushort)Species.Zamazenta, 0),
            ((ushort)Species.Eternatus, 0),
            ((ushort)Species.Zarude, 1),
            ((ushort)Species.Regieleki, 0),
            ((ushort)Species.Regidrago, 0),
            ((ushort)Species.Calyrex, 0),
            ((ushort)Species.Enamorus, 0),
            ((ushort)Species.Enamorus, 1),
            ((ushort)Species.Gimmighoul, 1),
        ];

        // Hat/Starter Pikachu forms
        for (byte f = 1; f <= 9; f++)
            transfers.Add(((ushort)Species.Pikachu, f));

        // Arceus forms
        for (byte f = 0; f <= 17; f++)
            transfers.Add(((ushort)Species.Arceus, f));

        return transfers;
    }
}
