using System.Numerics;
using OpenSage.Data.Ini;
using OpenSage.Mathematics;

namespace OpenSage.FX
{
    [AddedIn(SageGame.Bfme)]
    public sealed class DynamicDecalFXNuggetData : FXNuggetData
    {
        internal static DynamicDecalFXNuggetData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<DynamicDecalFXNuggetData> FieldParseTable = FXNuggetFieldParseTable.Concat(new IniParseTable<DynamicDecalFXNuggetData>
        {
            { "DecalName", (parser, x) => x.DecalName = parser.ParseAssetReference() },
            { "Size", (parser, x) => x.Size = parser.ParseInteger() },
            { "Color", (parser, x) => x.Color = parser.ParseColorRgb() },
            { "Offset", (parser, x) => x.Offset = parser.ParseVector2() },

            { "OpacityStart", (parser, x) => x.OpacityStart = parser.ParseInteger() },
            { "OpacityFadeTimeOne", (parser, x) => x.OpacityFadeTimeOne = parser.ParseInteger() },
            { "OpacityPeak", (parser, x) => x.OpacityPeak = parser.ParseInteger() },
            { "OpacityPeakTime", (parser, x) => x.OpacityPeakTime = parser.ParseInteger() },
            { "OpacityFadeTimeTwo", (parser, x) => x.OpacityFadeTimeTwo = parser.ParseInteger() },
            { "OpacityEnd", (parser, x) => x.OpacityEnd = parser.ParseInteger() },

            { "StartingDelay", (parser, x) => x.StartingDelay = parser.ParseInteger() },
            { "Lifetime", (parser, x) => x.Lifetime = parser.ParseInteger() },
            { "Shader", (parser, x) => x.Shader = parser.ParseEnum<DynamicDecalShaderType>() }
        });

        public string DecalName { get; private set; }
        public int Size { get; private set; }
        public ColorRgb Color { get; private set; }
        public Vector2 Offset { get; private set; }

        public int OpacityStart { get; private set; }
        public int OpacityFadeTimeOne { get; private set; }
        public int OpacityPeak { get; private set; }
        public int OpacityPeakTime { get; private set; }
        public int OpacityFadeTimeTwo { get; private set; }
        public int OpacityEnd { get; private set; }

        public int StartingDelay { get; private set; }
        public int Lifetime { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public DynamicDecalShaderType Shader { get; private set; }
    }
}
