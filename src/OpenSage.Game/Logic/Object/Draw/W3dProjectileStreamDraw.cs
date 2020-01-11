using OpenSage.Data.Ini;
using OpenSage.Data.Wnd;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires object to have KindOf = INERT.
    /// </summary>
    public sealed class W3dProjectileStreamDrawModuleData : DrawModuleData
    {
        internal static W3dProjectileStreamDrawModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<W3dProjectileStreamDrawModuleData> FieldParseTable = new IniParseTable<W3dProjectileStreamDrawModuleData>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseFileName() },
            { "Width", (parser, x) => x.Width = parser.ParseFloat() },
            { "TileFactor", (parser, x) => x.TileFactor = parser.ParseFloat() },
            { "ScrollRate", (parser, x) => x.ScrollRate = parser.ParseFloat() },
            { "MaxSegments", (parser, x) => x.MaxSegments = parser.ParseInteger() },
        };

        public string Texture { get; private set; }
        public float Width { get; private set; }
        public float TileFactor { get; private set; }
        public float ScrollRate { get; private set; }
        public int MaxSegments { get; private set; }
    }
}
