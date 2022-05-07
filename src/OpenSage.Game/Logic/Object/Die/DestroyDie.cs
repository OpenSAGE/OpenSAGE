using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class DestroyDie : DieModule
    {
        private readonly GameObject _gameObject;
        private readonly DestroyDieModuleData _moduleData;

        internal DestroyDie(GameObject gameObject, DestroyDieModuleData moduleData)
        {
            _gameObject = gameObject;
            _moduleData = moduleData;
        }

        internal override void OnDie(BehaviorUpdateContext context, DeathType deathType)
        {
            context.GameContext.GameObjects.DestroyObject(_gameObject);
        }

        internal override void Load(StatePersister reader)
        {
            reader.PersistVersion(1);

            reader.BeginObject("Base");
            base.Load(reader);
            reader.EndObject();
        }
    }

    public sealed class DestroyDieModuleData : DieModuleData
    {
        internal static DestroyDieModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<DestroyDieModuleData> FieldParseTable = DieModuleData.FieldParseTable
            .Concat(new IniParseTable<DestroyDieModuleData>());

        internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
        {
            return new DestroyDie(gameObject, this);
        }
    }
}
