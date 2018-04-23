using OpenSage.Data.Ini.Parser;

namespace OpenSage.Data.Ini
{
    [AddedIn(SageGame.Bfme)]
    public sealed class HouseColor
    {
        internal static HouseColor Parse(IniParser parser) => parser.ParseTopLevelBlock(FieldParseTable);

        private static readonly IniParseTable<HouseColor> FieldParseTable = new IniParseTable<HouseColor>
        {
            { "BaseTexture", (parser, x) => x.BaseTexture = parser.ParseAssetReference() },
            { "HouseTexture", (parser, x) => x.HouseTexture = parser.ParseAssetReference() },
        };

        public string BaseTexture { get; private set; }
        public string HouseTexture { get; private set; }
    }
}
