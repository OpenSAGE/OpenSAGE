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
    }
}
