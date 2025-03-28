
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public enum VeterancyLevel
{
    [IniEnum("REGULAR")]
    Regular,

    [IniEnum("VETERAN")]
    Veteran,

    [IniEnum("ELITE")]
    Elite,

    [IniEnum("HEROIC")]
    Heroic,

    Count,

    First = Regular,
    Last = Heroic
}
