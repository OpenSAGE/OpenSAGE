using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [AddedIn(SageGame.Bfme)]
    public enum LocomotorSetType
    {
        [IniEnum("SET_NORMAL")]
        Normal,

        [IniEnum("SET_NORMAL_UPGRADED")]
        NormalUpgraded,

        [IniEnum("SET_WANDER")]
        Wander,

        [IniEnum("SET_PANIC")]
        Panic,

        [IniEnum("SET_FREEFALL")]
        FreeFall,

        [IniEnum("SET_TAXIING")]
        Taxiing,

        [IniEnum("SET_SUPERSONIC")]
        Supersonic,

        [IniEnum("SET_SLUGGISH")]
        Sluggish,

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
}
