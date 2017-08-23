using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Requires the object to have KindOf = HARVESTER.
    /// </summary>
    public sealed class SupplyTruckAIUpdate : ObjectBehavior
    {
        internal static SupplyTruckAIUpdate Parse(IniParser parser) => parser.ParseBlock(BaseFieldParseTable);

        internal static readonly IniParseTable<SupplyTruckAIUpdate> BaseFieldParseTable = new IniParseTable<SupplyTruckAIUpdate>
        {
            { "MaxBoxes", (parser, x) => x.MaxBoxes = parser.ParseInteger() },
            { "SupplyCenterActionDelay", (parser, x) => x.SupplyCenterActionDelay = parser.ParseInteger() },
            { "SupplyWarehouseActionDelay", (parser, x) => x.SupplyWarehouseActionDelay = parser.ParseInteger() },
            { "SupplyWarehouseScanDistance", (parser, x) => x.SupplyWarehouseScanDistance = parser.ParseInteger() },
            { "SuppliesDepletedVoice", (parser, x) => x.SuppliesDepletedVoice = parser.ParseAssetReference() }
        };

        public int MaxBoxes { get; private set; }
        public int SupplyCenterActionDelay { get; private set; }
        public int SupplyWarehouseActionDelay { get; private set; }
        public int SupplyWarehouseScanDistance { get; private set; }
        public string SuppliesDepletedVoice { get; private set; }
    }
}
