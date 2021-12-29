using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class KeepObjectDie : DieModule
    {
        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);
        }
    }

    public sealed class KeepObjectDieModuleData : DieModuleData
    {
        internal static KeepObjectDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<KeepObjectDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<KeepObjectDieModuleData>
            {
                { "CollapsingTime", (parser, x) => x.CollapsingTime = parser.ParseInteger() },
                { "StayOnRadar", (parser, x) => x.StayOnRadar = parser.ParseBoolean() }
            });

        [AddedIn(SageGame.Bfme2)]
        public int CollapsingTime { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public bool StayOnRadar { get; private set; }

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new KeepObjectDie();
        }
    }
}
