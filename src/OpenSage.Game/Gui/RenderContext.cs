using LL.Graphics3D;

namespace OpenSage.Gui
{
    public sealed class RenderContext
    {
        public GraphicsDevice GraphicsDevice { get; }

        public RenderContext(GraphicsDevice graphicsDevice)
        {
            GraphicsDevice = graphicsDevice;
        }
    }
}
