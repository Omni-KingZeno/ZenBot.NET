using Discord;
using PKHeX.Core;
using SysBot.Pokemon.Discord.Helpers;

namespace SysBot.Pokemon.Discord;

public class TradeEmbedBuilder<T>(T PKM, PokeTradeHub<T> Hub, QueueUser trader, bool mysteryEgg) where T : PKM, new()
{
    private bool Initialized { get; set; } = false;
    public EmbedBuilder Builder { get; init; } = new();
    private PKMStringWrapper<T> Strings { get; init; } = new(PKM, Hub.Config.Discord.TradeEmbedSettings, mysteryEgg);

    public Embed Build()
    {
        if (!Initialized)
            InitializeEmbed();

        return Builder.Build();
    }

    public void InitializeEmbed()
    {
        //Embed layout Style
        var altStyle = Hub.Config.Discord.TradeEmbedSettings.UseAlternateLayout;

        Builder.Color = InitializeColor();
        Builder.Author = InitializeAuthor();
        Builder.Footer = InitializeFooter();
        Builder.ThumbnailUrl = altStyle ? "" : Strings.GetPokemonImageURL(PKM.IsEgg, mysteryEgg);
        Builder.ImageUrl = altStyle ? Strings.GetPokemonImageURL(PKM.IsEgg, mysteryEgg) : "";

        // Set the Pokémon Species as Embed Title
        var mark = Strings.Mark;
        var fieldName = mark.HasMark switch
        {
            true => $"{Strings.Shiny}{Strings.Species}{mark.Title}{Strings.Gender}",
            _ => $"{Strings.Shiny}{Strings.Species}{Strings.Gender}",
        };

        // Add general Pokémon informations
        var fieldValue = $"**Ability:** {Strings.Ability}{Environment.NewLine}" +
                         (mark.HasMark ? $"**Mark:** {mark.Name}{Environment.NewLine}" : "") +
                         $"**Level:** {PKM.CurrentLevel}{Environment.NewLine}" +
                         (Strings.HasTeraType ? $"**Tera Type:** {Strings.TeraType}{Environment.NewLine}" : "") +
                         $"**Nature:** {Strings.Nature}{Environment.NewLine}" +
                         $"**Scale:** {Strings.Scale}{Environment.NewLine}";

        //Add Pokémon IVs, if enabled
        if (Hub.Config.Discord.TradeEmbedSettings.ShowIVs)
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
            var ivs = "**IVs: **" + (PKM.IVTotal == 186 ? "6IV" : string.Join(" / ", ivList));

            fieldValue += ivs + Environment.NewLine;
        }

        //Add Pokémon EVs, if enabled
        if (Hub.Config.Discord.TradeEmbedSettings.ShowIVs)
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
            var evs = evList.Count == 0 ? "" : "**EVs: **" + string.Join(" / ", evList) + Environment.NewLine;

            fieldValue += evs;
        }

        var moves = string.Join(Environment.NewLine, Strings.Moves);
        Builder.AddField(x =>
        {
            x.Name = fieldName;
            x.Value = fieldValue + (altStyle ? "" : moves);
            x.IsInline = true;
        });

        if (altStyle)
        {
            Builder.AddField(x =>
            {
                x.Name = "Moves:";
                x.Value = moves;
                x.IsInline = true;
            });
        }

        Initialized = true;
    }

    private Color InitializeColor() =>
        EmbedColorHelper.GetDiscordColor(PKM.IsShiny ? EmbedColorHelper.ShinyMap[((Species)PKM.Species, PKM.Form)] : (PersonalColor)PKM.PersonalInfo.Color);

    private EmbedAuthorBuilder InitializeAuthor() => new()
    {
        Name = $"{trader.Username}'s {(mysteryEgg ? "Mystery Egg" : PKM.IsShiny ? "Shiny " : $"Pokémon {(PKM.IsEgg ? "Egg" : "")}")}",
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

        return new EmbedFooterBuilder { Text = footerText, IconUrl = Strings.Mark.HasMark ? Strings.GetMarkImageURL() : string.Empty };
    }
}

public record QueueUser(ulong UID, string Username);
