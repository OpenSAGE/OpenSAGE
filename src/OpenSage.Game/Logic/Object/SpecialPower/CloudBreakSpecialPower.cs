using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class CloudBreakSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new CloudBreakSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CloudBreakSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<CloudBreakSpecialPowerModuleData>
            {
                { "SunbeamObject", (parser, x) => x.SunbeamObject = parser.ParseAssetReference() },
                { "ObjectSpacing", (parser, x) => x.ObjectSpacing = parser.ParseInteger() },
                { "AntiFX", (parser, x) => x.AntiFX = parser.ParseAssetReference() },
                { "ReEnableAntiCategory", (parser, x) => x.ReEnableAntiCategory = parser.ParseBoolean() },
                { "WeatherDuration", (parser, x) => x.WeatherDuration = parser.ParseInteger() },
                { "ChangeWeather", (parser, x) => x.ChangeWeather = parser.ParseEnum<WeatherType>() }
            });

        public string SunbeamObject { get; private set; }
        public int ObjectSpacing { get; private set; }
        public string AntiFX { get; private set; }
        public bool ReEnableAntiCategory { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public int WeatherDuration { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public WeatherType ChangeWeather { get; private set; }
    }
}
