using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class DarknessSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new DarknessSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DarknessSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<DarknessSpecialPowerModuleData>
            {
                { "AffectEvil", (parser, x) => x.AffectEvil = parser.ParseBoolean() },
                { "WeatherDuration", (parser, x) => x.WeatherDuration = parser.ParseLong() },
                { "ChangeWeather", (parser, x) => x.ChangeWeather = parser.ParseEnum<WeatherType>() }
            });

        public bool AffectEvil { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public long WeatherDuration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public WeatherType ChangeWeather { get; private set; }
    }
}
