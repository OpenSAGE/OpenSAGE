using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to play the SabotageBuilding sound definition when triggered.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class SabotageCommandCenterCrateCollideModuleData : CrateCollideModuleData
    {
        internal static SabotageCommandCenterCrateCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SabotageCommandCenterCrateCollideModuleData> FieldParseTable = CrateCollideModuleData.FieldParseTable
            .Concat(new IniParseTable<SabotageCommandCenterCrateCollideModuleData>());
    }
}
