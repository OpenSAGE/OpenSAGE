using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class DestroyDie : DieModule
{
    internal DestroyDie(GameObject gameObject, GameContext context, DestroyDieModuleData moduleData)
        : base(gameObject, context, moduleData)
    {
    }

    private protected override void Die(BehaviorUpdateContext context, DeathType deathType)
    {
        context.GameContext.GameLogic.DestroyObject(GameObject);
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
        return new DestroyDie(gameObject, context, this);
    }
}
