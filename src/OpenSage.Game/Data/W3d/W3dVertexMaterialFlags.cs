using System;

namespace OpenSage.Data.W3d
{
    [Flags]
    public enum W3dVertexMaterialFlags : uint
    {
        None = 0,

        UseDepthCue = 0x00000001,
        ArgbEmissiveOnly = 0x00000002,
        CopySpecularToDiffuse = 0x00000004,
        DepthCueToAlpha = 0x00000008
    }
}
