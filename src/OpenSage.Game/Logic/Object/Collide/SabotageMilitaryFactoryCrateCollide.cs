using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to play the SabotageBuilding sound definition when triggered.
    /// </summary>
    [AddedIn(SageGame.CncGeneralsZeroHour)]
    public sealed class SabotageMilitaryFactoryCrateCollideModuleData : CrateCollideModuleData
    {
        internal static SabotageMilitaryFactoryCrateCollideModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<SabotageMilitaryFactoryCrateCollideModuleData> FieldParseTable = CrateCollideModuleData.FieldParseTable
            .Concat(new IniParseTable<SabotageMilitaryFactoryCrateCollideModuleData>
            {
                { "SabotageDuration", (parser, x) => x.SabotageDuration = parser.ParseInteger() },
            });

        public int SabotageDuration { get; private set; }
    }
}
