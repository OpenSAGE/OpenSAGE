using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the use of VoiceRepair, VoiceBuildResponse, VoiceSupply, VoiceNoBuild, and 
    /// VoiceTaskComplete within UnitSpecificSounds section of the object.
    /// Requires Kindof = DOZER HARVESTER.
    /// </summary>
    public sealed class WorkerAIUpdate : ObjectBehavior
    {
        internal static WorkerAIUpdate Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<WorkerAIUpdate> FieldParseTable = new IniParseTable<WorkerAIUpdate>
        {
            { "RepairHealthPercentPerSecond", (parser, x) => x.RepairHealthPercentPerSecond = parser.ParsePercentage() },
            { "BoredTime", (parser, x) => x.BoredTime = parser.ParseInteger() },
            { "BoredRange", (parser, x) => x.BoredRange = parser.ParseInteger() },
            { "MaxBoxes", (parser, x) => x.MaxBoxes = parser.ParseInteger() },
            { "SupplyCenterActionDelay", (parser, x) => x.SupplyCenterActionDelay = parser.ParseInteger() },
            { "SupplyWarehouseActionDelay", (parser, x) => x.SupplyWarehouseActionDelay = parser.ParseInteger() },
            { "SupplyWarehouseScanDistance", (parser, x) => x.SupplyWarehouseScanDistance = parser.ParseInteger() },
            { "SuppliesDepletedVoice", (parser, x) => x.SuppliesDepletedVoice = parser.ParseAssetReference() },
            { "AutoAcquireEnemiesWhenIdle", (parser, x) => x.AutoAcquireEnemiesWhenIdle = parser.ParseBoolean() }
        };

        public float RepairHealthPercentPerSecond { get; private set; }
        public int BoredTime { get; private set; }
        public int BoredRange { get; private set; }
        public int MaxBoxes { get; private set; }
        public int SupplyCenterActionDelay { get; private set; }
        public int SupplyWarehouseActionDelay { get; private set; }
        public int SupplyWarehouseScanDistance { get; private set; }
        public string SuppliesDepletedVoice { get; private set; }
        public bool AutoAcquireEnemiesWhenIdle { get; private set; }
    }
}
