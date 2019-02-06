using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    public sealed class WebpageUrl
    {
        internal static WebpageUrl Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<WebpageUrl> FieldParseTable = new IniParseTable<WebpageUrl>
        {
            { "URL", (parser, x) => x.Url = parser.ParseString() },
        };

        public string Name { get; private set; }

        public string Url { get; private set; }
    }
}
