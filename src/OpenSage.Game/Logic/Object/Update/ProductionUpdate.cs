using System.Collections.Generic;
using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    /// <summary>
    /// Required on an object that uses PublicTimer code for any SpecialPower and/or required for 
    /// units/structures with object upgrades.
    /// </summary>
    public sealed class ProductionUpdateModuleData : UpdateModuleData
    {
        internal static ProductionUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ProductionUpdateModuleData> FieldParseTable = new IniParseTable<ProductionUpdateModuleData>
        {
            { "NumDoorAnimations", (parser, x) => x.NumDoorAnimations = parser.ParseInteger() },
            { "DoorOpeningTime", (parser, x) => x.DoorOpeningTime = parser.ParseInteger() },
            { "DoorWaitOpenTime", (parser, x) => x.DoorWaitOpenTime = parser.ParseInteger() },
            { "DoorCloseTime", (parser, x) => x.DoorCloseTime = parser.ParseInteger() },
            { "ConstructionCompleteDuration", (parser, x) => x.ConstructionCompleteDuration = parser.ParseInteger() },
            { "MaxQueueEntries", (parser, x) => x.MaxQueueEntries = parser.ParseInteger() },
            { "QuantityModifier", (parser, x) => x.QuantityModifier = Object.QuantityModifier.Parse(parser) },

            { "DisabledTypesToProcess", (parser, x) => x.DisabledTypesToProcess = parser.ParseEnumBitArray<DisabledType>() },
            { "VeteranUnitsFromVeteranFactory", (parser, x) => x.VeteranUnitsFromVeteranFactory = parser.ParseBoolean() },
            { "SetBonusModelConditionOnSpeedBonus", (parser, x) => x.SetBonusModelConditionOnSpeedBonus = parser.ParseBoolean() },
            { "BonusForType", (parser, x) => x.BonusForType = parser.ParseString() },
            { "SpeedBonusAudioLoop", (parser, x) => x.SpeedBonusAudioLoop = parser.ParseAssetReference() },
            { "UnitInvulnerableTime", (parser, x) => x.UnitInvulnerableTime = parser.ParseInteger() },
            { "GiveNoXP", (parser, x) => x.GiveNoXP = parser.ParseBoolean() },
            { "SpecialPrepModelconditionTime", (parser, x) => x.SpecialPrepModelconditionTime = parser.ParseInteger() },
            { "ProductionModifier", (parser, x) => x.ProductionModifiers.Add(ProductionModifier.Parse(parser)) }
        };

        /// <summary>
        /// Specifies how many doors to use when unit training is complete.
        /// </summary>
        public int NumDoorAnimations { get; private set; }

        public int DoorOpeningTime { get; private set; }
        public int DoorWaitOpenTime { get; private set; }
        public int DoorCloseTime { get; private set; }
        public int ConstructionCompleteDuration { get; private set; }
        public int MaxQueueEntries { get; private set; }

        /// <summary>
        /// Red Guards use this so that they can come out of the barracks in pairs.
        /// </summary>
        public QuantityModifier? QuantityModifier { get; private set; }

        public BitArray<DisabledType> DisabledTypesToProcess { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool VeteranUnitsFromVeteranFactory { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool SetBonusModelConditionOnSpeedBonus { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string BonusForType { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public string SpeedBonusAudioLoop { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int UnitInvulnerableTime { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public bool GiveNoXP { get; private set; }

        [AddedIn(SageGame.Bfme)]
        public int SpecialPrepModelconditionTime { get; private set; }

        [AddedIn(SageGame.Bfme2)]
        public List<ProductionModifier> ProductionModifiers { get; } = new List<ProductionModifier>();
    }

    public struct QuantityModifier
    {
        internal static QuantityModifier Parse(IniParser parser)
        {
            return new QuantityModifier
            {
                ObjectName = parser.ParseAssetReference(),
                Count = parser.ParseInteger()
            };
        }

        public string ObjectName { get; private set; }
        public int Count { get; private set; }
    }

    [AddedIn(SageGame.Bfme2)]
    public class ProductionModifier
    {
        internal static ProductionModifier Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<ProductionModifier> FieldParseTable = new IniParseTable<ProductionModifier>
        {
            { "RequiredUpgrade", (parser, x) => x.RequiredUpgrade = parser.ParseAssetReference() },
            { "CostMultiplier", (parser, x) => x.CostMultiplier = parser.ParseFloat() },
            { "TimeMultiplier", (parser, x) => x.TimeMultiplier = parser.ParseFloat() },
            { "ModifierFilter", (parser, x) => x.ModifierFilter = ObjectFilter.Parse(parser) },
            { "HeroPurchase", (parser, x) => x.HeroPurchase = parser.ParseBoolean() },
            { "HeroRevive", (parser, x) => x.HeroRevive = parser.ParseBoolean() }
        };

        public string RequiredUpgrade { get; private set; }
        public float CostMultiplier { get; private set; }
        public float TimeMultiplier { get; private set; }
        public ObjectFilter ModifierFilter { get; private set; }
        public bool HeroPurchase { get; private set; }
        public bool HeroRevive { get; private set; }
    }

    public enum DisabledType
    {
        [IniEnum("DISABLED_HELD")]
        DisabledHeld,

        [IniEnum("DISABLED_UNDERPOWERED")]
        DisabledUnderpowered,
    }
}
