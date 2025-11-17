using Discord;
using PKHeX.Core;
using SysBot.Pokemon.Discord.Helpers;
using static SysBot.Pokemon.TradeEmbedSettings;

namespace SysBot.Pokemon.Discord;

public class TradeEmbedBuilder<T>(T PKM, PokeTradeHub<T> Hub, QueueUser trader, PokeTradeType type) where T : PKM, new()
{
    private bool Initialized { get; set; } = false;
    private bool MysteryEgg => type == PokeTradeType.MysteryEgg;
    private bool ItemTrade => type == PokeTradeType.ItemTrade;
    public EmbedBuilder Builder { get; init; } = new();
    private PKMStringWrapper<T> Strings { get; init; } = new(PKM, Hub.Config.Discord.TradeEmbedSettings, type == PokeTradeType.MysteryEgg);

    public Embed Build()
    {
        if (!Initialized)
            InitializeEmbed();

        return Builder.Build();
    }

    public void InitializeEmbed()
    {
        // Embed layout Style
        var altStyle = Hub.Config.Discord.TradeEmbedSettings.UseAlternateLayout;
        Builder.Color = InitializeColor();
        Builder.Author = InitializeAuthor();
        Builder.Footer = InitializeFooter();
        Builder.ThumbnailUrl = altStyle ? ItemTrade ? Strings.GetPokemonImageURL(PKM.IsEgg, MysteryEgg) : Strings.HasItem ? Strings.GetItemImgURL(Strings.HeldItem, false) : "" : Strings.GetPokemonImageURL(PKM.IsEgg, MysteryEgg);
        Builder.ImageUrl = altStyle ? ItemTrade ? Strings.GetItemImgURL(Strings.HeldItem, false) : Strings.GetPokemonImageURL(PKM.IsEgg, MysteryEgg) : ItemTrade ? Strings.GetItemImgURL(Strings.HeldItem, false) : "";

        // Build field value based on EmbedDisplayedInfo setting
        var fieldValue = "";
        var moves = "";
        var displayedInfo = Hub.Config.Discord.TradeEmbedSettings.EmbedDisplayedInfo;

        foreach (var info in displayedInfo)
        {
            var line = GetDisplayInfoLine(info);
            if (!string.IsNullOrEmpty(line))
            {
                if (info == DisplayedInfo.Moves)
                {
                    moves = line; // Store moves separately for alternate layout
                }
                else
                {
                    fieldValue += line + Environment.NewLine;
                }
            }
        }

        if (altStyle)
        {
            if (!ItemTrade)
            {
                Builder.AddField(x =>
                {
                    x.Name = "__Details:__";
                    x.Value = fieldValue;
                    x.IsInline = true;
                });

                if (!string.IsNullOrEmpty(moves))
                {
                    Builder.AddField(x =>
                    {
                        x.Name = "__Moves:__";
                        x.Value = moves;
                        x.IsInline = true;
                    });
                }
            }
        }
        else
        {
            if (!ItemTrade)
            {
                Builder.Description = fieldValue += moves;
            }
        }

        Initialized = true;
    }

