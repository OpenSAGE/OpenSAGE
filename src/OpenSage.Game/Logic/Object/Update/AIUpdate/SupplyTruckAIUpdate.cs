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
    }

    public sealed class SupplyTruckAIUpdateModuleData : SupplyAIUpdateModuleData
    {
        internal new static SupplyTruckAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private new static readonly IniParseTable<SupplyTruckAIUpdateModuleData> FieldParseTable = SupplyAIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<SupplyTruckAIUpdateModuleData>{});

        internal override AIUpdate CreateAIUpdate(GameObject gameObject)
        {
            return new SupplyTruckAIUpdate(gameObject, this);
        }
    }
}
