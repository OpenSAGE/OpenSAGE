using OpenSage.LowLevel.Graphics3D;

namespace OpenSage.LowLevel.Graphics2D
{
    public sealed partial class GraphicsDevice2D : DisposableBase
    {
        public GraphicsDevice2D(GraphicsDevice graphicsDevice)
        {
            PlatformConstruct(graphicsDevice);
        }
    }
}
