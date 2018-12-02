using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class ProductionSpeedBonusModuleData : SpecialPowerModuleData
    {
        internal static new ProductionSpeedBonusModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<ProductionSpeedBonusModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<ProductionSpeedBonusModuleData>
            {
                { "NumberOfFrames", (parser, x) => x.NumberOfFrames = parser.ParseInteger() },
                { "SpeedMulitplier", (parser, x) => x.SpeedMulitplier = parser.ParseFloat() },
                { "Type", (parser, x) => x.Types = parser.ParseAssetReferenceArray() }
            });

        public int NumberOfFrames { get; private set; }
        public float SpeedMulitplier { get; private set; }
        public string[] Types { get; private set; }
    }
}
