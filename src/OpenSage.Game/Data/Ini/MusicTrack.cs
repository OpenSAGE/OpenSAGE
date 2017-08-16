using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
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
            { "Filename", (parser, x) => x.Filename = parser.ParseFileName() },
            { "Volume", (parser, x) => x.Volume = parser.ParseFloat() }
        };

        public string Name { get; private set; }

        public string Filename { get; private set; }
        public float Volume { get; private set; } = 100;
    }
}
