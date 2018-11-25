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
            { "Width", (parser, x) => x.Width = parser.ParseInteger() },
            { "NumSegments", (parser, x) => x.NumSegments = parser.ParseInteger() },
            { "Color", (parser, x) => x.Color = parser.ParseColorRgba() },
            { "Texture", (parser, x) => x.Texture = parser.ParseAssetReference() }
        };

        public int Length { get; internal set; }
        public int Width { get; internal set; }
        public int NumSegments { get; internal set; }
        public ColorRgba Color { get; internal set; }
        public string Texture { get; internal set; }
    }
}
