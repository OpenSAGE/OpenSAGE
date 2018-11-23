using System;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Allows the use of VoiceRepair, VoiceBuildResponse, VoiceSupply, VoiceNoBuild, and 
    /// VoiceTaskComplete within UnitSpecificSounds section of the object.
    /// Requires Kindof = DOZER HARVESTER.
    /// </summary>
    public sealed class WorkerAIUpdateModuleData : AIUpdateModuleData
    {
        internal static new WorkerAIUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<WorkerAIUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<WorkerAIUpdateModuleData>
            {
                { "RepairHealthPercentPerSecond", (parser, x) => x.RepairHealthPercentPerSecond = parser.ParsePercentage() },
                { "BoredTime", (parser, x) => x.BoredTime = parser.ParseInteger() },
                { "BoredRange", (parser, x) => x.BoredRange = parser.ParseInteger() },
                { "MaxBoxes", (parser, x) => x.MaxBoxes = parser.ParseInteger() },
                { "SupplyCenterActionDelay", (parser, x) => x.SupplyCenterActionDelay = parser.ParseInteger() },
                { "SupplyWarehouseActionDelay", (parser, x) => x.SupplyWarehouseActionDelay = parser.ParseInteger() },
                { "SupplyWarehouseScanDistance", (parser, x) => x.SupplyWarehouseScanDistance = parser.ParseInteger() },
                { "SuppliesDepletedVoice", (parser, x) => x.SuppliesDepletedVoice = parser.ParseAssetReference() },
                { "UpgradedSupplyBoost", (parser, x) => x.UpgradedSupplyBoost = parser.ParseInteger() },
                { "SpecialContactPoints", (parser, x) => x.SpecialContactPoints = parser.ParseEnumBitArray<ContactPointType>() },
                { "HarvestTrees", (parser, x) => x.HarvestTrees = parser.ParseBoolean() },
                { "HarvestActivationRange", (parser, x) => x.HarvestActivationRange = parser.ParseInteger() },
                { "HarvestPreparationTime", (parser, x) => x.HarvestPreparationTime = parser.ParseInteger() },
                { "HarvestActionTime", (parser, x) => x.HarvestActionTime = parser.ParseInteger() },
            });

        public float RepairHealthPercentPerSecond { get; private set; }
        public int BoredTime { get; private set; }
        public int BoredRange { get; private set; }
        public int MaxBoxes { get; private set; }
        public int SupplyCenterActionDelay { get; private set; }
        public int SupplyWarehouseActionDelay { get; private set; }
        public int SupplyWarehouseScanDistance { get; private set; }
        public string SuppliesDepletedVoice { get; private set; }

        [AddedIn(SageGame.CncGeneralsZeroHour)]
        public int UpgradedSupplyBoost { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public BitArray<ContactPointType> SpecialContactPoints { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool HarvestTrees { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int HarvestActivationRange { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int HarvestPreparationTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int HarvestActionTime { get; private set; }
    }
}
