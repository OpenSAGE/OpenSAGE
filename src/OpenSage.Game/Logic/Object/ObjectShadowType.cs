using OpenSage.Data.Ini;

namespace OpenSage.Logic.Object
{
    public enum ObjectShadowType
    {
        [IniEnum("NONE")]
        None = 0,

        [IniEnum("SHADOW_VOLUME")]
        ShadowVolume,

        [IniEnum("SHADOW_DECAL")]
        ShadowDecal,

        [IniEnum("SHADOW_VOLUME_NON_SELF_1")]
        ShadowVolumeNonSelf1,

        [IniEnum("SHADOW_VOLUME_NON_SELF_2")]
        ShadowVolumeNonSelf2,
    }
}
