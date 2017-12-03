using LL.Graphics3D;

namespace OpenSage.Mathematics
{
    internal static class ViewportUtility
    {
        public static Rectangle Bounds(this Viewport viewport)
        {
            return new Rectangle(viewport.X, viewport.Y, viewport.Width, viewport.Height);
        }
    }
}
