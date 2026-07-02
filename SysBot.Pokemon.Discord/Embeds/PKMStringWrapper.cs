using PKHeX.Core;

namespace SysBot.Pokemon.Discord;

internal class PKMStringWrapper<T>(T PKM, TradeEmbedSettings Config, PokeTradeType type) where T : PKM, new()
{
    protected GameStrings GameStrings =>
        GameInfo.GetStrings(Language.GetLanguageCode(Config.ForceEmbedLanguage is LanguageID.None ? (LanguageID)PKM.Language : Config.ForceEmbedLanguage));

    internal string Species => GetSpeciesString();
    internal string Form => GetFormString();
    internal string Shiny => GetShinyString();
    internal string Gender => GetGenderString();
    internal string Scale => GetScaleString();
    internal string TeraType => GetTeraTypeString();
    internal string FormArgument => GetFormArgumentString();

    internal string Ability => type == PokeTradeType.MysteryEgg ? "Unknown" : GameStrings.Ability[PKM.Ability];
    internal string Nature => GameStrings.Natures[(byte)PKM.StatAlignment];
    internal string HeldItem => GameStrings.Item[PKM.HeldItem];

    internal bool HasForm = PKM.Form > 0;
    internal bool HasTeraType => PKM is ITeraType { TeraType: > MoveType.Any };
    internal bool HasItem => PKM.HeldItem > 0;
    internal PokemonMark Mark => new(PKM);
    internal List<string> Moves => GetMovesStrings();

    private string GetSpeciesString()
    {
        var species = $"{SpeciesName.GetSpeciesName(PKM.Species, PKM.Language)}";
        return type == PokeTradeType.MysteryEgg ? "Unknown" : $"{species}";
    }

    private string GetFormString()
    {
        var forms = FormConverter.GetFormList(PKM.Species, GameStrings.types, GameStrings.forms, GameInfo.GenderSymbolASCII, PKM.Context);
        var form = forms[PKM.Form];
        return form;
    }

    private string GetFormArgumentString()
    {
        if (PKM is IFormArgument pkarg)
        {
            return pkarg.FormArgument switch
            {
                0 => "Strawberry",
                1 => "Berry",
                2 => "Love",
                3 => "Star",
                4 => "Clover",
                5 => "Flower",
                6 => "Ribbon",
                _ => ""
            };
        }
        return "";
    }

    private string GetShinyString() =>
        PKM.ShinyXor == 0 && PKM is PK8 ? "■ " : PKM.IsShiny ? "★ " : "";

