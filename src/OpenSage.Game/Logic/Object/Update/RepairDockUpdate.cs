using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class RepairDockUpdate : DockUpdate
    {
        internal RepairDockUpdate(GameObject gameObject, RepairDockUpdateModuleData moduleData)
            : base(gameObject, moduleData)
        {

        }

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var unknown1 = reader.ReadInt32();
            if (unknown1 != 0)
            {
                throw new InvalidStateException();
            }

            var unknown2 = reader.ReadInt32();
            if (unknown2 != 0)
            {
                throw new InvalidStateException();
            }
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
