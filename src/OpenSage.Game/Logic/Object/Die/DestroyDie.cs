using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class DestroyDie : DieModule
{
    internal DestroyDie(GameObject gameObject, GameEngine gameEngine, DestroyDieModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
    }

    private protected override void Die(BehaviorUpdateContext context, DeathType deathType)
    {
        context.GameEngine.GameLogic.DestroyObject(GameObject);
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

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new DestroyDie(gameObject, gameEngine, this);
    }
}
