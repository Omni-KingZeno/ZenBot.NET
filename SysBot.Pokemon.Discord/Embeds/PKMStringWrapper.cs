using Microsoft.VisualBasic;
using PKHeX.Core;

namespace SysBot.Pokemon.Discord;

internal class PKMStringWrapper<T>(T PKM, TradeEmbedSettings Config, bool mysteryEgg) where T : PKM, new()
{
    protected GameStrings GameStrings =>
        GameInfo.GetStrings(Language.GetLanguageCode(Config.ForceEmbedLanguage is LanguageID.None ? (LanguageID)PKM.Language : Config.ForceEmbedLanguage));

    internal string Species => GetSpeciesString();
    internal string Form => GetFormString();
    internal string Shiny => GetShinyString();
    internal string Gender => GetGenderString();
    internal string Scale => GetScaleString();
    internal string TeraType => GetTeraTypeString();

    internal string Ability => mysteryEgg ? "Unknown" : GameStrings.Ability[PKM.Ability];
    internal string Nature => GameStrings.Natures[(byte)PKM.StatNature];
    internal string HeldItem => GameStrings.Item[PKM.HeldItem];

    internal bool HasForm = PKM.Form > 0;
    internal bool HasTeraType => PKM is ITeraType { TeraType: > MoveType.Any };
    internal bool HasItem => PKM.HeldItem > 0;
    internal PokemonMark Mark => new(PKM);
    internal List<string> Moves => GetMovesStrings();

    private string GetSpeciesString()
    {
        var species = $"{SpeciesName.GetSpeciesName(PKM.Species, PKM.Language)}";
        return mysteryEgg ? "Unknown" : $"{species}";
    }

    private string GetFormString()
    {
        var forms = FormConverter.GetFormList(PKM.Species, GameStrings.types, GameStrings.forms, GameInfo.GenderSymbolASCII, PKM.Context);
        var form = forms[PKM.Form];
        return form;
    }

    private string GetShinyString() =>
        PKM.ShinyXor == 0 ? "■ " : PKM.IsShiny ? "★ " : "";

    private string GetGenderString() => Config.UseGenderEmoji switch
    {
        true => $"<:GenderEmoji:{Config.GenderEmojiCodes.GetEmojiCode(PKM.Gender)}>",
        _ => (Gender)PKM.Gender != PKHeX.Core.Gender.Genderless ? $" {GameInfo.GenderSymbolUnicode[PKM.Gender]}" : ""
    };

    private List<string> GetMovesStrings()
    {
        var moves = new List<string>();
        for (int i = 0; i < PKM.Moves.Length; i++)
        {
            if (PKM.Moves[i] is { } move && move is not (ushort)Move.None)
            {
                var type = (MoveType)MoveInfo.GetType(move, PKM.Context);
                var emoji = $"{(Config.UseMoveEmoji ? $"<:TypeEmoji:{Config.MoveTypesEmojiCodes.GetEmojiCode(type)}> " : "")}";
                var name = GameStrings.movelist[move];
                var pp = Config.ShowMovePP ? i switch
                {
                    0 => $"({PKM.Move1_PP} PP)",
                    1 => $"({PKM.Move2_PP} PP)",
                    2 => $"({PKM.Move3_PP} PP)",
                    3 => $"({PKM.Move4_PP} PP)",
                    _ => throw new ArgumentOutOfRangeException(nameof(i), "Invalid move index.")
                } : "";
                moves.Add($"\\- {emoji}{name} {pp}");
            }
        }
        return moves;
    }

    private string GetScaleString() => PKM switch
    {
        var pk when pk is IScaledSize3 { } s => $"{PokeSizeDetailedUtil.GetSizeRating(s.Scale)} ({s.Scale})",
        var pk when pk is IScaledSize { } hw => $"{PokeSizeDetailedUtil.GetSizeRating(hw.HeightScalar)} ({hw.HeightScalar})",
        _ => throw new NotSupportedException("Unsupported PKM type for scale string.")
    };

    private string GetTeraTypeString()
    {
        if (PKM is ITeraType tera)
        {
            var type = (GemType)(tera.TeraType + 2);
            if (Config.UseTeraEmoji)
                return $"<:TypeEmoji:{Config.TeraTypesEmojiCodes.GetEmojiCode(type)}>";
            return $"{GameStrings.types[type is GemType.Stellar ? 18 : (int)(type - 2)]}";
        }
        return "";
    }

    internal string GetPokemonImageURL(bool isEgg, bool isMysteryEgg) =>
        TradeExtensions<T>.GetPokemonImageURL(PKM, PKM is IGigantamax { } g && g.CanGigantamax, fullSize: false, isEgg, isMysteryEgg);

    internal string GetBallImageURL() =>
        "https://raw.githubusercontent.com/Omni-KingZeno/HomeImages/refs/heads/main/Ballimg/50x50/" + $"{(Ball)PKM.Ball}ball.png".ToLower();
    internal string GetMarkImageURL() =>
       PKM is PA9 { IsAlpha: true } or PA8 { IsAlpha: true } ? "https://www.serebii.net/pokearth/hisui/icons/alphaza.png" : Mark.HasMark ? $"https://www.serebii.net/scarletviolet/ribbons/{(Mark.Name.ToLower())}mark.png" : string.Empty;

    internal string GetItemImgURL(string item, bool smallsize)
    {
        item = item.Replace(" ", "").ToLower();

        string? baseLink;
        if (smallsize)
        {
            baseLink = $"https://www.serebii.net/itemdex/sprites/{item}.png";
        }
        else
        {
            baseLink = $"https://www.serebii.net/itemdex/sprites/sv/{item}.png";
        }
        return baseLink;
    }
}
