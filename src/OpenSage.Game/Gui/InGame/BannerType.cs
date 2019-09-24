using OpenSage.Data.Ini;

namespace OpenSage.Gui.InGame
{
    [AddedIn(SageGame.Bfme)]
    public sealed class BannerType : BaseAsset
    {
        internal static BannerType Parse(IniParser parser)
        {
            return parser.ParseNamedBlock(
                (x, name) => x.SetNameAndInstanceId("BannerType", name),
                FieldParseTable);
        }

        private static readonly IniParseTable<BannerType> FieldParseTable = new IniParseTable<BannerType>
        {
            { "FlagObj", (parser, x) => x.FlagObj = parser.ParseAssetReference() },
            { "GlowObj", (parser, x) => x.GlowObj = parser.ParseAssetReference() },
            { "WipeMovie", (parser, x) => x.WipeMovie = parser.ParseAssetReference() },
            { "WipeFrame", (parser, x) => x.WipeFrame = parser.ParseInteger() },
            { "Icon", (parser, x) => x.Icon = parser.ParseAssetReference() },
        };

        public string FlagObj { get; private set; }
        public string GlowObj { get; private set; }
        public string WipeMovie { get; private set; }
        public int WipeFrame { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public string Icon { get; private set; }
    }
}
