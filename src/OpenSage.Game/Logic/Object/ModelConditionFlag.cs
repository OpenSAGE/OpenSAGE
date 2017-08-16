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
        Door1Opening
    }
}
