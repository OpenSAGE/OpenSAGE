using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SpyVisionSpecialPowerModuleData : SpecialPowerModuleData
    {
        internal static new SpyVisionSpecialPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SpyVisionSpecialPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<SpyVisionSpecialPowerModuleData>
            {
                { "BaseDuration", (parser, x) => x.BaseDuration = parser.ParseInteger() },
                { "BonusDurationPerCaptured", (parser, x) => x.BonusDurationPerCaptured = parser.ParseInteger() },
                { "MaxDuration", (parser, x) => x.MaxDuration = parser.ParseInteger() }
            });

        public int BaseDuration { get; private set; }
        public int BonusDurationPerCaptured { get; private set; }
        public int MaxDuration { get; private set; }
    }
}
