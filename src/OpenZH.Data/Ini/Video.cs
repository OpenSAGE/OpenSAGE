using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class Video
    {
        internal static Video Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<Video> FieldParseTable = new IniParseTable<Video>
        {
            { "Filename", (parser, x) => x.Filename = parser.ParseAsciiString() },
            { "Comment", (parser, x) => x.Comment = parser.ParseAsciiString() },
        };

        public string Name { get; private set; }

        public string Filename { get; private set; }
        public string Comment { get; private set; }
    }
}
