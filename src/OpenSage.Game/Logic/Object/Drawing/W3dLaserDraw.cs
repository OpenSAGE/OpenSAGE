using OpenSage.Data.Ini.Parser;
using OpenSage.Data.Wnd;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Interdependent with the <see cref="LaserUpdate"/> module and requires the object to have 
    /// KindOf = INERT IMMOBILE.
    /// </summary>
    public sealed class W3dLaserDraw : ObjectDrawModule
    {
        internal static W3dLaserDraw Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<W3dLaserDraw> FieldParseTable = new IniParseTable<W3dLaserDraw>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseFileName() },
            { "NumBeams", (parser, x) => x.NumBeams = parser.ParseInteger() },
            { "InnerBeamWidth", (parser, x) => x.InnerBeamWidth = parser.ParseInteger() },
            { "InnerColor", (parser, x) => x.InnerColor = WndColor.Parse(parser) },
            { "Tile", (parser, x) => x.Tile = parser.ParseBoolean() },
            { "ScrollRate", (parser, x) => x.ScrollRate = parser.ParseFloat() },
            { "Segments", (parser, x) => x.Segments = parser.ParseInteger() },
            { "ArcHeight", (parser, x) => x.ArcHeight = parser.ParseFloat() },
            { "SegmentOverlapRatio", (parser, x) => x.SegmentOverlapRatio = parser.ParseFloat() },
            { "TilingScalar", (parser, x) => x.TilingScalar = parser.ParseFloat() },
        };

        public string Texture { get; private set; }
        public int NumBeams { get; private set; }
        public int InnerBeamWidth { get; private set; }
        public WndColor InnerColor { get; private set; }
        public bool Tile { get; private set; }
        public float ScrollRate { get; private set; }
        public int Segments { get; private set; }
        public float ArcHeight { get; private set; }
        public float SegmentOverlapRatio { get; private set; }
        public float TilingScalar { get; private set; }
    }
}
