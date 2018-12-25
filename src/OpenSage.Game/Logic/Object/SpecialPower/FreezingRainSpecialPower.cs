using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class FreezingRainSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new FreezingRainSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<FreezingRainSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<FreezingRainSpecialPowerModuleData>
            {
                { "WeatherDuration", (parser, x) => x.WeatherDuration = parser.ParseInteger() },
                { "ChangeWeather", (parser, x) => x.ChangeWeather = parser.ParseEnum<WeatherType>() },
                { "BurnRateModifier", (parser, x) => x.BurnRateModifier = parser.ParseInteger() },
                { "BurnDecayModifier", (parser, x) => x.BurnDecayModifier = parser.ParseInteger() }
            });

        [AddedIn(SageGame.Bfme2)]
        public int WeatherDuration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public WeatherType ChangeWeather { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int BurnRateModifier { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int BurnDecayModifier { get; private set; }
    }
}
