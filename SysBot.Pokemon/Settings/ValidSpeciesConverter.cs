using System.ComponentModel;
using PKHeX.Core;

namespace SysBot.Pokemon
{
    public class ValidSpeciesConverter : TypeConverter
    {
        public static GameVersion Version { get; set; }

        public override bool GetStandardValuesSupported(ITypeDescriptorContext? context)
        {
            return Version is not GameVersion.GG and not GameVersion.PLA;
        }

        public override bool GetStandardValuesExclusive(ITypeDescriptorContext? context)
        {
            return true;
        }

        public override StandardValuesCollection GetStandardValues(ITypeDescriptorContext? context)
        {
            var personal = GameData.GetPersonal(Version);
            var validSpecies = Enum.GetValues<Species>()
                .Where(species => personal.IsSpeciesInGame((ushort)species))
                .OrderBy(s => s.ToString())
                .ToList();

            return new StandardValuesCollection(validSpecies);
        }

        public override bool CanConvertFrom(ITypeDescriptorContext? context, Type sourceType)
        {
            return sourceType == typeof(string) || base.CanConvertFrom(context, sourceType);
        }

        public override object? ConvertFrom(ITypeDescriptorContext? context, System.Globalization.CultureInfo? culture, object value)
        {
            if (value is string str)
            {
                return Enum.Parse<Species>(str);
            }
            return base.ConvertFrom(context, culture, value);
        }
    }
}
