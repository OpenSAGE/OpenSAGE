using OpenSage.Data.Ini.Parser;
using OpenSage.Data.Map;
using OpenSage.Mathematics;

namespace OpenSage.Data.Ini
{
    public sealed class WaterSet
    {
        internal static WaterSet Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.TimeOfDay = IniParser.ParseEnum<TimeOfDay>(new IniToken(name, default(IniTokenPosition))),
                FieldParseTable);
        }

        private static readonly IniParseTable<WaterSet> FieldParseTable = new IniParseTable<WaterSet>
        {
            { "SkyTexture", (parser, x) => x.SkyTexture = parser.ParseFileName() },
            { "WaterTexture", (parser, x) => x.WaterTexture = parser.ParseFileName() },
            { "Vertex00Color", (parser, x) => x.Vertex00Color = parser.ParseColorRgba() },
            { "Vertex10Color", (parser, x) => x.Vertex10Color = parser.ParseColorRgba() },
            { "Vertex01Color", (parser, x) => x.Vertex01Color = parser.ParseColorRgba() },
            { "Vertex11Color", (parser, x) => x.Vertex11Color = parser.ParseColorRgba() },
            { "DiffuseColor", (parser, x) => x.DiffuseColor = parser.ParseColorRgba() },
            { "TransparentDiffuseColor", (parser, x) => x.TransparentDiffuseColor = parser.ParseColorRgba() },
            { "UScrollPerMS", (parser, x) => x.UScrollPerMS = parser.ParseFloat() },
            { "VScrollPerMS", (parser, x) => x.VScrollPerMS = parser.ParseFloat() },
            { "SkyTexelsPerUnit", (parser, x) => x.SkyTexelsPerUnit = parser.ParseFloat() },
            { "WaterRepeatCount", (parser, x) => x.WaterRepeatCount = parser.ParseInteger() },
        };

        public TimeOfDay TimeOfDay { get; private set; }

        public string SkyTexture { get; private set; }
        public string WaterTexture { get; private set; }
        public ColorRgba Vertex00Color { get; private set; }
        public ColorRgba Vertex10Color { get; private set; }
        public ColorRgba Vertex01Color { get; private set; }
        public ColorRgba Vertex11Color { get; private set; }
        public ColorRgba DiffuseColor { get; private set; }
        public ColorRgba TransparentDiffuseColor { get; private set; }
        public float UScrollPerMS { get; private set; }
        public float VScrollPerMS { get; private set; }
        public float SkyTexelsPerUnit { get; private set; }
        public int WaterRepeatCount { get; private set; }
    }
}
