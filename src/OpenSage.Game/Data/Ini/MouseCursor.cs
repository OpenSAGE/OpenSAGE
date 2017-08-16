using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class MouseCursor
    {
        internal static MouseCursor Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<MouseCursor> FieldParseTable = new IniParseTable<MouseCursor>
        {
            { "Texture", (parser, x) => x.Texture = parser.ParseFileName() },
            { "Image", (parser, x) => x.Image = parser.ParseFileName() },
            { "Directions", (parser, x) => x.Directions = parser.ParseInteger() }
        };

        public string Name { get; private set; }

        public string Texture { get; private set; }
        public string Image { get; private set; }
        public int Directions { get; private set; }
    }
}
