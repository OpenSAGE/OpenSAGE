using OpenSage.Data.Ini;

namespace OpenSage.Input
{
    public enum CommandMapCategory
    {
        [IniEnum("INTERFACE")]
        Interface,

        [IniEnum("TEAM")]
        Team,

        [IniEnum("SELECTION")]
        Selection,

        [IniEnum("CONTROL")]
        Control,

        [IniEnum("MISC")]
        Misc
    }
}
