using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum ObjectStatus
    {
        [IniEnum("UNDER_CONSTRUCTION")]
        UnderConstruction,

        [IniEnum("HIJACKED")]
        Hijacked,

        [IniEnum("STATUS_RIDER1")]
        Rider1,

        [IniEnum("STATUS_RIDER2")]
        Rider2,

        [IniEnum("STATUS_RIDER3")]
        Rider3,

        [IniEnum("STATUS_RIDER4")]
        Rider4,

        [IniEnum("STATUS_RIDER5")]
        Rider5,

        [IniEnum("STATUS_RIDER6")]
        Rider6,

        [IniEnum("STATUS_RIDER7")]
        Rider7,

        [IniEnum("TOPPLED")]
        Toppled,
    }
}
