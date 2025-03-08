using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public sealed class SupplyWarehouseCreate : CreateModule, IDestroyModule
{
    public SupplyWarehouseCreate(GameObject gameObject, GameContext context) : base(gameObject, context)
    {
    }

    public override void OnCreate()
    {
        foreach (var player in Context.Scene3D.Players)
        {
            player.SupplyManager.AddSupplyWarehouse(GameObject);
        }
    }

    public void OnDestroy()
    {
        foreach (var player in Context.Scene3D.Players)
        {
            player.SupplyManager.RemoveSupplyWarehouse(GameObject);
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
/// Ensures the object acts as a source for supply collection.
/// </summary>
public sealed class SupplyWarehouseCreateModuleData : CreateModuleData
{
    internal static SupplyWarehouseCreateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    private static readonly IniParseTable<SupplyWarehouseCreateModuleData> FieldParseTable = new IniParseTable<SupplyWarehouseCreateModuleData>();

    internal override BehaviorModule CreateModule(GameObject gameObject, GameContext context)
    {
        return new SupplyWarehouseCreate(gameObject, context);
    }
}
