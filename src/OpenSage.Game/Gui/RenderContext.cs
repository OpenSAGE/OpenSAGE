using LL.Graphics2D;
using LL.Graphics3D;

namespace OpenSage.Gui
{
    public sealed class RenderContext
    {
        public GraphicsDevice GraphicsDevice { get; }
        public GraphicsDevice2D GraphicsDevice2D { get; }

        public RenderContext(
            GraphicsDevice graphicsDevice,
            GraphicsDevice2D graphicsDevice2D)
        {
            GraphicsDevice = graphicsDevice;
            GraphicsDevice2D = graphicsDevice2D;
        }
    }
}
