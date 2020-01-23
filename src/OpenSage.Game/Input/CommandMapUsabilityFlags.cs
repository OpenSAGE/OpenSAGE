using System;
using OpenSage.Data.Ini;

namespace OpenSage.Input
{
    [Flags]
    public enum CommandMapUsabilityFlags
    {
        [IniEnum("NONE")]
        None  = 0,

        [IniEnum("GAME")]
        Game  = 1,

        [IniEnum("SHELL")]
        Shell = 2
    }
}
