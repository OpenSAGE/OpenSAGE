using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [Flags]
    public enum ObjectBrowserFlags
    {
        [IniEnum("NONE")]
        None = 0,

        [IniEnum("CINEMATICS")]
        Cinematics      = 0x001,

        [IniEnum("UNIT")]
        Unit            = 0x002,

       
    }
}
