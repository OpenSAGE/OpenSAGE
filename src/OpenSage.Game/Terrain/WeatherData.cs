using OpenSage.Data.Ini;

namespace OpenSage.Terrain
{
    [AddedIn(SageGame.Bfme)]
    public sealed class WeatherData
    {
        internal static WeatherData Parse(IniParser parser)
        {
            var type = parser.ParseEnum<WeatherType>();
            var result = parser.ParseTopLevelBlock(FieldParseTable);
            result.WeatherType = type;
            return result; 
        }

        private static readonly IniParseTable<WeatherData> FieldParseTable = new IniParseTable<WeatherData>
        {
            { "WeatherSound", (parser, x) => x.WeatherSound = parser.ParseAssetReference() },
            { "HasLightning", (parser, x) => x.HasLightning = parser.ParseBoolean() }
        };

        public WeatherType WeatherType { get; private set; }

        public string WeatherSound { get; private set; }
        public bool HasLightning { get; private set; }
    }
}
