using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class ImmortalBody : ActiveBody
{
    internal ImmortalBody(GameObject gameObject, GameEngine gameEngine, ImmortalBodyModuleData moduleData)
        : base(gameObject, gameEngine, moduleData)
    {
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
/// Prevents the object from dying or taking damage.
/// </summary>
public sealed class ImmortalBodyModuleData : ActiveBodyModuleData
{
    internal static new ImmortalBodyModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static new readonly IniParseTable<ImmortalBodyModuleData> FieldParseTable = ActiveBodyModuleData.FieldParseTable
        .Concat(new IniParseTable<ImmortalBodyModuleData>());

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new ImmortalBody(gameObject, gameEngine, this);
    }
}
