using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class CashBountyPower : ObjectBehavior
    {
        internal static CashBountyPower Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<CashBountyPower> FieldParseTable = new IniParseTable<CashBountyPower>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },
            { "Bounty", (parser, x) => x.Bounty = parser.ParsePercentage() },
        };

        public string SpecialPowerTemplate { get; private set; }
        public float Bounty { get; private set; }
    }
}
