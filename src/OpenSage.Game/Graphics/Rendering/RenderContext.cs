using OpenSage.Graphics.Cameras;
using Veldrid;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderContext
    {
        public Game Game { get; internal set; }
        public GraphicsDevice GraphicsDevice { get; internal set; }
        public GraphicsSystem Graphics { get; internal set; }

        public CommandList CommandEncoder { get; internal set; }

        public Scene3D Scene { get; set; }
        public PerspectiveCamera Camera { get; set; }

        public Framebuffer RenderTarget { get; set; }

        public GameTime GameTime { get; set; }
    }
}
