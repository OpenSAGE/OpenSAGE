using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class BoredUpdateModuleData : UpdateModuleData
    {
        internal static BoredUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BoredUpdateModuleData> FieldParseTable = new IniParseTable<BoredUpdateModuleData>
        {
            { "ScanDelayTime", (parser, x) => x.ScanDelayTime = parser.ParseInteger() },
            { "ScanDistance", (parser, x) => x.ScanDistance = parser.ParseInteger() },
            { "BoredFilter", (parser, x) => x.BoredFilter = ObjectFilter.Parse(parser) },
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
        };

        public int ScanDelayTime { get; private set; }
        public int ScanDistance { get; private set; }
        public ObjectFilter BoredFilter { get; private set; }
        public string SpecialPowerTemplate { get; private set; }
    }
}
