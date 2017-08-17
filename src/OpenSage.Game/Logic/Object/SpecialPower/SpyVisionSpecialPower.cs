using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SpyVisionSpecialPower : ObjectBehavior
    {
        internal static SpyVisionSpecialPower Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SpyVisionSpecialPower> FieldParseTable = new IniParseTable<SpyVisionSpecialPower>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "BaseDuration", (parser, x) => x.BaseDuration = parser.ParseInteger() },
            { "BonusDurationPerCaptured", (parser, x) => x.BonusDurationPerCaptured = parser.ParseInteger() },
            { "MaxDuration", (parser, x) => x.MaxDuration = parser.ParseInteger() }
        };

        public string SpecialPowerTemplate { get; private set; }
        public int BaseDuration { get; private set; }
        public int BonusDurationPerCaptured { get; private set; }
        public int MaxDuration { get; private set; }
    }
}
