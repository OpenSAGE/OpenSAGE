using OpenSage.Data.Ini.Parser;
using OpenSage.Data.Wnd;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Interdependent with the <see cref="LaserUpdateModuleData"/> module and requires the object to have 
    /// KindOf = INERT IMMOBILE.
    /// </summary>
    public sealed class W3dLaserDrawModuleData : DrawModuleData
    {
        internal static W3dLaserDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<W3dLaserDrawModuleData> FieldParseTable = new IniParseTable<W3dLaserDrawModuleData>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseFileName() },
            { "NumBeams", (parser, x) => x.NumBeams = parser.ParseInteger() },
            { "InnerBeamWidth", (parser, x) => x.InnerBeamWidth = parser.ParseFloat() },
            { "InnerColor", (parser, x) => x.InnerColor = WndColor.Parse(parser) },
            { "OuterBeamWidth", (parser, x) => x.OuterBeamWidth = parser.ParseFloat() },
            { "OuterColor", (parser, x) => x.OuterColor = WndColor.Parse(parser) },
            { "Tile", (parser, x) => x.Tile = parser.ParseBoolean() },
            { "ScrollRate", (parser, x) => x.ScrollRate = parser.ParseFloat() },
            { "Segments", (parser, x) => x.Segments = parser.ParseInteger() },
            { "ArcHeight", (parser, x) => x.ArcHeight = parser.ParseFloat() },
            { "SegmentOverlapRatio", (parser, x) => x.SegmentOverlapRatio = parser.ParseFloat() },
            { "TilingScalar", (parser, x) => x.TilingScalar = parser.ParseFloat() },
        };

        public string Texture { get; private set; }
        public int NumBeams { get; private set; }
        public float InnerBeamWidth { get; private set; }
        public WndColor InnerColor { get; private set; }
        public float OuterBeamWidth { get; private set; }
        public WndColor OuterColor { get; private set; }
        public bool Tile { get; private set; }
        public float ScrollRate { get; private set; }
        public int Segments { get; private set; }
        public float ArcHeight { get; private set; }
        public float SegmentOverlapRatio { get; private set; }
        public float TilingScalar { get; private set; }
    }
}