    private string GetDisplayInfoLine(DisplayedInfo info)
    {
        return info switch
        {
            DisplayedInfo.Ability => $"**Ability:** {Strings.Ability}",

            DisplayedInfo.Alpha when PKM is IAlpha alpha && alpha.IsAlpha => "**Alpha:** Yes",

            DisplayedInfo.AVs when PKM is IAwakened awakened => GetAwakenedValuesString(awakened),

            DisplayedInfo.Ball => $"**Ball:** {GameInfo.Strings.balllist[PKM.Ball]}",

            DisplayedInfo.EVs => GetEVString(),

            DisplayedInfo.Form when PKM.Form > 0 && Strings.HasForm => $"**Form:** {Strings.Form}",

            DisplayedInfo.Friendship => $"**Friendship:** {PKM.CurrentFriendship}",

            DisplayedInfo.Gigantamax when PKM is IGigantamax gmax && gmax.CanGigantamax => "**Gigantamax:** Yes",

            DisplayedInfo.GVs when PKM is IGanbaru ganbaru => GetGanbaruValuesString(ganbaru),

            DisplayedInfo.Height when PKM is IScaledSize scaled => $"**Height:** {scaled.HeightScalar}",

            DisplayedInfo.IVs => GetIVString(),

            DisplayedInfo.Language => $"**Language:** {(LanguageID)PKM.Language}",

            DisplayedInfo.Level => $"**Level:** {PKM.CurrentLevel}",

            DisplayedInfo.Mark when Strings.Mark.HasMark => $"**Mark:** {Strings.Mark.Name}{Environment.NewLine}",

            DisplayedInfo.Moves => string.Join(Environment.NewLine, Strings.Moves),

            DisplayedInfo.Nature => $"**Nature:** {Strings.Nature}",

            DisplayedInfo.Nickname when !string.IsNullOrEmpty(PKM.Nickname) && PKM.Nickname != GameInfo.Strings.Species[PKM.Species] => $"**Nickname:** {PKM.Nickname}",

            DisplayedInfo.Scale => $"**Scale:** {Strings.Scale}",

            DisplayedInfo.Shiny when PKM.IsShiny => $"**Shiny:** {(PKM.ShinyXor == 0 ? "Square" : "Star")}",
            DisplayedInfo.Shiny => "**Shiny:** No",

            DisplayedInfo.Species => $"**{Strings.Shiny}{Strings.Species}{Strings.Gender}**",

            DisplayedInfo.SpeciesForm when Strings.HasForm => $"**{Strings.Shiny}{Strings.Species}-{Strings.Form}{Strings.Gender}**",
            DisplayedInfo.SpeciesForm => $"**{Strings.Shiny}{Strings.Species}{Strings.Gender}**",

            DisplayedInfo.SpeciesHeldItem when Strings.HasItem => $"**{Strings.Shiny}{Strings.Species}{Strings.Gender} ➜ {Strings.HeldItem}**",
            DisplayedInfo.SpeciesHeldItem => $"**{Strings.Shiny}{Strings.Species}{Strings.Gender}**",

            DisplayedInfo.SpeciesFormHeldItem when Strings.HasForm && Strings.HasItem => $"**{Strings.Shiny}{Strings.Species}-{Strings.Form}{Strings.Gender} ➜ {Strings.HeldItem}**",
            DisplayedInfo.SpeciesFormHeldItem when Strings.HasForm => $"**{Strings.Shiny}{Strings.Species}-{Strings.Form}{Strings.Gender}**",
            DisplayedInfo.SpeciesFormHeldItem when Strings.HasItem => $"**{Strings.Shiny}{Strings.Species}{Strings.Gender} ➜ {Strings.HeldItem}**",
            DisplayedInfo.SpeciesFormHeldItem => $"**{Strings.Shiny}{Strings.Species}{Strings.Gender}**",

            DisplayedInfo.SpeciesMark when Strings.Mark.HasMark => $"**{Strings.Shiny}{Strings.Species}{Strings.Mark.Title}{Strings.Gender}**",
            DisplayedInfo.SpeciesMark => $"**{Strings.Shiny}{Strings.Species}{Strings.Gender}**",

            DisplayedInfo.SpeciesFormMark when Strings.HasForm && Strings.Mark.HasMark => $"**{Strings.Shiny}{Strings.Species}-{Strings.Form}{Strings.Mark.Title}{Strings.Gender}**",
            DisplayedInfo.SpeciesFormMark when Strings.HasForm => $"**{Strings.Shiny}{Strings.Species}-{Strings.Form}{Strings.Gender}**",
            DisplayedInfo.SpeciesFormMark when Strings.Mark.HasMark => $"**{Strings.Shiny}{Strings.Species}{Strings.Mark.Title}{Strings.Gender}**",
            DisplayedInfo.SpeciesFormMark => $"**{Strings.Shiny}{Strings.Species}{Strings.Gender}**",

            DisplayedInfo.SpeciesMarkHeldItem when Strings.Mark.HasMark && Strings.HasItem => $"**{Strings.Shiny}{Strings.Species}{Strings.Mark.Title}{Strings.Gender} ➜ {Strings.HeldItem}**",
            DisplayedInfo.SpeciesMarkHeldItem when Strings.Mark.HasMark => $"**{Strings.Shiny}{Strings.Species}{Strings.Mark.Title}{Strings.Gender}**",
            DisplayedInfo.SpeciesMarkHeldItem when Strings.HasItem => $"**{Strings.Shiny}{Strings.Species}{Strings.Gender} ➜ {Strings.HeldItem}**",
            DisplayedInfo.SpeciesMarkHeldItem => $"**{Strings.Shiny}{Strings.Species}{Strings.Gender}**",

            DisplayedInfo.SpeciesFormMarkHeldItem when Strings.HasForm && Strings.Mark.HasMark && Strings.HasItem => $"**{Strings.Shiny}{Strings.Species}-{Strings.Form}{Strings.Mark.Title}{Strings.Gender} ➜ {Strings.HeldItem}**",
            DisplayedInfo.SpeciesFormMarkHeldItem when Strings.Mark.HasMark && Strings.HasItem => $"**{Strings.Shiny}{Strings.Species}{Strings.Mark.Title}{Strings.Gender} ➜ {Strings.HeldItem}**",
            DisplayedInfo.SpeciesFormMarkHeldItem when Strings.HasForm && Strings.Mark.HasMark => $"**{Strings.Shiny}{Strings.Species}-{Strings.Form}{Strings.Mark.Title}{Strings.Gender}**",
            DisplayedInfo.SpeciesFormMarkHeldItem when Strings.HasForm && Strings.HasItem => $"**{Strings.Shiny}{Strings.Species}-{Strings.Form}{Strings.Gender} ➜ {Strings.HeldItem}**",
            DisplayedInfo.SpeciesFormMarkHeldItem when Strings.Mark.HasMark => $"**{Strings.Shiny}{Strings.Species}{Strings.Mark.Title}{Strings.Gender}**",
            DisplayedInfo.SpeciesFormMarkHeldItem when Strings.HasItem => $"**{Strings.Shiny}{Strings.Species}{Strings.Gender} ➜ {Strings.HeldItem}**",
            DisplayedInfo.SpeciesFormMarkHeldItem when Strings.HasForm => $"**{Strings.Shiny}{Strings.Species}-{Strings.Form}{Strings.Gender}**",
            DisplayedInfo.SpeciesFormMarkHeldItem => $"**{Strings.Shiny}{Strings.Species}{Strings.Gender}**",

            DisplayedInfo.StatNature when PKM.StatNature != PKM.Nature => $"**Stat Nature:** {PKM.StatNature}",

            DisplayedInfo.TeraType when Strings.HasTeraType => $"**Tera Type:** {Strings.TeraType}",

            DisplayedInfo.TeraTypeOverride when PKM is ITeraType tera && tera.TeraTypeOverride != tera.TeraType => $"**Tera Type Override:** {tera.TeraTypeOverride}",

            DisplayedInfo.Weight when PKM is IScaledSize scaled => $"**Weight:** {scaled.WeightScalar}",

            _ => ""
        };
    }

