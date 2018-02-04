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

        public Scene Scene { get; set; }
        public CameraComponent Camera { get; set; }

        public Framebuffer RenderTarget { get; set; }

        public GameTime GameTime { get; set; }

        //public IReadOnlyList<RenderableComponent> Renderables { get; private set; }

        //public Action<RenderEffectPassCollection, RenderableComponent> EffectParametersCallback { get; set; }
        //public Stack<string> PermutationDefines { get; private set; }
        //public RenderPass CurrentPass;

        public RenderContext()
        {
            //PermutationDefines = new Stack<string>();
            //Renderables = new List<RenderableComponent>();
        }
    }
}
