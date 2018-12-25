using OpenSage.Data.Ini;
using OpenSage.Data.Ini.Parser;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme2)]
    public sealed class AISpecialPowerUpdateModuleData : AIUpdateModuleData
    {
        internal static new AISpecialPowerUpdateModuleData Parse(IniParser parser) => parser.ParseBlock(FieldParseTable);

        private static new readonly IniParseTable<AISpecialPowerUpdateModuleData> FieldParseTable = AIUpdateModuleData.FieldParseTable
            .Concat(new IniParseTable<AISpecialPowerUpdateModuleData>
            {
                { "CommandButtonName", (parser, x) => x.CommandButtonName = parser.ParseIdentifier() },
                { "SpecialPowerAIType", (parser, x) => x.SpecialPowerAIType = parser.ParseEnum<SpecialPowerAIType>() },
                { "SpecialPowerRadius", (parser, x) => x.SpecialPowerRadius = parser.ParseFloat() },
            });

        public string CommandButtonName { get; private set; }
        public SpecialPowerAIType SpecialPowerAIType { get; private set; }
        public float SpecialPowerRadius { get; private set; }
    }

    public enum SpecialPowerAIType
    {
        [IniEnum("AI_SPECIAL_POWER_CAPTURE_BUILDING")]
        CaptureBuilding,

        [IniEnum("AI_SPECIAL_POWER_TOGGLE_MOUNTED")]
        ToggleMounted,

        [IniEnum("AI_SPECIAL_POWER_BASIC_SELF_BUFF")]
        BasicSelfBuff,

        [IniEnum("AI_SPECIAL_POWER_ENEMY_TYPE_KILLER")]
        EnemyTypeKiller,

        [IniEnum("AI_SPECIAL_POWER_GIVEXP_AOE")]
        GiveXpAoe,

        [IniEnum("AI_SPECIAL_POWER_GANDALF_WIZARD_BLAST")]
        GandalfWizardBlast,

        [IniEnum("AI_SPECIAL_POWER_RANGED_AOE_ATTACK")]
        RangedAoeAttack,

        [IniEnum("AI_SPECIAL_POWER_TOGGLE_SIEGE")]
        ToggleSiege,

        [IniEnum("AI_SPECIAL_POWER_ENEMY_TYPE_KILLER_RANGED")]
        EnemyTypeKillerRanged,

        [IniEnum("AI_SPECIAL_POWER_GOBLINKING_MOUNTED")]
        GoblinkingMounted,

        [IniEnum("AI_SPECIAL_POWER_GOBLINKING_CALLOFTHEDEEP")]
        GoblinkingCallOfTheDeep,

        [IniEnum("AI_SPECIAL_POWER_CHARGE")]
        Charge,

        [IniEnum("AI_SPECIAL_POWER_STANCEBATTLE")]
        StanceBattle,

        [IniEnum("AI_SPECIAL_POWER_STANCEAGGRESSIVE")]
        StanceAggressive,

        [IniEnum("AI_SPECIAL_POWER_STANCEHOLDGROUND")]
        StanceHoldGround,
    }
}
