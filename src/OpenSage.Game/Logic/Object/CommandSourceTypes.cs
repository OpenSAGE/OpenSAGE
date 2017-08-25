using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [Flags]
    public enum CommandSourceTypes
    {
        [IniEnum("NONE")]
        None = 0,

        [IniEnum("FROM_PLAYER")]
        FromPlayer = 1 << 0,

        [IniEnum("FROM_AI")]
        FromAI = 1 << 1,

        [IniEnum("FROM_SCRIPT")]
        FromScript = 1 << 2,

        [IniEnum("DEFAULT_SWITCH_WEAPON")]
        DefaultSwitchWeapon = 1 << 3
    }
}
