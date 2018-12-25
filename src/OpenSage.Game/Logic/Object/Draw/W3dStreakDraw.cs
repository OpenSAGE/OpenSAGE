using OpenSage.Data.Ini.Parser;
using OpenSage.Mathematics;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class W3dStreakDrawModuleData : DrawModuleData
    {
        internal static W3dStreakDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<W3dStreakDrawModuleData> FieldParseTable = new IniParseTable<W3dStreakDrawModuleData>
        {
            { "Length", (parser, x) => x.Length =  parser.ParseInteger() },
            { "Width", (parser, x) => x.Width = parser.ParseFloat() },
            { "NumSegments", (parser, x) => x.NumSegments = parser.ParseInteger() },
            { "Color", (parser, x) => x.Color = parser.ParseColorRgba() },
            { "Texture", (parser, x) => x.Texture = parser.ParseAssetReference() },
            { "Additive", (parser, x) => x.Additive = parser.ParseBoolean() },
            { "WeatherTexture", (parser, x) => x.WeatherTexture = WeatherTexture.Parse(parser) }
        };

        public int Length { get; private set; }
        public float Width { get; private set; }
        public int NumSegments { get; private set; }
        public ColorRgba Color { get; private set; }
        public string Texture { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool Additive { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public WeatherTexture WeatherTexture { get; private set; }
    }
}
