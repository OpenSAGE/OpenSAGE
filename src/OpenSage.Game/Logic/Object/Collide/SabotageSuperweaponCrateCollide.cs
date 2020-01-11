using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to play the SabotageBuilding sound definition when triggered.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class SabotageSuperweaponCrateCollideModuleData : CrateCollideModuleData
    {
        internal static SabotageSuperweaponCrateCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SabotageSuperweaponCrateCollideModuleData> FieldParseTable = CrateCollideModuleData.FieldParseTable
            .Concat(new IniParseTable<SabotageSuperweaponCrateCollideModuleData>());
    }
}
