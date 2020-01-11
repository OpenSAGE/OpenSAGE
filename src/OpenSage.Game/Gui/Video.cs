using OpenSage.Data.Ini;

namespace OpenSage.Gui
{
    public sealed class Video : BaseAsset
    {
        internal static Video Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("Video", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<Video> FieldParseTable = new IniParseTable<Video>
        {
            { "Filename", (parser, x) => x.Filename = parser.ParseFileName() },
            { "Comment", (parser, x) => x.Comment = parser.ParseString() },
            { "Volume", (parser, x) => x.Volume = parser.ParseInteger() },
            { "IsDefault", (parser, x) => x.IsDefault = parser.ParseBoolean() },
        };

        public string Filename { get; private set; }
        public string Comment { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int Volume { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool IsDefault { get; private set; }
    }
}
