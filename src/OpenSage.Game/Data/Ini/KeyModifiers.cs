using System;

namespace OpenSage.Data.Ini
{
    [Flags]
    public enum KeyModifiers
    {
        [IniEnum("NONE")]
        None = 0,

        [IniEnum("CTRL")]
        Ctrl = 0x1,

        [IniEnum("SHIFT")]
        Shift = 0x2,

        [IniEnum("ALT")]
        Alt = 0x4,

        [IniEnum("SHIFT_CTRL")]
        ShiftCtrl = Shift | Ctrl,

        [IniEnum("SHIFT_ALT")]
        ShiftAlt = Shift | Alt,

        [IniEnum("SHIFT_ALT_CTRL")]
        ShiftAltCtrl = Shift | Alt | Ctrl
    }
}
