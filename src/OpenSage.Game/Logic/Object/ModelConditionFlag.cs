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

        [IniEnum("DOOR_1_CLOSING")]
        Door1Closing,

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

        [IniEnum("FREEFALL")]
        Freefall,

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

        [IniEnum("PREORDER")]
        Preorder
    }
}
