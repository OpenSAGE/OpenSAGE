using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.BattleForMiddleEarth)]
    public sealed class BannerType
    {
        internal static BannerType Parse(IniParser parser)
        {
            return parser.ParseTopLevelNamedBlock(
                (x, name) => x.Name = name,
                FieldParseTable);
        }

        private static readonly IniParseTable<BannerType> FieldParseTable = new IniParseTable<BannerType>
        {
            { "FlagObj", (parser, x) => x.FlagObj = parser.ParseAssetReference() },
            { "GlowObj", (parser, x) => x.GlowObj = parser.ParseAssetReference() },
            { "WipeMovie", (parser, x) => x.WipeMovie = parser.ParseAssetReference() },
            { "WipeFrame", (parser, x) => x.WipeFrame = parser.ParseInteger() }
        };

        public string Name { get; private set; }

        public string FlagObj { get; private set; }
        public string GlowObj { get; private set; }
        public string WipeMovie { get; private set; }
        public int WipeFrame { get; private set; }
    }
}
