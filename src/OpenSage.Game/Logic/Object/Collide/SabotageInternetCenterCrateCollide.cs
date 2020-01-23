using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to play the SabotageBuilding sound definition when triggered.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class SabotageInternetCenterCrateCollideModuleData : CrateCollideModuleData
    {
        internal static SabotageInternetCenterCrateCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SabotageInternetCenterCrateCollideModuleData> FieldParseTable = CrateCollideModuleData.FieldParseTable
            .Concat(new IniParseTable<SabotageInternetCenterCrateCollideModuleData>
            {
                { "SabotageDuration", (parser, x) => x.SabotageDuration = parser.ParseInteger() },
            });

        public int SabotageDuration { get; private set; }
    }
}
