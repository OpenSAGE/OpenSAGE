using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
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
            { "Texture", (parser, x) => x.Texture = parser.ParseFileName() },
            { "BlendEdges", (parser, x) => x.BlendEdges = parser.ParseBoolean() },
            { "Class", (parser, x) => x.Class = parser.ParseString() },
            { "RestrictConstruction", (parser, x) => x.RestrictConstruction = parser.ParseBoolean() },
        };

        public string Name { get; private set; }

        public string Texture { get; private set; }
        public bool BlendEdges { get; private set; }
        public string Class { get; private set; }
        public bool RestrictConstruction { get; private set; }
    }
}
