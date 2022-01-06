using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class RepairDockUpdate : DockUpdate
    {
        internal RepairDockUpdate(GameObject gameObject, RepairDockUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {

        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();

            reader.SkipUnknownBytes(8);
        }
    }

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

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new RepairDockUpdate(gameObject, this);
        }
    }
}
