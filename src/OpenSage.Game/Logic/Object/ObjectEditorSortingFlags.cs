using System;
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    [Flags]
    public enum ObjectEditorSortingFlags
    {
        [IniEnum("NONE")]
        None = 0,

        [IniEnum("MISC_MAN_MADE")]
        MiscManMade    = 0x001,

        [IniEnum("MISC_NATURAL")]
        MiscNatural    = 0x002,

        [IniEnum("STRUCTURE")]
        Structure      = 0x004,

        [IniEnum("SYSTEM")]
        System         = 0x008,

        [IniEnum("CLEARED_BY_BUILD")]
        ClearedByBuild = 0x010,

        [IniEnum("VEHICLE")]
        Vehicle        = 0x020,

        [IniEnum("INFANTRY")]
        Infantry       = 0x040,

        [IniEnum("AUDIO")]
        Audio          = 0x080,

        [IniEnum("DEBRIS")]
        Debris         = 0x100,

        [IniEnum("SHRUBBERY")]
        Shrubbery      = 0x200,

        [IniEnum("UNIT")]
        UNIT           = 0x300,
    }
}
