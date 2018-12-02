using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public sealed class DevastateSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new DevastateSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DevastateSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<DevastateSpecialPowerModuleData>
            {
                { "Radius", (parser, x) => x.Radius = parser.ParseInteger() },
                { "TreeValueMultiplier", (parser, x) => x.TreeValueMultiplier = parser.ParsePercentage() },
                { "TreeValueTotalCap", (parser, x) => x.TreeValueTotalCap = parser.ParseInteger() },
                { "FX", (parser, x) => x.FX = parser.ParseAssetReference() }
            });

        public int Radius { get; private set; }
        public float TreeValueMultiplier { get; private set; }
        public int TreeValueTotalCap { get; private set; }
        public string FX { get; private set; }
    }
}
