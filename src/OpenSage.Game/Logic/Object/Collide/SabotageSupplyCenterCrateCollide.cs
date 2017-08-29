using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to play the SabotageBuilding sound definition when triggered.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class SabotageSupplyCenterCrateCollideModuleData : CrateCollideModuleData
    {
        internal static SabotageSupplyCenterCrateCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SabotageSupplyCenterCrateCollideModuleData> FieldParseTable = CrateCollideModuleData.FieldParseTable
            .Concat(new IniParseTable<SabotageSupplyCenterCrateCollideModuleData>
            {
                { "StealCashAmount", (parser, x) => x.StealCashAmount = parser.ParseInteger() },
            });

        public int StealCashAmount { get; private set; }
    }
}
