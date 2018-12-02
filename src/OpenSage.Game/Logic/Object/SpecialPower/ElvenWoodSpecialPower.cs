using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ElvenWoodSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new ElvenWoodSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ElvenWoodSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<ElvenWoodSpecialPowerModuleData>
            {
                { "ElvenGroveObject", (parser, x) => x.ElvenGroveObject = parser.ParseAssetReference() },
                { "ElvenWoodRadius", (parser, x) => x.ElvenWoodRadius = parser.ParseInteger() },
                { "ElvenWoodFX", (parser, x) => x.ElvenWoodFX = parser.ParseAssetReference() },
                { "ElvenWoodOCL", (parser, x) => x.ElvenWoodOCL = parser.ParseAssetReference() }
            });

        public string ElvenGroveObject { get; private set; }
        public int ElvenWoodRadius { get; private set; }
        public string ElvenWoodFX { get; private set; }
        public string ElvenWoodOCL { get; private set; }
    }
}
