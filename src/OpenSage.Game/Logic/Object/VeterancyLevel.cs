
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public enum VeterancyLevel
{
    [IniEnum("REGULAR")]
    Regular = 0,

    [IniEnum("VETERAN")]
    Veteran = 1,

    [IniEnum("ELITE")]
    Elite = 2,

    [IniEnum("HEROIC")]
    Heroic = 3,

    Count,

    First = Regular,
    Last = Heroic
}
