using LL.Graphics3D;
using OpenSage;

namespace LL.Graphics2D
{
    public sealed partial class RenderTarget : DisposableBase
    {
        private readonly Texture _texture;

        public Texture Texture => _texture;

        public RenderTarget(
            GraphicsDevice graphicsDevice,
            GraphicsDevice2D graphicsDevice2D,
            int width,
            int height)
        {
            PlatformConstruct(
                graphicsDevice,
                graphicsDevice2D,
                width,
                height,
                out _texture);
        }

        public DrawingContext Open()
        {
            return new DrawingContext(this);
        }
    }
}
