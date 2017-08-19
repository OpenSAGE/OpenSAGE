using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Hardcoded to require DockWaitingN, DockEndN, DockActionN and DockStartN bones, where N 
    /// should correspond to <see cref="NumberApproachPositions"/>.
    /// </summary>
    public sealed class RepairDockUpdate : ObjectBehavior
    {
        internal static RepairDockUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<RepairDockUpdate> FieldParseTable = new IniParseTable<RepairDockUpdate>
        {
            { "TimeForFullHeal", (parser, x) => x.TimeForFullHeal = parser.ParseInteger() },
            { "NumberApproachPositions", (parser, x) => x.NumberApproachPositions = parser.ParseInteger() }
        };

        public int TimeForFullHeal { get; private set; }
        public int NumberApproachPositions { get; private set; }
    }
}