    private string GetGenderString() => Config.UseGenderEmoji switch
    {
        true => $" <:{(Gender)PKM.Gender}GenderEmoji:{Config.GenderEmojiCodes.GetEmojiCode(PKM.Gender)}>",
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
                var emoji = $"<:{type}TypeEmoji:{Config.MoveTypesEmojiCodes.GetEmojiCode(type)}>";
                var plusEmoji = $" <:PlusMoveEmoji:{Config.PlusMoveEmojiCode}>";
                var name = GameStrings.movelist[move];

                if (PKM is PA9 pa9 && pa9.PersonalInfo is IPermitPlus plus)
                {
                    var index = plus.PlusMoveIndexes.IndexOf(move);

                    if (pa9.GetMovePlusFlag(index))
                        name += Config.UsePlusMoveEmoji ? plusEmoji : " ***+***";
                }

                var pp = Config.ShowMovePP && PKM is not PA9 ? i switch
                {
                    0 => $"({PKM.Move1_PP} PP)",
                    1 => $"({PKM.Move2_PP} PP)",
                    2 => $"({PKM.Move3_PP} PP)",
                    3 => $"({PKM.Move4_PP} PP)",
                    _ => throw new ArgumentOutOfRangeException(nameof(i), "Invalid move index.")
                } : "";
                var prefix = Config.UseMoveEmoji ? emoji : "\\-";
                moves.Add($"{prefix} {name} {pp}");
            }
        }
        return moves;
    }

    private string GetScaleString() => PKM switch
    {
        PK9 s => $"{PokeSizeDetailedUtil.GetSizeRating(s.Scale)} ({s.Scale})",
        PA8 a => $"{PokeSizeUtil.GetSizeRating(a.Scale)} ({a.Scale})",
        PA9 a => $"{PokeSizeUtil.GetSizeRating(a.Scale)} ({a.Scale})",
        _ => string.Empty
    };

    private string GetTeraTypeString()
    {
        if (PKM is ITeraType tera)
        {
            var type = (GemType)(tera.TeraType + 2);
            var tName = GameStrings.types[type is GemType.Stellar ? 18 : (int)(type - 2)];

            return $"{tName}{(Config.UseTeraEmoji ? $" <:{type}TeraEmoji:{Config.TeraTypesEmojiCodes.GetEmojiCode(type)}>" : string.Empty)}";
        }
        return "";
    }

    internal string GetAuthorText(string trader)
    {
        return type switch
        {
            PokeTradeType.Clone          => "Cloning Pod Activated",
            PokeTradeType.Dump           => "Pokémon Scanner Activated",
            PokeTradeType.ItemTrade      => $"{trader}'s {HeldItem}",
            PokeTradeType.MysteryEgg     => $"{trader}'s Mystery Egg",
            PokeTradeType.Seed           => $"Seed Checker Activated",
            PokeTradeType.SpecialRequest => $"Special Request Initiated",
            PokeTradeType.Specific or
            PokeTradeType.Giveaway       => $"{trader}'s {(PKM.IsShiny ? "Shiny Pokémon" : $"Pokémon {(PKM.IsEgg ? "Egg" : "")}")}",
            _ => string.Empty
        };
    }

    internal string GetImageURL()
    {
        return type switch
        {
            PokeTradeType.Specific   => GetPokemonImageURL(PKM.IsEgg),
            PokeTradeType.Clone      => "https://raw.githubusercontent.com/Omni-KingZeno/Pokemon-Sprites/refs/heads/main/Bot/clone.png",
            PokeTradeType.Dump       => "https://raw.githubusercontent.com/Omni-KingZeno/Pokemon-Sprites/refs/heads/main/Bot/dump.gif",
            PokeTradeType.MysteryEgg => "https://raw.githubusercontent.com/Omni-KingZeno/HomeImages/refs/heads/main/Sprites/128x128/MysteryEgg.png",
            PokeTradeType.ItemTrade  => GetPokemonImageURL(PKM.IsEgg),
            PokeTradeType.Seed       => "https://github.com/Omni-KingZeno/Pokemon-Sprites/blob/main/Bot/seedcheck.gif?raw=true",
            PokeTradeType.SpecialRequest => "https://raw.githubusercontent.com/Omni-KingZeno/Pokemon-Sprites/f628d1603de38365204b3d1c609d3315d82a3f5a/Bot/SpecialRequest.png",
            _ => string.Empty,
        };
    }

    internal string GetThumbnailURL()
    {
        return type switch
        {
            PokeTradeType.Specific   => HasItem ? GetItemImgURL(HeldItem) : string.Empty,
            PokeTradeType.Clone      => "https://raw.githubusercontent.com/Omni-KingZeno/Pokemon-Sprites/refs/heads/main/Bot/clone.png",
            PokeTradeType.Dump       => "https://raw.githubusercontent.com/Omni-KingZeno/Pokemon-Sprites/refs/heads/main/Bot/dump.gif",
            PokeTradeType.MysteryEgg => "https://raw.githubusercontent.com/Omni-KingZeno/HomeImages/refs/heads/main/Sprites/128x128/MysteryEgg.png",
            PokeTradeType.ItemTrade  => GetItemImgURL(HeldItem),
            PokeTradeType.Seed       => "https://github.com/Omni-KingZeno/Pokemon-Sprites/blob/main/Bot/seedcheck.gif?raw=true",
            PokeTradeType.SpecialRequest => "https://raw.githubusercontent.com/Omni-KingZeno/Pokemon-Sprites/f628d1603de38365204b3d1c609d3315d82a3f5a/Bot/SpecialRequest.png",
            _ => string.Empty,
        };
    }

    internal string GetPokemonImageURL(bool isEgg) =>
        TradeExtensions<T>.GetPokemonImageURL(PKM, PKM is IGigantamax { } g && g.CanGigantamax, Config.UseFullSizeImages, isEgg);

    internal string GetBallImageURL() =>
        "https://raw.githubusercontent.com/Omni-KingZeno/HomeImages/refs/heads/main/Ballimg/50x50/" + $"{(Ball)PKM.Ball}ball.png".ToLower();

    internal string GetMarkImageURL() =>
       PKM is PA9 { IsAlpha: true } or PA8 { IsAlpha: true } ? "https://www.serebii.net/pokearth/hisui/icons/alphaza.png" : Mark.HasMark ? $"https://www.serebii.net/scarletviolet/ribbons/{(Mark.Name.ToLower())}mark.png" : string.Empty;

    internal string GetItemImgURL(string item)
    {
        item = item.Replace(" ", "").ToLower();
        return $"https://raw.githubusercontent.com/Omni-KingZeno/Pokemon-Sprites/refs/heads/main/Items/{item}.png";
    }
}
