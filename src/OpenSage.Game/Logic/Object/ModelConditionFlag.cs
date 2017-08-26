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
    }
}
