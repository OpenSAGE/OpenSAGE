using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    public sealed class BattlePlanUpdateModuleData : UpdateModuleData
    {
        internal static BattlePlanUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static readonly IniParseTable<BattlePlanUpdateModuleData> FieldParseTable = new IniParseTable<BattlePlanUpdateModuleData>
        {
            { "SpecialPowerTemplate", (parser, x) => x.SpecialPowerTemplate = parser.ParseAssetReference() },

            { "BombardmentPlanAnimationTime", (parser, x) => x.BombardmentPlanAnimationTime = parser.ParseInteger() },
            { "HoldTheLinePlanAnimationTime", (parser, x) => x.HoldTheLinePlanAnimationTime = parser.ParseInteger() },
            { "SearchAndDestroyPlanAnimationTime", (parser, x) => x.SearchAndDestroyPlanAnimationTime = parser.ParseInteger() },
            { "TransitionIdleTime", (parser, x) => x.TransitionIdleTime = parser.ParseInteger() },

            { "BombardmentMessageLabel", (parser, x) => x.BombardmentMessageLabel = parser.ParseLocalizedStringKey() },
            { "HoldTheLineMessageLabel", (parser, x) => x.HoldTheLineMessageLabel = parser.ParseLocalizedStringKey() },
            { "SearchAndDestroyMessageLabel", (parser, x) => x.SearchAndDestroyMessageLabel = parser.ParseLocalizedStringKey() },

            { "BombardmentPlanUnpackSoundName", (parser, x) => x.BombardmentPlanUnpackSoundName = parser.ParseAssetReference() },
            { "BombardmentPlanPackSoundName", (parser, x) => x.BombardmentPlanPackSoundName = parser.ParseAssetReference() },
            { "BombardmentAnnouncementName", (parser, x) => x.BombardmentAnnouncementName = parser.ParseAssetReference() },
            { "SearchAndDestroyPlanUnpackSoundName", (parser, x) => x.SearchAndDestroyPlanUnpackSoundName = parser.ParseAssetReference() },
            { "SearchAndDestroyPlanIdleLoopSoundName", (parser, x) => x.SearchAndDestroyPlanIdleLoopSoundName = parser.ParseAssetReference() },
            { "SearchAndDestroyPlanPackSoundName", (parser, x) => x.SearchAndDestroyPlanPackSoundName = parser.ParseAssetReference() },
            { "SearchAndDestroyAnnouncementName", (parser, x) => x.SearchAndDestroyAnnouncementName = parser.ParseAssetReference() },
            { "HoldTheLinePlanUnpackSoundName", (parser, x) => x.HoldTheLinePlanUnpackSoundName = parser.ParseAssetReference() },
            { "HoldTheLinePlanPackSoundName", (parser, x) => x.HoldTheLinePlanPackSoundName = parser.ParseAssetReference() },
            { "HoldTheLineAnnouncementName", (parser, x) => x.HoldTheLineAnnouncementName = parser.ParseAssetReference() },

            { "ValidMemberKindOf", (parser, x) => x.ValidMemberKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "InvalidMemberKindOf", (parser, x) => x.InvalidMemberKindOf = parser.ParseEnumBitArray<ObjectKinds>() },
            { "BattlePlanChangeParalyzeTime", (parser, x) => x.BattlePlanChangeParalyzeTime = parser.ParseInteger() },
            { "HoldTheLinePlanArmorDamageScalar", (parser, x) => x.HoldTheLinePlanArmorDamageScalar = parser.ParseFloat() },
            { "SearchAndDestroyPlanSightRangeScalar", (parser, x) => x.SearchAndDestroyPlanSightRangeScalar = parser.ParseFloat() },

            { "StrategyCenterSearchAndDestroySightRangeScalar", (parser, x) => x.StrategyCenterSearchAndDestroySightRangeScalar = parser.ParseFloat() },
            { "StrategyCenterSearchAndDestroyDetectsStealth", (parser, x) => x.StrategyCenterSearchAndDestroyDetectsStealth = parser.ParseBoolean() },
            { "StrategyCenterHoldTheLineMaxHealthScalar", (parser, x) => x.StrategyCenterHoldTheLineMaxHealthScalar = parser.ParseFloat() },
            { "StrategyCenterHoldTheLineMaxHealthChangeType", (parser, x) => x.StrategyCenterHoldTheLineMaxHealthChangeType = parser.ParseEnum<ChangeType>() },

            { "VisionObjectName", (parser, x) => x.VisionObjectName = parser.ParseAssetReference() }
        };

        public string SpecialPowerTemplate { get; private set; }

        // Transition times
        public int BombardmentPlanAnimationTime { get; private set; }
        public int HoldTheLinePlanAnimationTime { get; private set; }
        public int SearchAndDestroyPlanAnimationTime { get; private set; }
        public int TransitionIdleTime { get; private set; }

        // Messages
        public string BombardmentMessageLabel { get; private set; }
        public string HoldTheLineMessageLabel { get; private set; }
        public string SearchAndDestroyMessageLabel { get; private set; }

        // Sounds
        public string BombardmentPlanUnpackSoundName { get; private set; }
        public string BombardmentPlanPackSoundName { get; private set; }
        public string BombardmentAnnouncementName { get; private set; }
        public string SearchAndDestroyPlanUnpackSoundName { get; private set; }
        public string SearchAndDestroyPlanIdleLoopSoundName { get; private set; }
        public string SearchAndDestroyPlanPackSoundName { get; private set; }
        public string SearchAndDestroyAnnouncementName { get; private set; }
        public string HoldTheLinePlanUnpackSoundName { get; private set; }
        public string HoldTheLinePlanPackSoundName { get; private set; }
        public string HoldTheLineAnnouncementName { get; private set; }

        // Army bonuses
        public BitArray<ObjectKinds> ValidMemberKindOf { get; private set; }
        public BitArray<ObjectKinds> InvalidMemberKindOf { get; private set; }
        public int BattlePlanChangeParalyzeTime { get; private set; }
        public float HoldTheLinePlanArmorDamageScalar { get; private set; }
        public float SearchAndDestroyPlanSightRangeScalar { get; private set; }

        // Building bonuses
        public float StrategyCenterSearchAndDestroySightRangeScalar { get; private set; }
        public bool StrategyCenterSearchAndDestroyDetectsStealth { get; private set; }
        public float StrategyCenterHoldTheLineMaxHealthScalar { get; private set; }
        public ChangeType StrategyCenterHoldTheLineMaxHealthChangeType { get; private set; }

        // Revealing
        public string VisionObjectName { get; private set; }
    }

    public enum ChangeType
    {
        [IniEnum("PRESERVE_RATIO")]
        PreserveRatio
    }
}
