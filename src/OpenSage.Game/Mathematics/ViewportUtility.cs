using Veldrid;

namespace OpenSage.Mathematics
{
    internal static class ViewportUtility
    {
        public static Rectangle Bounds(this Viewport viewport)
        {
            return new Rectangle(
                (int) viewport.X,
                (int) viewport.Y,
                (int) viewport.Width,
                (int) viewport.Height);
        }
    }
}
