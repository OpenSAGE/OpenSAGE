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
                { "SpecialPowerRange", (parser, x) => x.SpecialPowerRange = parser.ParseInteger() },
                { "SpellMakesAStructure", (parser, x) => x.SpellMakesAStructure = parser.ParseBoolean() },
                { "RandomizeTargetLocation", (parser, x) => x.RandomizeTargetLocation = parser.ParseBoolean() }
            });

        public string CommandButtonName { get; private set; }
        public SpecialPowerAIType SpecialPowerAIType { get; private set; }
        public float SpecialPowerRadius { get; private set; }
        public int SpecialPowerRange { get; private set; }
        public bool SpellMakesAStructure { get; private set; }
        public bool RandomizeTargetLocation { get; private set; }
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

        [IniEnum("AI_SPECIAL_POWER_TARGETAOE_SUMMON")]
        TargetAoeSummon,

        [IniEnum("AI_SPECIAL_POWER_ENEMY_TYPE_KILLER_STRUCTURES")]
        EnemyTypeKillerStructures,

        [IniEnum("AI_SPECIAL_POWER_SELFAOEHEALHEROS")]
        SelfAoeHealHeros,

        [IniEnum("AI_SPELLBOOK_SHROUD_REVEAL")]
        ShroudReveal,

        [IniEnum("AI_SPECIAL_POWER_HEAL_AOE")]
        HealAoe,

        [IniEnum("AI_SPECIAL_POWER_LEGOLAS_ARROWWIND")]
        LegolasArrowWind,

        [IniEnum("AI_SPECIAL_POWER_LEGOLAS_TRAINARCHERS")]
        LegolasTrainArchers,

        [IniEnum("AI_SPECIAL_POWER_ELENDIL")]
        Elendil,

        [IniEnum("AI_SPELLBOOK_ASSIST_BATTLE_BUFF")]
        AssistBattleBuff,

        [IniEnum("AI_SPELLBOOK_ASSIST_BATTLE_DEBUFF")]
        AssistBattleDebuff,

        [IniEnum("AI_SPELLBOOK_STRUCTURE_BASEKILL")]
        StructureBasekill,

        [IniEnum("AI_SPELLBOOK_STRUCTURE_BREAKER")]
        StructureBreaker,

        [IniEnum("AI_SPELLBOOK_STRUCTURE_BREAKER_PREF_WALLS")]
        StructureBreakerPrefWalls,

        [IniEnum("AI_SPELLBOOK_ALWAYS_FIRE")]
        AlwaysFire,

        [IniEnum("AI_SPELLBOOK_CAPTURE_CREEP")]
        CaptureCreep,

        [IniEnum("AI_SPELLBOOK_TREE_KILLER")]
        TreeKiller,

        [IniEnum("AI_SPELLBOOK_BUFFTERRAIN")]
        Buffterrain,

        [IniEnum("AI_SPELLBOOK_REBUILD")]
        Rebuild,

        [IniEnum("AI_SPELLBOOK_BUFFECONOMYBUILDING")]
        BuffEconomyBuilding,

        [IniEnum("AI_SPELLBOOK_CALLTHEHORDE")]
        CallTheHorde,

        [IniEnum("AI_SPELLBOOK_HEAL")]
        Heal,

        [IniEnum("AI_SPELLBOOK_ENSHROUDINGMIST")]
        Enshroudingmist,

        [IniEnum("AI_SPELLBOOK_ARMY_BREAKER")]
        ArmyBreaker,

        [IniEnum("AI_SPELLBOOK_CITADEL")]
        Citadel,

        [IniEnum("AI_SPECIAL_POWER_DOMINATE_ENEMY"), AddedIn(SageGame.Bfme2Rotwk)]
        DominateEnemy,

        [IniEnum("AI_SPECIAL_POWER_TOGGLE_MELEE_AND_RANGE"), AddedIn(SageGame.Bfme2Rotwk)]
        ToggleMeleeAndRange,

        [IniEnum("AI_SPECIAL_POWER_RANGED_AOE_ATTACK_UNITS"), AddedIn(SageGame.Bfme2Rotwk)]
        RangedAoeAttackUnits,

        [IniEnum("AI_SPECIAL_POWER_SOUL_FREEZE"), AddedIn(SageGame.Bfme2Rotwk)]
        SoulFreeze,

        [IniEnum("AI_SPECIAL_POWER_AOE_AND_BUFF"), AddedIn(SageGame.Bfme2Rotwk)]
        AoeAndBuff,

        [IniEnum("AI_SPECIAL_POWER_ATTACK_HEAL_AOE"), AddedIn(SageGame.Bfme2Rotwk)]
        AttackHealAoe,

        [IniEnum("AI_SPECIAL_POWER_MORGUL_BLADE"), AddedIn(SageGame.Bfme2Rotwk)]
        MorgulBlade,

        [IniEnum("AI_SPECIAL_POWER_GOBLIN_POISON"), AddedIn(SageGame.Bfme2Rotwk)]
        GoblinPoison,

        [IniEnum("AI_SPECIAL_POWER_DOMINATE_TROLL"), AddedIn(SageGame.Bfme2Rotwk)]
        DominateTroll,

        [IniEnum("AI_SPECIAL_POWER_TAME_THE_BEAST"), AddedIn(SageGame.Bfme2Rotwk)]
        TameTheBeast,

        [IniEnum("AI_SPECIAL_POWER_BASIC_SELF_DEBUFF"), AddedIn(SageGame.Bfme2Rotwk)]
        BasicSelfDebuff,

        [IniEnum("AI_SPELLBOOK_DEBUFFECONOMYBUILDING"), AddedIn(SageGame.Bfme2Rotwk)]
        DebuffEconomyBuilding,

        [IniEnum("AI_SPELLBOOK_DEBUFFPRODUCTIONBUILDING"), AddedIn(SageGame.Bfme2Rotwk)]
        DebuffProductionBuilding,
    }
}