    private string GetIVString()
    {
        List<string> ivList =
        [
            PKM.IV_HP  < 31 ? $"{PKM.IV_HP} HP" : "",
            PKM.IV_ATK < 31 ? $"{PKM.IV_ATK} Atk" : "",
            PKM.IV_DEF < 31 ? $"{PKM.IV_DEF} Def" : "",
            PKM.IV_SPA < 31 ? $"{PKM.IV_SPA} SpA" : "",
            PKM.IV_SPD < 31 ? $"{PKM.IV_SPD} SpD" : "",
            PKM.IV_SPE < 31 ? $"{PKM.IV_SPE} Spe" : "",
        ];
        ivList = [.. ivList.Where(s => !string.IsNullOrEmpty(s))];
        return "**IVs:** " + (PKM.IVTotal == 186 ? "6IV" : string.Join(" / ", ivList));
    }

    private string GetEVString()
    {
        List<string> evList =
        [
            PKM.EV_HP  > 0 ? $"{PKM.EV_HP} HP" : "",
            PKM.EV_ATK > 0 ? $"{PKM.EV_ATK} Atk" : "",
            PKM.EV_DEF > 0 ? $"{PKM.EV_DEF} Def" : "",
            PKM.EV_SPA > 0 ? $"{PKM.EV_SPA} SpA" : "",
            PKM.EV_SPD > 0 ? $"{PKM.EV_SPD} SpD" : "",
            PKM.EV_SPE > 0 ? $"{PKM.EV_SPE} Spe" : "",
        ];
        evList = [.. evList.Where(s => !string.IsNullOrEmpty(s))];
        return evList.Count == 0 ? "" : "**EVs:** " + string.Join(" / ", evList);
    }

