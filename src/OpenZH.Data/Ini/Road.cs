using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class Road
    {
        internal static Road Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Road> FieldParseTable = new IniParseTable<Road>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseAsciiString() },
            { "RoadWidth", (parser, x) => x.RoadWidth = parser.ParseFloat() },
            { "RoadWidthInTexture", (parser, x) => x.RoadWidthInTexture = parser.ParseFloat() }
        };

        public string Name { get; private set; }

        public string Texture { get; private set; }
        public float RoadWidth { get; private set; }
        public float RoadWidthInTexture { get; private set; }
    }
}
