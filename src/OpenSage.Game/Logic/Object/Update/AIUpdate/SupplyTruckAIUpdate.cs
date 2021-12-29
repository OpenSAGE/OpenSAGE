using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public class SupplyTruckAIUpdate : SupplyAIUpdate
    {
        private readonly SupplyTruckAIUpdateModuleData _moduleData;

        private readonly WorkerAIUpdateStateMachine2 _stateMachine = new();
        private uint _dockId;
        private int _unknownInt;
        private bool _unknownBool;

        internal SupplyTruckAIUpdate(GameObject gameObject, SupplyTruckAIUpdateModuleData moduleData) : base(gameObject, moduleData)
        {
            _moduleData = moduleData;
        }

        internal override void Update(BehaviorUpdateContext context)
        {
            base.Update(context);
        }

        internal override void Load(StatePersister reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            _stateMachine.Load(reader);

            reader.ReadObjectID(ref _dockId);
            reader.ReadInt32(ref _unknownInt);
            reader.ReadBoolean(ref _unknownBool);
        }
    }

    public class SupplyTruckAIUpdateModuleData : SupplyAIUpdateModuleData
    {
        internal new static SupplyTruckAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        internal new static readonly IniParseTable<SupplyTruckAIUpdateModuleData> FieldParseTable = SupplyAIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<SupplyTruckAIUpdateModuleData>{});

        internal override AIUpdate CreateAIUpdate(GameObject gameObject)
        {
            return new SupplyTruckAIUpdate(gameObject, this);
        }
    }
}
