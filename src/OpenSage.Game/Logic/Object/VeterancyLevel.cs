
using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object;

public enum VeterancyLevel
{
    [IniEnum("REGULAR")]
    [IniEnum("FIRST")]
    First_Or_Regular = 0,

    [IniEnum("VETERAN")]
    Veteran = 1,

    [IniEnum("ELITE")]
    Elite = 2,

    [IniEnum("HEROIC")]
    [IniEnum("LAST")]
    Last_Or_Heroic = 3,

    // TODO: Assign a numeric value
    Count,
}
