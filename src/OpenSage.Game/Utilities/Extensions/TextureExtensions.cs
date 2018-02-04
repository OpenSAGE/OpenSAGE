using Veldrid;

namespace OpenSage.Utilities.Extensions
{
    // TODO: See if this can go upstream to Veldrid.
    internal static class TextureExtensions
    {
        public static uint CalculateMipMapWidth(this Texture texture, uint mipLevel) => CalculateMipMapSize(texture, mipLevel, texture.Width);
        public static uint CalculateMipMapHeight(this Texture texture, uint mipLevel) => CalculateMipMapSize(texture, mipLevel, texture.Height);

        private static uint CalculateMipMapSize(Texture texture, uint mipLevel, uint baseSize)
        {
            baseSize = baseSize >> (int) mipLevel;

            // TODO_VELDRID: BC1, BC2
            var minSize = texture.Format == PixelFormat.BC3_UNorm
                ? 4u
                : 1u;

            return baseSize > 0
                ? baseSize
                : minSize;
        }
    }
}
