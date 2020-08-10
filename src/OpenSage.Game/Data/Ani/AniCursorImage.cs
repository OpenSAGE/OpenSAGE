namespace OpenSage.Data.Ani
{
    public sealed class AniCursorImage
    {
        public readonly byte[] PixelsBgra;

        internal AniCursorImage(byte[] pixelsBgra)
        {
            PixelsBgra = pixelsBgra;
        }
    }
}
