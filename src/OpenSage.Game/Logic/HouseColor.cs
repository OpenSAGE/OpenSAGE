using OpenSage.Data.Ini;

namespace OpenSage.Logic
{
    [AddedIn(SageGame.Bfme)]
    public sealed class HouseColor : BaseAsset
    {
        internal static HouseColor Parse(IniParser parser)
        {
            var result = parser.ParseTopLevelBlock(FieldParseTable);
            result.SetNameAndInstanceId("HouseColor", result.BaseTexture);
            return result;
        }

        private static readonly IniParseTable<HouseColor> FieldParseTable = new IniParseTable<HouseColor>
        {
            { "BaseTexture", (parser, x) => x.BaseTexture = parser.ParseAssetReference() },
            { "HouseTexture", (parser, x) => x.HouseTexture = parser.ParseAssetReference() },
        };

        public string BaseTexture { get; private set; }
        public string HouseTexture { get; private set; }
    }
}
