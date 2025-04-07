using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class TechBuildingBehavior : UpdateModule
{
    public TechBuildingBehavior(GameObject gameObject, IGameEngine gameEngine) : base(gameObject, gameEngine)
    {
    }

    public override UpdateSleepTime Update()
    {
        // TODO(Port): Use correct value.
        return UpdateSleepTime.None;
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();
    }
}

/// <summary>
/// This module is required when KindOf contains TECH_BUILDING.
/// </summary>
public sealed class TechBuildingBehaviorModuleData : BehaviorModuleData
{
    internal static TechBuildingBehaviorModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<TechBuildingBehaviorModuleData> FieldParseTable = new IniParseTable<TechBuildingBehaviorModuleData>();

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new TechBuildingBehavior(gameObject, gameEngine);
    }
}
