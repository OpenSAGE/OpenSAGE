using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class GraphicsSystem : GameSystem
    {
        private readonly RenderContext _renderContext;

        internal RenderPipeline RenderPipeline { get; private set; }

        public Texture ShadowMap => RenderPipeline.ShadowMap;
        public Texture ReflectionMap => RenderPipeline.ReflectionMap;
        public Texture RefractionMap => RenderPipeline.RefractionMap;

        public GraphicsSystem(Game game)
            : base(game)
        {
            _renderContext = new RenderContext();
        }

        public override void Initialize()
        {
            RenderPipeline = AddDisposable(new RenderPipeline(Game));
        }

        internal void Draw(in TimeInterval gameTime)
        {
            _renderContext.ContentManager = Game.ContentManager;
            _renderContext.GraphicsDevice = Game.GraphicsDevice;
            _renderContext.Scene3D = Game.Scene3D;
            _renderContext.Scene2D = Game.Scene2D;
            _renderContext.RenderTarget = Game.Panel.Framebuffer;
            _renderContext.GameTime = gameTime;

            RenderPipeline.Execute(_renderContext);
        }
    }
}
