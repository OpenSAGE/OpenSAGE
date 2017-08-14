using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class Terrain
    {
        internal static Terrain Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Terrain> FieldParseTable = new IniParseTable<Terrain>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseAsciiString() },
            { "Class", (parser, x) => x.Class = parser.ParseAsciiString() },
        };

        public string Name { get; private set; }

        public string Texture { get; private set; }
        public string Class { get; private set; }
    }
}
