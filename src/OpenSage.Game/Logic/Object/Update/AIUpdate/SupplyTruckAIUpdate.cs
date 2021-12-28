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

        internal override void Load(SaveFileReader reader)
        {
            reader.ReadVersion(1);

            base.Load(reader);

            var stateMachine = new WorkerAIUpdateStateMachine2();
            stateMachine.Load(reader);

            reader.ReadObjectID(ref _dockId);

            _unknownInt = reader.ReadInt32();

            _unknownBool = reader.ReadBoolean();
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
