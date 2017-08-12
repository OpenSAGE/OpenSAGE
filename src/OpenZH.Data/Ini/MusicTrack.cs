using OpenZH.Data.Ini.Parser;

namespace OpenZH.Data.Ini
{
    public sealed class MusicTrack
    {
        internal static MusicTrack Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<MusicTrack> FieldParseTable = new IniParseTable<MusicTrack>
        {
            { "Filename", (parser, x) => x.Filename = parser.ParseAsciiString() }
        };

        public string Name { get; private set; }

        public string Filename { get; private set; }
    }
}
