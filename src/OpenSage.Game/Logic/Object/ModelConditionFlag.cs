using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum ModelConditionFlag
    {
        [IniEnum("DAMAGED")]
        Damaged,

        [IniEnum("REALLYDAMAGED")]
        ReallyDamaged,

        [IniEnum("RUBBLE")]
        Rubble,

        [IniEnum("SNOW")]
        Snow,

        [IniEnum("NIGHT")]
        Night,

        [IniEnum("GARRISONED")]
        Garrisoned,

        [IniEnum("POST_COLLAPSE")]
        PostCollapse,

        [IniEnum("CAPTURED")]
        Captured,

        [IniEnum("DOOR_1_OPENING")]
        Door1Opening,

        [IniEnum("DOOR_1_WAITING_OPEN")]
        Door1WaitingOpen,

        [IniEnum("DOOR_1_WAITING_TO_CLOSE")]
        Door1WaitingToClose,

        [IniEnum("DOOR_1_CLOSING")]
        Door1Closing,

        [IniEnum("DOOR_2_OPENING")]
        Door2Opening,

        [IniEnum("DOOR_2_WAITING_OPEN")]
        Door2WaitingOpen,

        [IniEnum("DOOR_2_WAITING_TO_CLOSE")]
        Door2WaitingToClose,

        [IniEnum("DOOR_2_CLOSING")]
        Door2Closing,

        [IniEnum("DOOR_3_OPENING")]
        Door3Opening,

        [IniEnum("DOOR_3_WAITING_OPEN")]
        Door3WaitingOpen,

        [IniEnum("DOOR_3_WAITING_TO_CLOSE")]
        Door3WaitingToClose,

        [IniEnum("DOOR_3_CLOSING")]
        Door3Closing,

        [IniEnum("DOOR_4_OPENING")]
        Door4Opening,

        [IniEnum("DOOR_4_WAITING_OPEN")]
        Door4WaitingOpen,

        [IniEnum("DOOR_4_CLOSING")]
        Door4Closing,

        [IniEnum("MOVING")]
        Moving,

        [IniEnum("PANICKING")]
        Panicking,

        [IniEnum("DYING")]
        Dying,

        [IniEnum("EXPLODED_FLAILING")]
        ExplodedFlailing,

        [IniEnum("EXPLODED_BOUNCING")]
        ExplodedBouncing,

        [IniEnum("FRONTCRUSHED")]
        FrontCrushed,

        [IniEnum("BACKCRUSHED")]
        BackCrushed,

        [IniEnum("LOADED")]
        Loaded,

        [IniEnum("OVER_WATER")]
        OverWater,

        [IniEnum("TURRET_ROTATE")]
        TurretRotate,

        [IniEnum("FIRING_A")]
        FiringA,

        [IniEnum("BETWEEN_FIRING_SHOTS_A")]
        BetweenShotsFiringA,

        [IniEnum("RELOADING_A")]
        ReloadingA,

        [IniEnum("PREATTACK_A")]
        PreAttackA,

        [IniEnum("FIRING_B")]
        FiringB,

        [IniEnum("BETWEEN_FIRING_SHOTS_B")]
        BetweenShotsFiringB,

        [IniEnum("RELOADING_B")]
        ReloadingB,

        [IniEnum("PREATTACK_B")]
        PreAttackB,

        [IniEnum("FIRING_C")]
        FiringC,

        [IniEnum("BETWEEN_FIRING_SHOTS_C")]
        BetweenShotsFiringC,

        [IniEnum("RELOADING_C")]
        ReloadingC,

        [IniEnum("PREATTACK_C")]
        PreAttackC,

        [IniEnum("FIRING_PRIMARY")]
        FiringPrimary,

        [IniEnum("FIRING_SECONDARY"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FiringSecondary,

        [IniEnum("FIRING_TERTIARY"), AddedIn(SageGame.CncGeneralsZeroHour)]
        FiringTertiary,

        [IniEnum("USING_WEAPON_A")]
        UsingWeaponA,

        [IniEnum("USING_WEAPON_B")]
        UsingWeaponB,

        [IniEnum("USING_WEAPON_C"), AddedIn(SageGame.CncGeneralsZeroHour)]
        UsingWeaponC,

        [IniEnum("FREEFALL")]
        FreeFall,

        [IniEnum("PARACHUTING")]
        Parachuting,

        [IniEnum("SPLATTED")]
        Splatted,

        [IniEnum("SOLD")]
        Sold,

        [IniEnum("AWAITING_CONSTRUCTION")]
        AwaitingConstruction,

        [IniEnum("PARTIALLY_CONSTRUCTED")]
        PartiallyConstructed,

        [IniEnum("ACTIVELY_BEING_CONSTRUCTED")]
        ActivelyBeingConstructed,

        [IniEnum("ACTIVELY_CONSTRUCTING")]
        ActivelyConstructing,

        [IniEnum("CONSTRUCTION_COMPLETE")]
        ConstructionComplete,

        [IniEnum("PREORDER")]
        Preorder,

        [IniEnum("RADAR_EXTENDING")]
        RadarExtending,

        [IniEnum("RADAR_UPGRADED")]
        RadarUpgraded,

        [IniEnum("POWER_PLANT_UPGRADING")]
        PowerPlantUpgrading,

        [IniEnum("POWER_PLANT_UPGRADED")]
        PowerPlantUpgraded,

        [IniEnum("PACKING")]
        Packing,

        [IniEnum("UNPACKING")]
        Unpacking,

        [IniEnum("DEPLOYED")]
        Deployed,

        [IniEnum("ATTACKING")]
        Attacking,

        [IniEnum("DOCKING_ACTIVE")]
        DockingActive,

        [IniEnum("CONTINUOUS_FIRE_SLOW")]
        ContinuousFireSlow,

        [IniEnum("CONTINUOUS_FIRE_MEAN")]
        ContinuousFireMean,

        [IniEnum("CONTINUOUS_FIRE_FAST")]
        ContinuousFireFast,

        [IniEnum("ENEMYNEAR")]
        EnemyNear,

        [IniEnum("SPECIAL_CHEERING")]
        SpecialCheering,

        [IniEnum("SPECIAL_DAMAGED")]
        SpecialDamaged,

        [IniEnum("CLIMBING")]
        Climbing,

        [IniEnum("RAPPELLING")]
        Rappelling,

        [IniEnum("IS_FIRING_WEAPON")]
        IsFiringWeapon,

        [IniEnum("WEAPONSET_PLAYER_UPGRADE")]
        WeaponSetPlayerUpgrade,

        [IniEnum("WEAPONSET_CRATEUPGRADE_ONE")]
        WeaponSetCrateUpgradeOne,

        [IniEnum("WEAPONSET_CRATEUPGRADE_TWO")]
        WeaponSetCrateUpgradeTwo,

        [IniEnum("JETEXHAUST")]
        JetExhaust,

        [IniEnum("JETAFTERBURNER")]
        JetAfterburner,

        [IniEnum("RAISING_FLAG")]
        RaisingFlag,

        [IniEnum("USING_ABILITY")]
        UsingAbility,

        [IniEnum("CARRYING")]
        Carrying,

        [IniEnum("DOCKING")]
        Docking,

        [IniEnum("AFLAME")]
        Aflame,

        [IniEnum("SMOLDERING")]
        Smoldering,

        [IniEnum("BURNED")]
        Burned,

        [IniEnum("RIDERS_ATTACKING"), AddedIn(SageGame.CncGeneralsZeroHour)]
        RidersAttacking,

        [IniEnum("DISGUISED"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Disguised,

        [IniEnum("JAMMED"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Jammed,

        [IniEnum("DOCKING_BEGINNING"), AddedIn(SageGame.CncGeneralsZeroHour)]
        DockingBeginning,

        [IniEnum("RIDER1"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Rider1,

        [IniEnum("RIDER2"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Rider2,

        [IniEnum("RIDER3"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Rider3,

        [IniEnum("RIDER4"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Rider4,

        [IniEnum("RIDER5"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Rider5,

        [IniEnum("RIDER6"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Rider6,

        [IniEnum("RIDER7"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Rider7,

        [IniEnum("TOPPLED"), AddedIn(SageGame.CncGeneralsZeroHour)]
        Toppled,

        [IniEnum("CENTER_TO_LEFT"), AddedIn(SageGame.CncGeneralsZeroHour)]
        CenterToLeft,

        [IniEnum("LEFT_TO_CENTER"), AddedIn(SageGame.CncGeneralsZeroHour)]
        LeftToCenter,

        [IniEnum("CENTER_TO_RIGHT"), AddedIn(SageGame.CncGeneralsZeroHour)]
        CenterToRight,

        [IniEnum("RIGHT_TO_CENTER"), AddedIn(SageGame.CncGeneralsZeroHour)]
        RightToCenter,

        [IniEnum("USER_1"), AddedIn(SageGame.CncGeneralsZeroHour)]
        User1,

        [IniEnum("USER_2"), AddedIn(SageGame.CncGeneralsZeroHour)]
        User2,

        [IniEnum("SECOND_LIFE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        SecondLife,

        [IniEnum("ARMORSET_CRATEUPGRADE_ONE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        ArmorSetCrateUpgradeOne,

        [IniEnum("ARMORSET_CRATEUPGRADE_TWO"), AddedIn(SageGame.CncGeneralsZeroHour)]
        ArmorSetCrateUpgradeTwo,

        [IniEnum("TAKING_DAMAGE"), AddedIn(SageGame.CncGeneralsZeroHour)]
        TakingDamage,

        [IniEnum("HERO"), AddedIn(SageGame.Bfme)]
        Hero,

        [IniEnum("UPGRADE_ECONOMY_BONUS"), AddedIn(SageGame.Bfme)]
        UpgradeEconomyBonus,

        [IniEnum("WAR_CHANT"), AddedIn(SageGame.Bfme)]
        WarChant,

        [IniEnum("JUST_BUILT"), AddedIn(SageGame.Bfme)]
        JustBuilt,

        [IniEnum("EMOTION_AFRAID"), AddedIn(SageGame.Bfme)]
        EmotionAfraid,

        [IniEnum("WEAPONSET_TOGGLE_1"), AddedIn(SageGame.Bfme)]
        WeaponSetToggle1,

        [IniEnum("MOUNTED"), AddedIn(SageGame.Bfme)]
        Mounted,

        [IniEnum("WEAPONLOCK_SECONDARY"), AddedIn(SageGame.Bfme)]
        WeaponLockSecondary,

        [IniEnum("EMOTION_TAUNTING"), AddedIn(SageGame.Bfme)]
        EmotionTaunting,

        [IniEnum("EMOTION_ALERT"), AddedIn(SageGame.Bfme)]
        EmotionAlert,

        [IniEnum("EMOTION_CELEBRATING"), AddedIn(SageGame.Bfme)]
        EmotionCelebrating,

        [IniEnum("EMOTION_UNCONTROLLABLY_AFRAID"), AddedIn(SageGame.Bfme)]
        EmotionUncontrollablyAfraid,

        [IniEnum("EMOTION_TERROR"), AddedIn(SageGame.Bfme)]
        EmotionTerror,

        [IniEnum("EMOTION_POINTING"), AddedIn(SageGame.Bfme)]
        EmotionPointing,

        [IniEnum("EMOTION_DOOM"), AddedIn(SageGame.Bfme)]
        EmotionDoom,

        [IniEnum("WADING"), AddedIn(SageGame.Bfme)]
        Wading,

        [IniEnum("ENGAGED"), AddedIn(SageGame.Bfme)]
        Engaged,

        [IniEnum("STUNNED"), AddedIn(SageGame.Bfme)]
        Stunned,

        [IniEnum("STUNNED_STANDING_UP"), AddedIn(SageGame.Bfme)]
        StunnedStandingUp,

        [IniEnum("STUNNED_FLAILING"), AddedIn(SageGame.Bfme)]
        StunnedFlailing,

        [IniEnum("FIRING_OR_PREATTACK_A"), AddedIn(SageGame.Bfme)]
        FiringOrPreAttackA,

        [IniEnum("THROWN_PROJECTILE"), AddedIn(SageGame.Bfme)]
        ThrownProjectile,

        [IniEnum("PASSENGER"), AddedIn(SageGame.Bfme)]
        Passenger,

        [IniEnum("GUARDING"), AddedIn(SageGame.Bfme)]
        Guarding,

        [IniEnum("EMOTION_QUARRELSOME"), AddedIn(SageGame.Bfme)]
        EmotionQuarrelsome,

        [IniEnum("DECELERATE"), AddedIn(SageGame.Bfme)]
        Decelerate,

        [IniEnum("ACCELERATE"), AddedIn(SageGame.Bfme)]
        Accelerate,

        [IniEnum("TURN_LEFT"), AddedIn(SageGame.Bfme)]
        TurnLeft,

        [IniEnum("TURN_RIGHT"), AddedIn(SageGame.Bfme)]
        TurnRight,

        [IniEnum("BACKING_UP"), AddedIn(SageGame.Bfme)]
        BackingUp,

        [IniEnum("CHANT_FOR_GROND"), AddedIn(SageGame.Bfme)]
        ChantForGrond,

        [IniEnum("WANDER"), AddedIn(SageGame.Bfme)]
        Wander,

        [IniEnum("HARVEST_PREPARATION"), AddedIn(SageGame.Bfme)]
        HarvestPrepariation,

        [IniEnum("HARVEST_ACTION"), AddedIn(SageGame.Bfme)]
        HarvestAction,

        [IniEnum("SPECIAL_ENEMY_NEAR"), AddedIn(SageGame.Bfme)]
        SpecialEnemyNear,

        [IniEnum("DEATH_1"), AddedIn(SageGame.Bfme)]
        Death1,

        [IniEnum("DEATH_2"), AddedIn(SageGame.Bfme)]
        Death2,

        [IniEnum("EMOTION_MORALE_HIGH"), AddedIn(SageGame.Bfme)]
        EmotionMoraleHigh,

        [IniEnum("EMOTION_MORALE_LOW"), AddedIn(SageGame.Bfme)]
        EmotionMoraleLow,

        [IniEnum("HIT_REACTION"), AddedIn(SageGame.Bfme)]
        HitReaction,

        [IniEnum("HIT_LEVEL_1"), AddedIn(SageGame.Bfme)]
        HitLevel1,

        [IniEnum("SELECTED"), AddedIn(SageGame.Bfme)]
        Selected,

        [IniEnum("WEAPONLOCK_PRIMARY"), AddedIn(SageGame.Bfme)]
        WeaponLockPrimary,

        [IniEnum("FIRING_OR_PREATTACK_B"), AddedIn(SageGame.Bfme)]
        FiringOrPreattackB,

        [IniEnum("PACKING_TYPE_1"), AddedIn(SageGame.Bfme)]
        PackingType1,

        [IniEnum("PACKING_TYPE_2"), AddedIn(SageGame.Bfme)]
        PackingType2,

        [IniEnum("PREPARING"), AddedIn(SageGame.Bfme)]
        Preparing,

        [IniEnum("HIDDEN"), AddedIn(SageGame.Bfme)]
        Hidden,

        [IniEnum("BURNT_MODEL"), AddedIn(SageGame.Bfme)]
        BurntModel,

        [IniEnum("WEAPONSTATE_ONE"), AddedIn(SageGame.Bfme)]
        WeaponstateOne,

        [IniEnum("WEAPONSTATE_TWO"), AddedIn(SageGame.Bfme)]
        WeaponstateTwo,

        [IniEnum("FIRING_OR_PREATTACK_C"), AddedIn(SageGame.Bfme)]
        FiringOrPreattackC,

        [IniEnum("PASSENGER_VARIATION_1"), AddedIn(SageGame.Bfme)]
        PassengerVariation1,

        [IniEnum("PASSENGER_VARIATION_2"), AddedIn(SageGame.Bfme)]
        PassengerVariation2,

        [IniEnum("DESTROYED_WEAPON"), AddedIn(SageGame.Bfme)]
        DestroyedWeapon,

        [IniEnum("HIT_LEVEL_2"), AddedIn(SageGame.Bfme)]
        HitLevel2,

        [IniEnum("HIT_LEVEL_3"), AddedIn(SageGame.Bfme)]
        HitLevel3,

        [IniEnum("REACT_1"), AddedIn(SageGame.Bfme)]
        React1,

        [IniEnum("REACT_2"), AddedIn(SageGame.Bfme)]
        React2,

        [IniEnum("REACT_3"), AddedIn(SageGame.Bfme)]
        React3,

        [IniEnum("REACT_4"), AddedIn(SageGame.Bfme)]
        React4,

        [IniEnum("REACT_5"), AddedIn(SageGame.Bfme)]
        React5,

        [IniEnum("WEAPONSTATE_CLOSE_RANGE"), AddedIn(SageGame.Bfme)]
        WeaponstateCloseRange,

        [IniEnum("AIM_HIGH"), AddedIn(SageGame.Bfme)]
        AimHigh,

        [IniEnum("WORLD_BUILDER"), AddedIn(SageGame.Bfme)]
        WorldBuilder,

        [IniEnum("DEATH_3"), AddedIn(SageGame.Bfme)]
        Death3,
        
        [IniEnum("DEATH_4"), AddedIn(SageGame.Bfme)]
        Death4,

        [IniEnum("WALKING"), AddedIn(SageGame.Bfme)]
        Walking,

        [IniEnum("EMOTION_LOOK_TO_SKY"), AddedIn(SageGame.Bfme)]
        EmotionLookToSky,

        [IniEnum("USER_3"), AddedIn(SageGame.Bfme)]
        User3,

        [IniEnum("SIEGE_CONTAIN"), AddedIn(SageGame.Bfme)]
        SiegeContain,

        [IniEnum("USING_SPECIAL_ABILITY"), AddedIn(SageGame.Bfme)]
        UsingSpecialAbility,

        [IniEnum("AIM_LOW"), AddedIn(SageGame.Bfme)]
        AimLow,

        [IniEnum("RUNNING_OFF_MAP"), AddedIn(SageGame.Bfme)]
        RunningOffMap,

        [IniEnum("TURN_LEFT_HIGH_SPEED"), AddedIn(SageGame.Bfme)]
        TurnLeftHighSpeed,

        [IniEnum("TURN_RIGHT_HIGH_SPEED"), AddedIn(SageGame.Bfme)]
        TurnRightHighSpeed,

        [IniEnum("WEAPONSET_RAMPAGE"), AddedIn(SageGame.Bfme)]
        WeaponsetRampage,

        [IniEnum("CHARGING"), AddedIn(SageGame.Bfme)]
        Charging,

        [IniEnum("DECAY"), AddedIn(SageGame.Bfme)]
        Decay,

        [IniEnum("CLUB"), AddedIn(SageGame.Bfme)]
        Club,

        [IniEnum("POST_RUBBLE"), AddedIn(SageGame.Bfme)]
        PostRubble,

        [IniEnum("USER_4"), AddedIn(SageGame.Bfme)]
        User4,

        [IniEnum("USER_5"), AddedIn(SageGame.Bfme)]
        User5,

        [IniEnum("AWAY_FROM_TREES"), AddedIn(SageGame.Bfme)]
        AwayFromTrees,

        [IniEnum("DESTROYED_FRONT"), AddedIn(SageGame.Bfme)]
        DestroyedFront,

        [IniEnum("DESTROYED_RIGHT"), AddedIn(SageGame.Bfme)]
        DestroyedRight,

        [IniEnum("DESTROYED_LEFT"), AddedIn(SageGame.Bfme)]
        DestroyedLeft,

        [IniEnum("DESTROYED_BACK"), AddedIn(SageGame.Bfme)]
        DestroyedBack,

        [IniEnum("EMOTION_COWER"), AddedIn(SageGame.Bfme)]
        EmotionCower,

        [IniEnum("HORDE_EMPTY"), AddedIn(SageGame.Bfme)]
        HordeEmpty,

        [IniEnum("BASE_BUILD"), AddedIn(SageGame.Bfme)]
        BaseBuild,

        [IniEnum("UPGRADE_NUMENOR_STONEWORK"), AddedIn(SageGame.Bfme)]
        UpgradeNumenorStonework,

        [IniEnum("UPGRADE_TREBUCHET"), AddedIn(SageGame.Bfme)]
        UpgradeTrebuchet,

        [IniEnum("UPGRADE_POSTERN_GATE"), AddedIn(SageGame.Bfme)]
        UpgradePosternGate,

        [IniEnum("UPGRADE_GARRISON"), AddedIn(SageGame.Bfme)]
        UpgradeGarrison,

        [IniEnum("LEVELED"), AddedIn(SageGame.Bfme)]
        Leveled,

        [IniEnum("SPECIAL_POWER_1"), AddedIn(SageGame.Bfme)]
        SpecialPower1,

        [IniEnum("COLLAPSING"), AddedIn(SageGame.Bfme)]
        Collapsing,

        [IniEnum("TAINT"), AddedIn(SageGame.Bfme)]
        Taint,

        [IniEnum("TRANSPORT_MOVING"), AddedIn(SageGame.Bfme)]
        TransportMoving,

        [IniEnum("WEAPON_TOGGLING"), AddedIn(SageGame.Bfme)]
        WeaponToggling,

        [IniEnum("FIRING_OR_RELOADING_A"), AddedIn(SageGame.Bfme)]
        FiringOrReloadingA,

        [IniEnum("UNCONTROLLABLE"), AddedIn(SageGame.Bfme)]
        Uncontrollable,

        [IniEnum("COMING_OUT_OF_FACTORY"), AddedIn(SageGame.Bfme)]
        ComingOutOfFactory,

        [IniEnum("ALTERNATE_FORMATION"), AddedIn(SageGame.Bfme)]
        AlternateFormation,

        [IniEnum("USING_COMBO_LOCOMOTOR"), AddedIn(SageGame.Bfme)]
        UsingComboLocomotor,

        [IniEnum("SPECIAL_WEAPON_ONE"), AddedIn(SageGame.Bfme)]
        SpecialWeaponOne,

        [IniEnum("SPECIAL_WEAPON_TWO"), AddedIn(SageGame.Bfme)]
        SpecialWeaponTwo,

        [IniEnum("WEAPON_TOGGLE"), AddedIn(SageGame.Bfme)]
        WeaponToggle,

        [IniEnum("EATING"), AddedIn(SageGame.Bfme)]
        Eating,

        [IniEnum("QUARRELSOME_FIGHTING"), AddedIn(SageGame.Bfme)]
        QuarrelsomeFighting,

        [IniEnum("WEAPONSTATE_CONTAINED"), AddedIn(SageGame.Bfme)]
        WeaponstateContained,

        [IniEnum("DIVING"), AddedIn(SageGame.Bfme)]
        Diving,

        [IniEnum("ABOUT_TO_HIT"), AddedIn(SageGame.Bfme)]
        AboutToHit,

        [IniEnum("PASSENGER_VARIATION_3"), AddedIn(SageGame.Bfme)]
        PassengerVariation3,

        [IniEnum("PASSENGER_VARIATION_4"), AddedIn(SageGame.Bfme)]
        PassengerVariation4,

        [IniEnum("DESTROYED_WHILST_BEING_CONSTRUCTED"), AddedIn(SageGame.Bfme)]
        DestroyedWhilstBeingConstructed,

        [IniEnum("ELVEN_WOOD"), AddedIn(SageGame.Bfme)]
        ElvenWood,

        [IniEnum("HORDEBRAIN_NOT_STEALTHED"), AddedIn(SageGame.Bfme)]
        HordebrainNotStealthed,

        [IniEnum("AIM_NEAR"), AddedIn(SageGame.Bfme)]
        AimNear,

        [IniEnum("WEAPONSET_ENRAGED"), AddedIn(SageGame.Bfme)]
        WeaponsetEnraged,

        [IniEnum("INITIAL_ENRAGED"), AddedIn(SageGame.Bfme)]
        InitialEnraged,

        [IniEnum("GRAB_BUILDING_CHUNK"), AddedIn(SageGame.Bfme)]
        GrabBuildingChunk,

        [IniEnum("WEAPONSET_HERO_MODE"), AddedIn(SageGame.Bfme)]
        WeaponsetHeroMode,

        [IniEnum("ATTACKING_STRUCTURE"), AddedIn(SageGame.Bfme)]
        AttackingStructure,

        [IniEnum("SPECIAL_WEAPON_THREE"), AddedIn(SageGame.Bfme)]
        SpecialWeaponThree,

        [IniEnum("EMOTION_GUNG_HO"), AddedIn(SageGame.Bfme)]
        EmotionGungHo,

        [IniEnum("RESURRECTED"), AddedIn(SageGame.Bfme)]
        Resurrected,

        [IniEnum("ONE_RING"), AddedIn(SageGame.Bfme)]
        OneRing,

        [IniEnum("PUTTING_ON_RING"), AddedIn(SageGame.Bfme)]
        PuttingOnRing,

        [IniEnum("TAKING_OFF_RING"), AddedIn(SageGame.Bfme)]
        TakingOffRing,

        [IniEnum("TURRET_ANGLE_90"), AddedIn(SageGame.Bfme)]
        TurretAngle90,

        [IniEnum("TURRET_ANGLE_180"), AddedIn(SageGame.Bfme)]
        TurretAngle180,

        [IniEnum("TURRET_ANGLE_270"), AddedIn(SageGame.Bfme)]
        TurretAngle270,

        [IniEnum("DEBUG"), AddedIn(SageGame.Bfme)]
        Debug,

        [IniEnum("PACKING_TYPE_3"), AddedIn(SageGame.Bfme)]
        PackingType3,

        [IniEnum("OATH_FULLFILLED"), AddedIn(SageGame.Bfme)]
        OathFullfilled,

        [IniEnum("EMOTION_AMUSED"), AddedIn(SageGame.Bfme2)]
        EmotionAmused,

        [IniEnum("BUILD_VARIATION_ONE"), AddedIn(SageGame.Bfme2)]
        BuildVariationOne,

        [IniEnum("BUILD_VARIATION_TWO"), AddedIn(SageGame.Bfme2)]
        BuildVariationTwo,

        [IniEnum("BUILD_PLACEMENT_CURSOR"), AddedIn(SageGame.Bfme2)]
        BuildPlacementCursor,

        [IniEnum("PHANTOM_STRUCTURE"), AddedIn(SageGame.Bfme2)]
        PhantomStructure,

        [IniEnum("CREATE_A_HERO_00"), AddedIn(SageGame.Bfme2)]
        CreateAHero00,

        [IniEnum("CREATE_A_HERO_01"), AddedIn(SageGame.Bfme2)]
        CreateAHero01,

        [IniEnum("CREATE_A_HERO_02"), AddedIn(SageGame.Bfme2)]
        CreateAHero02,

        [IniEnum("CREATE_A_HERO_03"), AddedIn(SageGame.Bfme2)]
        CreateAHero03,

        [IniEnum("CREATE_A_HERO_04"), AddedIn(SageGame.Bfme2)]
        CreateAHero04,

        [IniEnum("CREATE_A_HERO_05"), AddedIn(SageGame.Bfme2)]
        CreateAHero05,

        [IniEnum("CREATE_A_HERO_06"), AddedIn(SageGame.Bfme2)]
        CreateAHero06,

        [IniEnum("CREATE_A_HERO_07"), AddedIn(SageGame.Bfme2)]
        CreateAHero07,

        [IniEnum("CREATE_A_HERO_08"), AddedIn(SageGame.Bfme2)]
        CreateAHero08,

        [IniEnum("CREATE_A_HERO_09"), AddedIn(SageGame.Bfme2)]
        CreateAHero09,

        [IniEnum("CREATE_A_HERO_10"), AddedIn(SageGame.Bfme2)]
        CreateAHero10,

        [IniEnum("CREATE_A_HERO_11"), AddedIn(SageGame.Bfme2)]
        CreateAHero11,

        [IniEnum("CREATE_A_HERO_12"), AddedIn(SageGame.Bfme2)]
        CreateAHero12,

        [IniEnum("CREATE_A_HERO_13"), AddedIn(SageGame.Bfme2)]
        CreateAHero13,

        [IniEnum("CREATE_A_HERO_14"), AddedIn(SageGame.Bfme2)]
        CreateAHero14,

        [IniEnum("CREATE_A_HERO_15"), AddedIn(SageGame.Bfme2)]
        CreateAHero15,

        [IniEnum("CREATE_A_HERO_16"), AddedIn(SageGame.Bfme2)]
        CreateAHero16,

        [IniEnum("CREATE_A_HERO_17"), AddedIn(SageGame.Bfme2)]
        CreateAHero17,

        [IniEnum("CREATE_A_HERO_18"), AddedIn(SageGame.Bfme2)]
        CreateAHero18,

        [IniEnum("CREATE_A_HERO_19"), AddedIn(SageGame.Bfme2)]
        CreateAHero19,

        [IniEnum("CREATE_A_HERO_20"), AddedIn(SageGame.Bfme2)]
        CreateAHero20,

        [IniEnum("CREATE_A_HERO_21"), AddedIn(SageGame.Bfme2)]
        CreateAHero21,

        [IniEnum("CREATE_A_HERO_22"), AddedIn(SageGame.Bfme2)]
        CreateAHero22,

        [IniEnum("CREATE_A_HERO_23"), AddedIn(SageGame.Bfme2)]
        CreateAHero23,

        [IniEnum("CREATE_A_HERO_24"), AddedIn(SageGame.Bfme2)]
        CreateAHero24,

        [IniEnum("CREATE_A_HERO_25"), AddedIn(SageGame.Bfme2)]
        CreateAHero25,

        [IniEnum("CREATE_A_HERO_26"), AddedIn(SageGame.Bfme2)]
        CreateAHero26,

        [IniEnum("CREATE_A_HERO_27"), AddedIn(SageGame.Bfme2)]
        CreateAHero27,

        [IniEnum("INVISIBLE_STEALTH"), AddedIn(SageGame.Bfme2)]
        InvisibleStealth,

        [IniEnum("CREATE_A_HERO_IN_CREATION_SCREEN"), AddedIn(SageGame.Bfme2)]
        CreateAHeroInCreationScreen,

        [IniEnum("CREATE_A_HERO_EXAMINE_SELF"), AddedIn(SageGame.Bfme2)]
        CreateAHeroExamineSelf,

        [IniEnum("CREATE_A_HERO_SELECTED_CHEER"), AddedIn(SageGame.Bfme2)]
        CreateAHeroSelectedCheer,

        [IniEnum("CREATE_A_HERO_EXAMINE_WEAPON_RIGHT"), AddedIn(SageGame.Bfme2)]
        CreateAHeroExamineWeaponRight,

        [IniEnum("USER_75"), AddedIn(SageGame.Bfme2)]
        User75,

        [IniEnum("SPECIAL_WEAPON_FOUR"), AddedIn(SageGame.Bfme2)]
        SpecialWeaponFour,

        [IniEnum("SPECIAL_WEAPON_FIVE"), AddedIn(SageGame.Bfme2)]
        SpecialWeaponFive,

        [IniEnum("SPECIAL_WEAPON_SIX"), AddedIn(SageGame.Bfme2)]
        SpecialWeaponSix,

        [IniEnum("PACKING_TYPE_4"), AddedIn(SageGame.Bfme2)]
        PackingType4,

        [IniEnum("PACKING_TYPE_5"), AddedIn(SageGame.Bfme2)]
        PackingType5,

        [IniEnum("PACKING_TYPE_6"), AddedIn(SageGame.Bfme2)]
        PackingType6,

        [IniEnum("CREATE_A_HERO_65"), AddedIn(SageGame.Bfme2)]
        CreateAHero65,

        [IniEnum("FIRING_ANY"), AddedIn(SageGame.Bfme2)]
        FiringAny,

        [IniEnum("FORTRESS_IMPROVEMENT_1"), AddedIn(SageGame.Bfme2)]
        FortressImprovement1,

        [IniEnum("FORTRESS_IMPROVEMENT_2"), AddedIn(SageGame.Bfme2)]
        FortressImprovement2,

        [IniEnum("FORTRESS_IMPROVEMENT_3"), AddedIn(SageGame.Bfme2)]
        FortressImprovement3,

        [IniEnum("FORTRESS_IMPROVEMENT_4"), AddedIn(SageGame.Bfme2)]
        FortressImprovement4,

        [IniEnum("UPGRADE_FORTRESS_MONUMENT"), AddedIn(SageGame.Bfme2)]
        UpgradeFortressMonument,

        [IniEnum("LEASHED_RETURNING"), AddedIn(SageGame.Bfme2)]
        LeashedReturning,

        [IniEnum("FORTRESS_MONUMENT_CREATURE_AVAILABLE"), AddedIn(SageGame.Bfme2)]
        FortressMonumentCreatureAvailable,

        [IniEnum("FORTRESS_MONUMENT_CREATURE_UNAVAILABLE"), AddedIn(SageGame.Bfme2)]
        FortressMonumentCreatureUnavailable,

        [IniEnum("SWIMMING"), AddedIn(SageGame.Bfme2)]
        Swimming,

        [IniEnum("BURNINGDEATH"), AddedIn(SageGame.Bfme2)]
        BurningDeath,

        [IniEnum("ARMORSET_PLAYER_UPGRADE"), AddedIn(SageGame.Bfme2)]
        ArmorsetPlayerUpgrade,

        [IniEnum("SAIL_BLOWN_LEFT"), AddedIn(SageGame.Bfme2)]
        SailBlownLeft,

        [IniEnum("SAIL_BLOWN_Right"), AddedIn(SageGame.Bfme2)]
        SailBlownRight,

        [IniEnum("ATTACKING_POSITION"), AddedIn(SageGame.Bfme2)]
        AttackingPosition,

        [IniEnum("PARALYZED"), AddedIn(SageGame.Bfme2)]
        Paralyzed,

        [IniEnum("PORCUPINE"), AddedIn(SageGame.Bfme2)]
        Porcupine,

        [IniEnum("FIRING_OR_RELOADING_C"), AddedIn(SageGame.Bfme2)]
        FiringOrReloadingC,

        [IniEnum("STONED"), AddedIn(SageGame.Bfme2)]
        Stoned,

        [IniEnum("FIRING_OR_PREATTACK_E"), AddedIn(SageGame.Bfme2)]
        FiringOrPreattackE,

        [IniEnum("SWAPPING_TO_WEAPONSET_1"), AddedIn(SageGame.Bfme2)]
        SwappingToWeaponset1,

        [IniEnum("PREATTACK_E"), AddedIn(SageGame.Bfme2)]
        PreattackE,

        [IniEnum("FIRING_OR_PREATTACK_D"), AddedIn(SageGame.Bfme2)]
        FiringOrPreattackD,
    }
}
