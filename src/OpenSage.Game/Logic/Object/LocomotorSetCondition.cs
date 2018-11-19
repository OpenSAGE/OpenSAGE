using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum LocomotorSetCondition
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
        Enraged
    }
}
