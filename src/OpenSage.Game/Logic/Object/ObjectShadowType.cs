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

        [IniEnum("SHADOW_VOLUME_NEW")]
        ShadowVolumeNew,

        [IniEnum("SHADOW_ADDITIVE_DECAL"), AddedIn(SageGame.Bfme)]
        ShadowAdditiveDecal,

        [IniEnum("SHADOW_ADDITIVE_DECAL_DYNAMIC"), AddedIn(SageGame.Bfme)]
        ShadowAdditiveDecalDynamic,

        [IniEnum("SHADOW_VOLUME_NON_SELF_3"), AddedIn(SageGame.Bfme)]
        ShadowVolumeNonSelf3,

        [IniEnum("SHADOW_ALPHA_DECAL_DYNAMIC"), AddedIn(SageGame.Bfme2)]
        ShadowAlphaDecalDynamic,

        [IniEnum("SHADOW_ALPHA_DECAL"), AddedIn(SageGame.Bfme2)]
        ShadowAlphaDecal,
    }
}
