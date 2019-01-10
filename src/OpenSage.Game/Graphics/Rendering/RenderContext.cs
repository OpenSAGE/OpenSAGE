using OpenSage.Content;
using Veldrid;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderContext
    {
        public ContentManager ContentManager { get; internal set; }
        public GraphicsDevice GraphicsDevice { get; internal set; }

        public Scene3D Scene3D { get; set; }
        public Scene2D Scene2D { get; set; }
        public Framebuffer RenderTarget { get; set; }

        public GameTime GameTime { get; set; }
    }
}