    private string GetAwakenedValuesString(IAwakened awakened)
    {
        List<string> avList =
        [
            awakened.AV_HP  > 0 ? $"{awakened.AV_HP} HP" : "",
            awakened.AV_ATK > 0 ? $"{awakened.AV_ATK} Atk" : "",
            awakened.AV_DEF > 0 ? $"{awakened.AV_DEF} Def" : "",
            awakened.AV_SPA > 0 ? $"{awakened.AV_SPA} SpA" : "",
            awakened.AV_SPD > 0 ? $"{awakened.AV_SPD} SpD" : "",
            awakened.AV_SPE > 0 ? $"{awakened.AV_SPE} Spe" : "",
        ];
        avList = [.. avList.Where(s => !string.IsNullOrEmpty(s))];
        return avList.Count == 0 ? "" : "**AVs:** " + string.Join(" / ", avList);
    }

    private string GetGanbaruValuesString(IGanbaru ganbaru)
    {
        List<string> gvList =
        [
            ganbaru.GV_HP  > 0 ? $"{ganbaru.GV_HP} HP" : "",
            ganbaru.GV_ATK > 0 ? $"{ganbaru.GV_ATK} Atk" : "",
            ganbaru.GV_DEF > 0 ? $"{ganbaru.GV_DEF} Def" : "",
            ganbaru.GV_SPA > 0 ? $"{ganbaru.GV_SPA} SpA" : "",
            ganbaru.GV_SPD > 0 ? $"{ganbaru.GV_SPD} SpD" : "",
            ganbaru.GV_SPE > 0 ? $"{ganbaru.GV_SPE} Spe" : "",
        ];
        gvList = [.. gvList.Where(s => !string.IsNullOrEmpty(s))];
        return gvList.Count == 0 ? "" : "**GVs:** " + string.Join(" / ", gvList);
    }

    private Color InitializeColor() =>
        EmbedColorHelper.GetDiscordColor(PKM.IsShiny ? EmbedColorHelper.ShinyMap[((Species)PKM.Species, PKM.Form)] : (PersonalColor)PKM.PersonalInfo.Color);

    private EmbedAuthorBuilder InitializeAuthor() => new()
    {
        Name = $"{trader.Username}'s {(MysteryEgg ? "Mystery Egg" : PKM.IsShiny ? "Shiny " : $"Pokémon {(PKM.IsEgg ? "Egg" : "")}")}",
        IconUrl = Strings.GetBallImageURL(),
    };

    private EmbedFooterBuilder InitializeFooter()
    {
        var type = Hub.Config.Discord.UseTradeEmbeds;
        string footerText = string.Empty;

        // Assume OT and TID can change during the trade process, only show them if the trade has been completed.
        if (type is TradeEmbedDisplay.TradeInitialize)
        {
            var position = Hub.Queues.Info.CheckPosition(trader.UID, PokeRoutineType.LinkTrade);
            var botCount = Hub.Queues.Info.Hub.Bots.Count;
            footerText += $"Current Position: {position.Position}";

            if (position.Position > botCount)
            {
                var eta = Hub.Config.Queues.EstimateDelay(position.Position, botCount);
                footerText += $"{Environment.NewLine}Estimated wait time: {eta:F1} minutes.";
            }
        }
        else if (type is TradeEmbedDisplay.TradeComplete)
        {
            footerText += $"OT: {PKM.OriginalTrainerName} | TID: {PKM.DisplayTID}" +
                          $"{Environment.NewLine}Trade finished. Enjoy your Pokémon!";
        }
        var imgURL = Strings.GetMarkImageURL();
        return new EmbedFooterBuilder { Text = footerText, IconUrl = imgURL };
    }
}

public record QueueUser(ulong UID, string Username);
