using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public class SupplyTruckAIUpdate : SupplyAIUpdate
    {
        private SupplyTruckAIUpdateModuleData _moduleData;

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

            var unknown1 = reader.ReadInt32();
            if (unknown1 != 0)
            {
                throw new InvalidStateException();
            }

            var unknown2 = reader.ReadInt32();

            var unknown7 = reader.ReadBoolean();
            if (unknown7)
            {
                throw new InvalidStateException();
            }
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
