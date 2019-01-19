using OpenSage.Content;
using Veldrid;

namespace OpenSage.Graphics.Rendering
{
    public sealed class RenderContext
    {
        public ContentManager ContentManager;
        public GraphicsDevice GraphicsDevice;

        public Scene3D Scene3D;
        public Scene2D Scene2D;
        public Framebuffer RenderTarget;

        public GameTime GameTime;
    }
}
