using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to require DockWaitingN, DockEndN, DockActionN and DockStartN bones, where N 
    /// should correspond to <see cref="NumberApproachPositions"/>.
    /// </summary>
    public sealed class RepairDockUpdateModuleData : DockUpdateModuleData
    {
        internal static RepairDockUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<RepairDockUpdateModuleData> FieldParseTable = DockUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<RepairDockUpdateModuleData>
            {
                { "TimeForFullHeal", (parser, x) => x.TimeForFullHeal = parser.ParseInteger() },
            });

        public int TimeForFullHeal { get; private set; }
    }
}
