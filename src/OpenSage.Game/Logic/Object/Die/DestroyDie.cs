using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public sealed class DestroyDie : DieModule
    {
        private readonly GameObject _gameObject;

        internal DestroyDie(GameObject gameObject, DestroyDieModuleData moduleData) : base(moduleData)
        {
            _gameObject = gameObject;
        }

        private protected override void Die(BehaviorUpdateContext context, DeathType deathType)
        {
            context.GameContext.GameLogic.DestroyObject(_gameObject);
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
