namespace OpenSage.Data.Ani
{
    public sealed class CursorImage
    {
        public readonly uint Width;
        public readonly uint Height;

        public readonly uint HotspotX;
        public readonly uint HotspotY;

        public readonly byte[] PixelsBgra;

        internal CursorImage(
            uint width, uint height,
            uint hotspotX, uint hotspotY,
            byte[] pixelsBgra)
        {
            Width = width;
            Height = height;

            HotspotX = hotspotX;
            HotspotY = hotspotY;

            PixelsBgra = pixelsBgra;
        }
    }
}
