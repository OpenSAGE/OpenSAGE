using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum WeaponSlot
    {
        [IniEnum("PRIMARY")]
        Primary,

        [IniEnum("SECONDARY")]
        Secondary,

        [IniEnum("TERTIARY")]
        Tertiary,

        [IniEnum("QUATERNARY")]
        Quaternary,

        [IniEnum("QUINARY")]
        Quinary,
    }
}
