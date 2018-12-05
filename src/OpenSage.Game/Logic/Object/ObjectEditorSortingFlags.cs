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
        MiscManMade         = 1 << 0,

        [IniEnum("MISC_NATURAL")]
        MiscNatural         = 1 << 1,

        [IniEnum("STRUCTURE")]
        Structure           = 1 << 2,

        [IniEnum("SYSTEM")]
        System              = 1 << 3,

        [IniEnum("CLEARED_BY_BUILD")]
        ClearedByBuild      = 1 << 4,

        [IniEnum("VEHICLE")]
        Vehicle             = 1 << 5,

        [IniEnum("INFANTRY")]
        Infantry            = 1 << 6,

        [IniEnum("AUDIO")]
        Audio               = 1 << 7,

        [IniEnum("DEBRIS")]
        Debris              = 1 << 8,

        [IniEnum("SHRUBBERY")]
        Shrubbery           = 1 << 9,

        [IniEnum("UNIT")]
        UNIT                = 1 << 10,

        [IniEnum("EMITTERS")]
        EMITTERS            = 1 << 11,

        [IniEnum("OBSOLETE")]
        OBSOLETE            = 1 << 12,

        [IniEnum("SELECTABLE"), AddedIn(SageGame.Bfme2)]
        SELECTABLE          = 1 << 13,
    }
}
