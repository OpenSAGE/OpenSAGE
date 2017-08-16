using System;

namespace OpenSage.Data.Dds
{
    [Flags]
    public enum DdsCaps2 : uint
    {
        CubeMap = 0x200,
        CubeMapPositiveX = 0x200,
        CubeMapNegativeX = 0x800,
        CubeMapPositiveY = 0x1000,
        CubeMapNegativeY = 0x2000,
        CubeMapPositiveZ = 0x4000,
        CubeMapNegativeZ = 0x8000,
        Volume = 0x200000
    }
}
