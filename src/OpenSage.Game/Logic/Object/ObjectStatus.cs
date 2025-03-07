using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

/// <summary>
/// The explicit numeric values are used for loading Generals .sav files.
/// In Zero Hour and beyond, object status is persisted as a BitArray,
/// and the numeric values are unused.
/// 
/// TODO(Port): the numeric values here are off-by-one compared to the C++ code.
/// This is just an artifact of how we load the values from Generals .sav files,
/// and could be fixed on our side. The actual persisted values are
/// 1 << (these numeric values).
/// </summary>
public enum ObjectStatus
{
    /// <summary>
    /// No status bit.
    /// </summary>
    None = -1,

    /// <summary>
    /// Has been destroyed, pending delete.
    /// </summary>
    [IniEnum("DESTROYED")]
    Destroyed = 0,

    /// <summary>
    /// Used for garrisoned buildings. Is OR'ed with <see cref="ObjectKinds.CanAttack"/>
    /// in isAbleToAttack().
    /// </summary>
    [IniEnum("CAN_ATTACK")]
    CanAttack = 1,

    /// <summary>
    /// Object is being constructed and is not yet complete.
    /// </summary>
    [IniEnum("UNDER_CONSTRUCTION")]
    UnderConstruction = 2,

    /// <summary>
    /// This is a negative condition since these statuses are overrides.
    /// </summary>
    [IniEnum("UNSELECTABLE")]
    Unselectable = 3,

    /// <summary>
    /// Object should be ignored for object-object collisions (but not object-ground).
    /// Used for things like collapsing parachutes that are intangible.
    /// </summary>
    [IniEnum("NO_COLLISIONS")]
    NoCollisions = 4,

    /// <summary>
    /// Absolute override to being able to attack.
    /// </summary>
    NoAttack = 5,

    /// <summary>
    /// In the air as far as anti-air weapons are concerned only.
    /// </summary>
    [IniEnum("AIRBORNE_TARGET")]
    AirborneTarget = 6,

    /// <summary>
    /// Object is on a parachute.
    /// </summary>
    Parachuting = 7,

    /// <summary>
    /// Object repluses <see cref="ObjectKinds.CanBeRepulsed"/> objects.
    /// </summary>
    Repulsor = 8,

    /// <summary>
    /// Unit is in the possession of an enemy criminal. Call the authorities.
    /// </summary>
    [IniEnum("HIJACKED")]
    Hijacked = 9,

    /// <summary>
    /// This object is on fire.
    /// </summary>
    Aflame = 10,

    /// <summary>
    /// This object has already burned as much as it can.
    /// </summary>
    Burned = 11,

    /// <summary>
    /// Object has been soaked with water.
    /// </summary>
    Wet = 12,

    /// <summary>
    /// Object is firing a weapon, now. Not true for special attacks.
    /// </summary>
    [IniEnum("IS_FIRING_WEAPON")]
    IsFiringWeapon = 13,

    /// <summary>
    /// Object is braking, and subverts the physics.
    /// </summary>
    [IniEnum("IS_BRAKING")]
    IsBraking = 14,

    /// <summary>
    /// Object is currently "stealthed".
    /// </summary>
    Stealthed = 15,

    /// <summary>
    /// Object is in range of a stealth-detector unit.
    /// Meaningless if <see cref="Stealthed"/> is not set.
    /// </summary>
    Detected = 16,

    /// <summary>
    /// Object has ability to stealth allowing the stealth update module to run.
    /// </summary>
    CanStealth = 17,

    /// <summary>
    /// Object is being sold.
    /// </summary>
    [IniEnum("SOLD")]
    Sold = 18,

    /// <summary>
    /// Object is awaiting/undergoing a repair order that has been issued.
    /// </summary>
    UndergoingRepair = 19,

    /// <summary>
    /// Reconstructing.
    /// </summary>
    Reconstructing = 20,

    /// <summary>
    /// Masked objects are not selectable and targetable by players or AI.
    /// </summary>
    [IniEnum("MASKED")]
    Masked = 21,

    [IniEnum("INSIDE_GARRISON")]
    InsideGarrison = 21, // Same as Masked?

    /// <summary>
    /// Object is in the general Attack state (including aim, approach, etc).
    /// Note that <see cref="IsFiringWeapon"/> and <see cref="IsAimingWeapon"/>
    /// is a subset of this.
    /// </summary>
    [IniEnum("IS_ATTACKING")]
    IsAttacking = 22,

