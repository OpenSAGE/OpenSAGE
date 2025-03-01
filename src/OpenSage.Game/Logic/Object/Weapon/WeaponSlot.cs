using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum WeaponSlot
    {
        [IniEnum("PRIMARY")]
        Primary = 0,

        [IniEnum("SECONDARY")]
        Secondary = 1,

        [IniEnum("TERTIARY")]
        Tertiary = 2,

        [IniEnum("QUATERNARY")]
        Quaternary = 3,

        [IniEnum("QUINARY")]
        Quinary = 4,

        NoWeapon,
    }
}
