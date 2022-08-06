using OpenSage.Data.Ini;

namespace OpenSage.Data.Map
{
    public enum TimeOfDay : uint
    {
        [IniEnum("MORNING")]
        Morning = 1,

        [IniEnum("AFTERNOON")]
        Afternoon,

        [IniEnum("EVENING")]
        Evening,

        [IniEnum("NIGHT")]
        Night
    }
}
