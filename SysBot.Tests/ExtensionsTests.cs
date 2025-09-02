using FluentAssertions;
using PKHeX.Core;
using SysBot.Pokemon;
using System;
using System.Linq;
using Xunit;

namespace SysBot.Tests;

public class ExtensionsTests
{
    static ExtensionsTests() => AutoLegalityWrapper.EnsureInitialized(new Pokemon.LegalitySettings());

    [Theory]
    [InlineData("Eternatus", typeof(PK8), FullTrainerName, ShortTrainerName)]
    [InlineData("Manaphy", typeof(PB8), FullTrainerName, ShortTrainerName)]
    [InlineData("Arceus", typeof(PA8), FullTrainerName, ShortTrainerName)]
    [InlineData("Terapagos", typeof(PK9), FullTrainerName, ShortTrainerName)]
    public void CanClearTrainerTrash(string set, Type type, string fullTrainerName, string shortTrainerName)
    {
        var sav = type switch
        {
            var t when t == typeof(PK8) => AutoLegalityWrapper.GetTrainerInfo<PK8>(),
            var t when t == typeof(PB8) => AutoLegalityWrapper.GetTrainerInfo<PB8>(),
            var t when t == typeof(PA8) => AutoLegalityWrapper.GetTrainerInfo<PA8>(),
            var t when t == typeof(PK9) => AutoLegalityWrapper.GetTrainerInfo<PK9>(),
            _ => throw new ArgumentException("Invalid type", nameof(type))
        };

        var s = new ShowdownSet(set);
        var template = AutoLegalityWrapper.GetTemplate(s);
        PKM pk = sav.GetLegal(template, out _);
        pk.Should().NotBeNull();

        pk.OriginalTrainerName = fullTrainerName;
        CountTrashChars(pk).Should().Be(0);
        pk.OriginalTrainerName = shortTrainerName;
        CountTrashChars(pk).Should().BeGreaterThan(0);

        var trashFixed = type switch
        {
            var t when t == typeof(PK8) => TradeExtensions<PK8>.FixTrashChars(((PK8)pk)),
            var t when t == typeof(PB8) => TradeExtensions<PB8>.FixTrashChars(((PB8)pk)),
            var t when t == typeof(PA8) => TradeExtensions<PA8>.FixTrashChars(((PA8)pk)),
            var t when t == typeof(PK9) => TradeExtensions<PK9>.FixTrashChars(((PK9)pk)),
            _ => throw new ArgumentException("Invalid type", nameof(type))
        };

        CountTrashChars(trashFixed).Should().Be(0);
    }

    private static int CountTrashChars(PKM pkm)
    {
        const int MaxTrashCount = 0x1A;

        var offset = pkm.Context switch
        {
            EntityContext.Gen8 => 0xF8,
            EntityContext.Gen8b => 0xF8,
            EntityContext.Gen8a => 0x110,
            EntityContext.Gen9 => 0xF8,
            _ => throw new ArgumentException("Invalid gen context"),
        };

        var trainerNameLength = pkm.OriginalTrainerName.Length * 2;

        if (trainerNameLength == MaxTrashCount)
            return 0;

        var slice = pkm.Data.Slice(offset + trainerNameLength, MaxTrashCount - trainerNameLength);

        int count = 0;
        for (int i = 0; i < slice.Length; i++)
        {
            if (slice[i] != 0)
                count++;
        }
        return count;
    }

    private const string FullTrainerName = "MANUMANUMANU";
    private const string ShortTrainerName = "Manu";
}
