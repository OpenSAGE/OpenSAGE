using OpenSage.Data.Ini.Parser;
using OpenSage.Data.Map;
using OpenSage.Data.Wnd;

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
            { "Vertex00Color", (parser, x) => x.Vertex00Color = WndColor.Parse(parser) },
            { "Vertex10Color", (parser, x) => x.Vertex10Color = WndColor.Parse(parser) },
            { "Vertex01Color", (parser, x) => x.Vertex01Color = WndColor.Parse(parser) },
            { "Vertex11Color", (parser, x) => x.Vertex11Color = WndColor.Parse(parser) },
            { "DiffuseColor", (parser, x) => x.DiffuseColor = WndColor.Parse(parser) },
            { "TransparentDiffuseColor", (parser, x) => x.TransparentDiffuseColor = WndColor.Parse(parser) },
            { "UScrollPerMS", (parser, x) => x.UScrollPerMS = parser.ParseFloat() },
            { "VScrollPerMS", (parser, x) => x.VScrollPerMS = parser.ParseFloat() },
            { "SkyTexelsPerUnit", (parser, x) => x.SkyTexelsPerUnit = parser.ParseFloat() },
            { "WaterRepeatCount", (parser, x) => x.WaterRepeatCount = parser.ParseInteger() },
        };

        public TimeOfDay TimeOfDay { get; private set; }

        public string SkyTexture { get; private set; }
        public string WaterTexture { get; private set; }
        public WndColor Vertex00Color { get; private set; }
        public WndColor Vertex10Color { get; private set; }
        public WndColor Vertex01Color { get; private set; }
        public WndColor Vertex11Color { get; private set; }
        public WndColor DiffuseColor { get; private set; }
        public WndColor TransparentDiffuseColor { get; private set; }
        public float UScrollPerMS { get; private set; }
        public float VScrollPerMS { get; private set; }
        public float SkyTexelsPerUnit { get; private set; }
        public int WaterRepeatCount { get; private set; }
    }
}
