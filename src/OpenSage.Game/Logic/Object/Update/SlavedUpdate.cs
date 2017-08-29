using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class SlavedUpdateModuleData : UpdateModuleData
    {
        internal static SlavedUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<SlavedUpdateModuleData> FieldParseTable = new IniParseTable<SlavedUpdateModuleData>
        {
            { "GuardMaxRange", (parser, x) => x.GuardMaxRange = parser.ParseInteger() },
            { "GuardWanderRange", (parser, x) => x.GuardWanderRange = parser.ParseInteger() },
            { "AttackRange", (parser, x) => x.AttackRange = parser.ParseInteger() },
            { "AttackWanderRange", (parser, x) => x.AttackWanderRange = parser.ParseInteger() },
            { "ScoutRange", (parser, x) => x.ScoutRange = parser.ParseInteger() },
            { "ScoutWanderRange", (parser, x) => x.ScoutWanderRange = parser.ParseInteger() },
            { "RepairRange", (parser, x) => x.RepairRange = parser.ParseInteger() },
            { "RepairMinAltitude", (parser, x) => x.RepairMinAltitude = parser.ParseFloat() },
            { "RepairMaxAltitude", (parser, x) => x.RepairMaxAltitude = parser.ParseFloat() },
            { "RepairRatePerSecond", (parser, x) => x.RepairRatePerSecond = parser.ParseFloat() },
            { "RepairWhenBelowHealth%", (parser, x) => x.RepairWhenBelowHealthPercent = parser.ParseInteger() },
            { "RepairMinReadyTime", (parser, x) => x.RepairMinReadyTime = parser.ParseInteger() },
            { "RepairMaxReadyTime", (parser, x) => x.RepairMaxReadyTime = parser.ParseInteger() },
            { "RepairMinWeldTime", (parser, x) => x.RepairMinWeldTime = parser.ParseInteger() },
            { "RepairMaxWeldTime", (parser, x) => x.RepairMaxWeldTime = parser.ParseInteger() },
            { "RepairWeldingSys", (parser, x) => x.RepairWeldingSys = parser.ParseAssetReference() },
            { "RepairWeldingFXBone", (parser, x) => x.RepairWeldingFXBone = parser.ParseBoneName() },
            { "DistToTargetToGrantRangeBonus", (parser, x) => x.DistToTargetToGrantRangeBonus = parser.ParseInteger() },
            { "StayOnSameLayerAsMaster", (parser, x) => x.StayOnSameLayerAsMaster = parser.ParseBoolean() }
        };

        public int GuardMaxRange { get; private set; }
        public int GuardWanderRange { get; private set; }
        public int AttackRange { get; private set; }
        public int AttackWanderRange { get; private set; }
        public int ScoutRange { get; private set; }
        public int ScoutWanderRange { get; private set; }
        public int RepairRange { get; private set; }
        public float RepairMinAltitude { get; private set; }
        public float RepairMaxAltitude { get; private set; }
        public float RepairRatePerSecond { get; private set; }
        public int RepairWhenBelowHealthPercent { get; private set; }
        public int RepairMinReadyTime { get; private set; }
        public int RepairMaxReadyTime { get; private set; }
        public int RepairMinWeldTime { get; private set; }
        public int RepairMaxWeldTime { get; private set; }
        public string RepairWeldingSys { get; private set; }
        public string RepairWeldingFXBone { get; private set; }
        public int DistToTargetToGrantRangeBonus { get; private set; }
        public bool StayOnSameLayerAsMaster { get; private set; }
    }
}
