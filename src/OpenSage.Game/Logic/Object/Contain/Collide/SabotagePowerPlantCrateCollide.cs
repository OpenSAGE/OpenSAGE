using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to play the SabotageBuildingPower sound definition when triggered.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class SabotagePowerPlantCrateCollideModuleData : CrateCollideModuleData
    {
        internal static SabotagePowerPlantCrateCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SabotagePowerPlantCrateCollideModuleData> FieldParseTable = CrateCollideModuleData.FieldParseTable
            .Concat(new IniParseTable<SabotagePowerPlantCrateCollideModuleData>
            {
                { "SabotagePowerDuration", (parser, x) => x.SabotagePowerDuration = parser.ParseInteger() },
            });

        public int SabotagePowerDuration { get; private set; }
    }
}
