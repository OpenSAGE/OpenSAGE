using System;

namespace OpenSage.Data.Dds
{
    [Flags]
    public enum DdsPixelFormatFlags : uint
    {
        AlphaPixels = 0x1,
        Alpha = 0x2,
        FourCc = 0x4,
        Rgb = 0x40,
        Yuv = 0x200,
        Luminance = 0x20000
    }
}
