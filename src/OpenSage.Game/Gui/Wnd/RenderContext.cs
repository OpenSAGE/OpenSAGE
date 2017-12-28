using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.Gui.Wnd
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
