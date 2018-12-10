using System.Collections.Generic;
using System.Numerics;
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
            { "SnowBoxHeight", (parser, x) => x.SnowBoxHeight = parser.ParseInteger() },
            { "SnowSpacing", (parser, x) => x.SnowSpacing = parser.ParseInteger() },
            { "NumberTiles", (parser, x) => x.NumberTiles = parser.ParseInteger() },
            { "SnowSpeed", (parser, x) => x.SnowSpeed = parser.ParseFloat() },

            { "LightningEnabled", (parser, x) => x.LightningEnabled = parser.ParseBoolean() },
            { "LightningFactor", (parser, x) => x.LightningFactor = parser.ParseFloatArray() },
            { "LightningDuration", (parser, x) => x.LightningDuration = parser.ParseInteger() },
            { "LightningChance", (parser, x) => x.LightningChance = parser.ParseFloat() },

            { "SpellEnabled", (parser, x) => x.SpellEnabled = parser.ParseBoolean() },
            { "SpellDuration", (parser, x) => x.SpellDuration = parser.ParseInteger() },
            { "RampControl", (parser, x) => x.RampControl = parser.ParseVector2() },
            { "RampSpeed", (parser, x) => x.RampSpeed = parser.ParseVector2() },
            { "RampSpacing", (parser, x) => x.RampSpacing = parser.ParseVector2() },
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

        [AddedIn(SageGame.Bfme)]
        public int SnowBoxHeight { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int SnowSpacing { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int NumberTiles { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float SnowSpeed { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool LightningEnabled { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float[] LightningFactor { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int LightningDuration { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public float LightningChance { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool SpellEnabled { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int SpellDuration { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Vector2 RampControl { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Vector2 RampSpeed { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public Vector2 RampSpacing { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public List<WeatherData> WeatherDatas { get; } = new List<WeatherData>();
    }

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

    [AddedIn(SageGame.Bfme)]
    public enum WeatherType
    {
        [IniEnum("NONE")]
        None,

        [IniEnum("RAINY")]
        Rainy,

        [IniEnum("CLOUDYRAINY")]
        CloudyRainy,

        [IniEnum("SUNNY")]
        Sunny,

        [IniEnum("CLOUDY")]
        Cloudy,
    }
}
