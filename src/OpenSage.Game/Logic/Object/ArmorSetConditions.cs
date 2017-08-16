using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [Flags]
    public enum ArmorSetConditions
    {
        [IniEnum("None")]
        None = 0,

        [IniEnum("PLAYER_UPGRADE")]
        PlayerUpgrade,
    }
}
