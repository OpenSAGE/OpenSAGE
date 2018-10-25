using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class RoadTemplate
    {
        internal static RoadTemplate Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<RoadTemplate> FieldParseTable = new IniParseTable<RoadTemplate>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseFileName() },
            { "RoadWidth", (parser, x) => x.RoadWidth = parser.ParseFloat() },
            { "RoadWidthInTexture", (parser, x) => x.RoadWidthInTexture = parser.ParseFloat() }
        };

        public string Name { get; private set; }

        public string Texture { get; private set; }
        public float RoadWidth { get; private set; }
        public float RoadWidthInTexture { get; private set; }
    }
}
