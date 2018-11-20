using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class CashBountyPowerModuleData : SpecialPowerModuleData
    {
        internal static new CashBountyPowerModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<CashBountyPowerModuleData> FieldParseTable = SpecialPowerModuleData.FieldParseTable
            .Concat(new IniParseTable<CashBountyPowerModuleData>
            {
                { "Bounty", (parser, x) => x.Bounty = parser.ParsePercentage() },
            });

        public float Bounty { get; private set; }
    }
}
