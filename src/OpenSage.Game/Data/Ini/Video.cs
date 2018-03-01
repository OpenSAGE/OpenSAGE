using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
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
            { "Filename", (parser, x) => x.Filename = parser.ParseFileName() },
            { "Comment", (parser, x) => x.Comment = parser.ParseString() },
            { "Volume", (parser, x) => x.Volume = parser.ParseInteger() },
            { "IsDefault", (parser, x) => x.IsDefault = parser.ParseBoolean() },
        };

        public string Name { get; private set; }

        public string Filename { get; private set; }
        public string Comment { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int Volume { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool IsDefault { get; private set; }
    }
}
