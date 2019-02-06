using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class MusicTrack : BaseSingleSound
    {
        internal static MusicTrack Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static new readonly IniParseTable<MusicTrack> FieldParseTable = BaseSingleSound.FieldParseTable
            .Concat(new IniParseTable<MusicTrack>
            {
                { "Filename", (parser, x) => x.Filename = parser.ParseFileName() },
            });

        public string Filename { get; private set; }
    }
}
