using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class Weather
    {
        internal static Weather Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<Weather> FieldParseTable = new IniParseTable<Weather>
        {
            { "SnowEnabled", (parser, x) => x.SnowEnabled = parser.ParseBoolean() },
            { "IsSnowing", (parser, x) => x.IsSnowing = parser.ParseBoolean() },
            { "SnowTexture", (parser, x) => x.SnowTexture = parser.ParseFileName() },
            { "SnowBoxDimensions", (parser, x) => x.SnowBoxDimensions = parser.ParseInteger() },
            { "SnowBoxDensity", (parser, x) => x.SnowBoxDensity = parser.ParseFloat() },
            { "SnowFrequencyScaleX", (parser, x) => x.SnowFrequencyScaleX = parser.ParseFloat() },
            { "SnowFrequencyScaleY", (parser, x) => x.SnowFrequencyScaleY = parser.ParseFloat() },
            { "SnowAmplitude", (parser, x) => x.SnowAmplitude = parser.ParseFloat() },
            { "SnowVelocity", (parser, x) => x.SnowVelocity = parser.ParseFloat() },
            { "SnowPointSize", (parser, x) => x.SnowPointSize = parser.ParseFloat() },
            { "SnowMaxPointSize", (parser, x) => x.SnowMaxPointSize = parser.ParseFloat() },
            { "SnowMinPointSize", (parser, x) => x.SnowMinPointSize = parser.ParseFloat() },

            { "SnowPointSprites", (parser, x) => x.SnowPointSprites = parser.ParseBoolean() },
            { "SnowQuadSize", (parser, x) => x.SnowQuadSize = parser.ParseFloat() },
        };

        public bool SnowEnabled { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool IsSnowing { get; private set; }

        public string SnowTexture { get; private set; }
        public int SnowBoxDimensions { get; private set; }
        public float SnowBoxDensity { get; private set; }
        public float SnowFrequencyScaleX { get; private set; }
        public float SnowFrequencyScaleY { get; private set; }
        public float SnowAmplitude { get; private set; }
        public float SnowVelocity { get; private set; }
        public float SnowPointSize { get; private set; }
        public float SnowMaxPointSize { get; private set; }
        public float SnowMinPointSize { get; private set; }

        public bool SnowPointSprites { get; private set; }
        public float SnowQuadSize { get; private set; }
    }
}
