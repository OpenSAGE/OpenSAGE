using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum LocomotorSet
    {
        [IniEnum("SET_NORMAL")]
        Normal,

        [IniEnum("SET_WANDER")]
        Wander,

        [IniEnum("SET_PANIC")]
        Panic
    }
}
