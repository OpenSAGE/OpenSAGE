using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [Flags]
    public enum WeaponSetConditions
    {
        [IniEnum("None")]
        None = 0,

        [IniEnum("PLAYER_UPGRADE")]
        PlayerUpgrade = 1 << 0,

        [IniEnum("CARBOMB")]
        CarBomb = 1 << 1
    }
}
