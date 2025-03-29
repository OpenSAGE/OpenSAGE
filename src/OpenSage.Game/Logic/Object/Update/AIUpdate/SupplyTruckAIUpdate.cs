using OpenSage.Data.Ini;
using OpenSage.Logic.AI;

namespace OpenSage.Logic.Object;

public class SupplyTruckAIUpdate : SupplyAIUpdate
{
    internal override SupplyTruckAIUpdateModuleData ModuleData { get; }

    private readonly SupplyAIUpdateStateMachine _stateMachine;
    private ObjectId _dockId;
    private int _unknownInt;
    private bool _unknownBool;

    internal SupplyTruckAIUpdate(GameObject gameObject, IGameEngine gameEngine, SupplyTruckAIUpdateModuleData moduleData) : base(gameObject, gameEngine, moduleData)
    {
        ModuleData = moduleData;
        _stateMachine = new SupplyAIUpdateStateMachine(gameObject, gameEngine, this);
    }

    internal override void Load(StatePersister reader)
    {
        reader.PersistVersion(1);

        reader.BeginObject("Base");
        base.Load(reader);
        reader.EndObject();

        reader.PersistObject(_stateMachine);
        reader.PersistObjectId(ref _dockId);
        reader.PersistInt32(ref _unknownInt);
        reader.PersistBoolean(ref _unknownBool);
    }
}

public class SupplyTruckAIUpdateModuleData : SupplyAIUpdateModuleData
{
    internal new static SupplyTruckAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

    internal new static readonly IniParseTable<SupplyTruckAIUpdateModuleData> FieldParseTable = SupplyAIUpdateModuleData.FieldParseTable
        .Concat(new IniParseTable<SupplyTruckAIUpdateModuleData> { });

    internal override BehaviorModule CreateModule(GameObject gameObject, IGameEngine gameEngine)
    {
        return new SupplyTruckAIUpdate(gameObject, gameEngine, this);
    }
}
