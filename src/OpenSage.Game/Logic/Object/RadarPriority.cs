using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum RadarPriority
    {
        [IniEnum("INVALID")]
        Invalid = 0,

        [IniEnum("NOT_ON_RADAR")]
        NotOnRadar,

        [IniEnum("STRUCTURE")]
        Structure,

        [IniEnum("UNIT")]
        Unit,

        [IniEnum("LOCAL_UNIT_ONLY")]
        LocalUnitOnly,
    }
}
