using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum RadarPriority
    {
        [IniEnum("INVALID")]
        Invalid = 0,

        [IniEnum("LOCAL_UNIT_ONLY")]
        LocalUnitOnly,

        [IniEnum("STRUCTURE")]
        Structure,

        [IniEnum("UNIT")]
        Unit,

        [IniEnum("NOT_ON_RADAR")]
        NotOnRadar
    }
}
