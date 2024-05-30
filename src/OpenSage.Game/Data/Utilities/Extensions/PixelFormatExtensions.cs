using Veldrid;

namespace OpenSage.Data.Utilities.Extensions
{
    internal static class PixelFormatExtensions
    {
        public static bool IsBlockCompressed(this PixelFormat format)
        {
            switch (format)
            {
                case PixelFormat.BC1_Rgb_UNorm:
                case PixelFormat.BC1_Rgba_UNorm:
                case PixelFormat.BC1_Rgba_UNorm_SRgb:
                case PixelFormat.BC1_Rgb_UNorm_SRgb:
                case PixelFormat.BC2_UNorm:
                case PixelFormat.BC2_UNorm_SRgb:
                case PixelFormat.BC3_UNorm:
                case PixelFormat.BC3_UNorm_SRgb:
                case PixelFormat.BC4_UNorm:
                case PixelFormat.BC4_SNorm:
                case PixelFormat.BC5_UNorm:
                case PixelFormat.BC5_SNorm:
                    return true;
            }

            return false;
        }

        public static uint GetBlockDivisor(this PixelFormat format)
        {
            if(format.IsBlockCompressed())
            {
                return 4;
            }

            return 1;
        }
    }
}
