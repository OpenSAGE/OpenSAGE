namespace OpenSage.Data.Ini
{
    public enum KeyModifiers
    {
        [IniEnum("NONE")]
        None,

        [IniEnum("CTRL")]
        Ctrl,

        [IniEnum("SHIFT")]
        Shift,

        [IniEnum("ALT")]
        Alt,

        [IniEnum("SHIFT_CTRL")]
        ShiftCtrl,

        [IniEnum("SHIFT_ALT")]
        ShiftAlt,

        [IniEnum("SHIFT_ALT_CTRL")]
        ShiftAltCtrl
    }
}
