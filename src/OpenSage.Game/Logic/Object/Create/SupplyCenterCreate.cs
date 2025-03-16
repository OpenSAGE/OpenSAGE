using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class SupplyCenterCreate : CreateModule, IDestroyModule
{
    public SupplyCenterCreate(GameObject gameObject, GameEngine gameEngine) : base(gameObject, gameEngine)
    {
    }

    protected override void OnBuildCompleteImpl()
    {
        foreach (var player in GameEngine.Scene3D.Players)
        {
            player.SupplyManager.AddSupplyCenter(GameObject);
        }
    }

    public void OnDestroy()
    {
        foreach (var player in GameEngine.Scene3D.Players)
        {
            player.SupplyManager.RemoveSupplyCenter(GameObject);
        }
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
/// Requires the <see cref="ObjectKinds.SupplySource"/> KindOf defined in order to work properly.
/// Ensures the object acts as a destination for collection of supplies.
/// </summary>
public sealed class SupplyCenterCreateModuleData : CreateModuleData
{
    internal static SupplyCenterCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<SupplyCenterCreateModuleData> FieldParseTable = new IniParseTable<SupplyCenterCreateModuleData>();

    internal override BehaviorModule CreateModule(GameObject gameObject, GameEngine gameEngine)
    {
        return new SupplyCenterCreate(gameObject, gameEngine);
    }
}
