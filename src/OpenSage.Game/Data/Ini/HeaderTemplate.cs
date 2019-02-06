using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class HeaderTemplate
    {
        internal static HeaderTemplate Parse(IniParser parser)
        {
           return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<HeaderTemplate> FieldParseTable = new IniParseTable<HeaderTemplate>
        {
            { "Font", (parser, x) => x.Font = parser.ParseString() },
            { "Point", (parser, x) => x.Point = parser.ParseInteger() },
            { "Bold", (parser, x) => x.Bold = parser.ParseBoolean() }
        };

        public string Name { get; private set; }

        public string Font { get; private set; }
        public int Point { get; private set; }
        public bool Bold { get; private set; }
    }
}
