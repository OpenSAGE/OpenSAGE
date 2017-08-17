using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class CashBountyPowerBehavior : ObjectBehavior
    {
        internal static CashBountyPowerBehavior Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CashBountyPowerBehavior> FieldParseTable = new IniParseTable<CashBountyPowerBehavior>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "Bounty", (parser, x) => x.Bounty = parser.ParsePercentage() },
        };

        public string SpecialPowerTemplate { get; private set; }
        public float Bounty { get; private set; }
    }
}
