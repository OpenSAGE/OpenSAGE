using OpenSage.Graphics.Rendering;
using Veldrid;

namespace OpenSage.Graphics
{
    public sealed class GraphicsSystem : GameSystem
    {
        private readonly RenderContext _renderContext;

        private RenderPipeline _renderPipeline;

        public Texture ShadowMap => _renderPipeline.ShadowMap;

        public GraphicsSystem(Game game)
            : base(game)
        {
            _renderContext = new RenderContext();
        }

        public override void Initialize()
        {
            _renderPipeline = AddDisposable(new RenderPipeline(Game));
        }

        internal void Draw(Framebuffer framebuffer, in GameTime gameTime)
        {
            _renderContext.Game = Game;
            _renderContext.GraphicsDevice = Game.GraphicsDevice;
            _renderContext.Graphics = this;
            _renderContext.Camera = Game.Scene3D?.Camera;
            _renderContext.Scene = Game.Scene3D;
            _renderContext.RenderTarget = framebuffer;
            _renderContext.GameTime = gameTime;

            _renderPipeline.Execute(_renderContext);
        }
    }
}
