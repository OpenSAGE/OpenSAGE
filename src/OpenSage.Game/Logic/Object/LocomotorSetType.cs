using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

[AddedIn(SageGame.Bfme)]
public enum LocomotorSetType
{
    Invalid = -1,

    [IniEnum("SET_NORMAL")]
    Normal = 0,

    [IniEnum("SET_NORMAL_UPGRADED")]
    NormalUpgraded = 1,

    [IniEnum("SET_FREEFALL")]
    FreeFall = 2,

    [IniEnum("SET_WANDER")]
    Wander = 3,

    [IniEnum("SET_PANIC")]
    Panic = 4,

    /// <summary>
    /// Used for normally-airborne items while taxiing on ground.
    /// </summary>
    [IniEnum("SET_TAXIING")]
    Taxiing = 5,

    /// <summary>
    /// Used for high-speed attacks 
    /// </summary>
    [IniEnum("SET_SUPERSONIC")]
    Supersonic = 6,

    /// <summary>
    /// Used for abnormally slow (but not damaged) speeds
    /// </summary>
    [IniEnum("SET_SLUGGISH")]
    Sluggish = 7,

    [IniEnum("SET_ENRAGED")]
    Enraged,

    [IniEnum("SET_SCARED")]
    Scared,

    [IniEnum("SET_MOUNTED")]
    SetMounted,

    [IniEnum("SET_COMBO")]
    SetCombo,

    [IniEnum("SET_CONTAINED")]
    SetContained,

    [IniEnum("SET_BURNINGDEATH"), AddedIn(SageGame.Bfme2)]
    SetBurningDeath,
}