    /// <summary>
    /// Object is in the process of preparing or firing a special ability.
    /// </summary>
    UsingAbility = 23,

    /// <summary>
    /// Object is aiming a weapon, now. Not true for special attacks.
    /// </summary>
    [IniEnum("IS_AIMING_WEAPON")]
    IsAimingWeapon = 24,

    /// <summary>
    /// Attacking this object may not be done from commandSource == CMD_FROM_AI.
    /// </summary>
    NoAttackFromAI = 25,

    /// <summary>
    /// Temporarily ignoring all stealth bits.
    /// Used only for some special-case mine clearing stuff.
    /// </summary>
    IgnoringStealth = 26,

    /// <summary>
    /// Object is now a car bomb.
    /// </summary>
    IsCarBomb = 27,

    // Everything past here was added in Zero Hour or beyond.
    // So no need for explicit numeric values.

    /// <summary>
    /// Object factors deck height on top of ground altitude.
    /// </summary>
    DeckHeightOffset,

    [IniEnum("STATUS_RIDER1")]
    StatusRider1,

    [IniEnum("STATUS_RIDER2")]
    StatusRider2,

    [IniEnum("STATUS_RIDER3")]
    StatusRider3,

    [IniEnum("STATUS_RIDER4")]
    StatusRider4,

    [IniEnum("STATUS_RIDER5")]
    StatusRider5,

    [IniEnum("STATUS_RIDER6")]
    StatusRider6,

    [IniEnum("STATUS_RIDER7")]
    StatusRider7,

    [IniEnum("STATUS_RIDER8")]
    StatusRider8,

    /// <summary>
    /// Anyone shooting at you shoots faster than normal.
    /// </summary>
    FaerieFire,

    /// <summary>
    /// Object (likely a missile or bomb) is *BUSTING* its way through the *BUNKER*,
    /// building or ground, awaiting death at the bottom.
    /// </summary>
    MissingKillingSelf,

    /// <summary>
    /// We need to know we have a booby trap on us so we can detonate it
    /// from many different code segments.
    /// </summary>
    BoobyTrapped,

    /// <summary>
    /// Do not move!
    /// </summary>
    Immobile,

    /// <summary>
    /// Object is disguised (a type of stealth).
    /// </summary>
    Disguised,

    /// <summary>
    /// Object is deployed.
    /// </summary>
    [IniEnum("DEPLOYED")]
    Deployed,

    [IniEnum("UNATTACKABLE"), AddedIn(SageGame.Bfme)]
    Unattackable,

    [IniEnum("TOPPLED")]
    Toppled,

    [IniEnum("DEATH_1"), AddedIn(SageGame.Bfme)]
    Death1,

    [IniEnum("DEATH_2"), AddedIn(SageGame.Bfme)]
    Death2,

    [IniEnum("DEATH_3"), AddedIn(SageGame.Bfme)]
    Death3,

    [IniEnum("DEATH_4"), AddedIn(SageGame.Bfme)]
    Death4,

    [IniEnum("KILLING_SELF")]
    KillingSelf,

    [IniEnum("BLOODTHIRSTY"), AddedIn(SageGame.Bfme)]
    BloodThirsty,

    [IniEnum("ENCLOSED"), AddedIn(SageGame.Bfme)]
    Enclosed,

    [IniEnum("RIDER1"), AddedIn(SageGame.Bfme)]
    Rider1,

    [IniEnum("RIDER2"), AddedIn(SageGame.Bfme)]
    Rider2,

    [IniEnum("RIDERLESS"), AddedIn(SageGame.Bfme)]
    Riderless,

    [IniEnum("HOLDING_THE_RING"), AddedIn(SageGame.Bfme2)]
    HoldingTheRing,

    [IniEnum("IGNORE_AI_COMMAND"), AddedIn(SageGame.Bfme2Rotwk)]
    IgnoreAICommand,

    [IniEnum("SUMMONING_REPLACEMENT"), AddedIn(SageGame.Bfme2Rotwk)]
    SummoningReplacement,

    [IniEnum("USER_DEFINED_1"), AddedIn(SageGame.Bfme2Rotwk)]
    UserDefined1,

    [IniEnum("NO_HERO_PROPERTIES"), AddedIn(SageGame.Bfme2Rotwk)]
    NoHeroProperties,

    [IniEnum("HOLDING_THE_SHARD"), AddedIn(SageGame.Bfme2Rotwk)]
    HoldingTheShard,
}
