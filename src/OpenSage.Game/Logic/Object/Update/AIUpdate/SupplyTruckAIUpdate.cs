using System.IO;
using OpenSage.Data.Ini;
using OpenSage.FileFormats;

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

        internal override void Load(BinaryReader reader)
        {
            var version = reader.ReadVersion();
            if (version != 1)
            {
                throw new InvalidDataException();
            }

            base.Load(reader);

            // TODO
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
