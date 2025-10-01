using PKHeX.Core;

namespace SysBot.Pokemon;

public record PokemonMark
{
    public bool HasMark => Index != RibbonIndex.MAX_COUNT;

    public RibbonIndex Index { get; init; } = RibbonIndex.MAX_COUNT;
    public string Name { get; init; } = "";
    public string Title { get; init; } = "";

    public PokemonMark(PKM pkm)
    {
        const RibbonIndex Gen8StartMark = RibbonIndex.MarkLunchtime;
        const RibbonIndex Gen8EndMark = RibbonIndex.MarkSlump;
        const RibbonIndex Gen9StartMark = RibbonIndex.MarkJumbo;
        const RibbonIndex Gen9EndMark = RibbonIndex.MarkTitan;

        if (pkm is not IRibbonIndex r)
            return;

        foreach (var mark in Enum.GetValues<RibbonIndex>())
        {
            if ((mark >= Gen8StartMark && mark <= Gen8EndMark) || (mark >= Gen9StartMark && mark <= Gen9EndMark))
            {
                if (r.GetRibbon((int)mark))
                {
                    Index = mark;
                    Name = $"{Index}".Replace("Mark", "");
                    Title = MarkTitle[(Index - Gen8StartMark)];
                    break;
                }
            }
        }
    }

    public static readonly string[] MarkTitle =
    [
        " the Peckish"," the Sleepy"," the Dozy"," the Early Riser"," the Cloud Watcher"," the Sodden"," the Thunderstruck"," the Snow Frolicker"," the Shivering"," the Parched"," the Sandswept"," the Mist Drifter",
        " the Chosen One"," the Catch of the Day"," the Curry Connoisseur"," the Sociable"," the Recluse"," the Rowdy"," the Spacey"," the Anxious"," the Giddy"," the Radiant"," the Serene"," the Feisty"," the Daydreamer",
        " the Joyful"," the Furious"," the Beaming"," the Teary-Eyed"," the Chipper"," the Grumpy"," the Scholar"," the Rampaging"," the Opportunist"," the Stern"," the Kindhearted"," the Easily Flustered"," the Driven",
        " the Apathetic"," the Arrogant"," the Reluctant"," the Humble"," the Pompous"," the Lively"," the Worn-Out", " of the Distant Past", " the Twinkling Star", " the Paldea Champion", " the Great", " the Teeny", " the Treasure Hunter",
        " the Reliable Partner", " the Gourmet", " the One-in-a-Million", " the Former Alpha", " the Unrivaled", " the Former Titan",
    ];
}
