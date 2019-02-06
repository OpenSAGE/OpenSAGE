using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class SkyboxTextureSet
    {
        internal static SkyboxTextureSet Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<SkyboxTextureSet> FieldParseTable = new IniParseTable<SkyboxTextureSet>
        {
            { "SkyboxTextureN", (parser, x) => x.SkyboxTextureN = parser.ParseFileName() },
            { "SkyboxTextureE", (parser, x) => x.SkyboxTextureE = parser.ParseFileName() },
            { "SkyboxTextureS", (parser, x) => x.SkyboxTextureS = parser.ParseFileName() },
            { "SkyboxTextureW", (parser, x) => x.SkyboxTextureW = parser.ParseFileName() },
            { "SkyboxTextureT", (parser, x) => x.SkyboxTextureT = parser.ParseFileName() },
        };

        public string Name { get; private set; }

        public string SkyboxTextureN { get; private set; }
        public string SkyboxTextureE { get; private set; }
        public string SkyboxTextureS { get; private set; }
        public string SkyboxTextureW { get; private set; }
        public string SkyboxTextureT { get; private set; }
    }
}
