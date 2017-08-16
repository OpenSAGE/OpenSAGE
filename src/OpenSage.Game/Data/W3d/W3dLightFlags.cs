using System;

namespace OpenSage.Data.W3d
{
    [Flags]
    public enum W3dLightFlags : uint
    {
        Point = 0x00000001,
        Directional = 0x00000002,
        Spot = 0x00000003,

        CastShadows = 0x00000100
    }
}
