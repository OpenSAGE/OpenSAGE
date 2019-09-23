using OpenSage.Data.Ini;
using OpenSage.Mathematics;

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

        public Percentage Bounty { get; private set; }
    }
}
