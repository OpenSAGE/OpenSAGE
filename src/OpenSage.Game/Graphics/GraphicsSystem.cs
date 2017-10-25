using System.Collections.Generic;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Graphics
{
    public sealed class GraphicsSystem : GameSystem
    {
        private readonly RenderContext _renderContext;
        private readonly List<ModelComponent> _models;

        private RenderPipeline _renderPipeline;

        internal readonly RenderList RenderList = new RenderList();

        public GraphicsSystem(Game game)
            : base(game)
        {
            RegisterComponentList(_models = new List<ModelComponent>());

            RenderList = new RenderList();

            _renderContext = new RenderContext();
        }

        public override void Initialize()
        {
            _renderPipeline = new RenderPipeline(Game.GraphicsDevice);

            base.Initialize();
        }

        internal override void OnEntityComponentAdded(EntityComponent component)
        {
            base.OnEntityComponentAdded(component);

            if (component is RenderableComponent r)
            {
                r.BuildRenderList(RenderList);
            }
        }

        internal override void OnEntityComponentRemoved(EntityComponent component)
        {
            base.OnEntityComponentRemoved(component);

            if (component is RenderableComponent r)
            {
                RenderList.RemoveRenderable(r);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // TODO: Do this in Update?
            Game.Scene.CameraController.UpdateCamera(Game.Input, gameTime);

            _renderContext.Game = Game;
            _renderContext.GraphicsDevice = Game.GraphicsDevice;
            _renderContext.Graphics = this;
            _renderContext.Camera = Game.Scene.Camera;
            _renderContext.Scene = Game.Scene;
            _renderContext.SwapChain = Game.SwapChain;
            _renderContext.RenderTarget = Game.SwapChain.GetNextRenderTarget();
            _renderContext.GameTime = gameTime;

            _renderPipeline.Execute(_renderContext);

            base.Draw(gameTime);
        }

        protected override void Dispose(bool disposeManagedResources)
        {
            _renderPipeline.Dispose();
            _renderPipeline = null;

            base.Dispose(disposeManagedResources);
        }
    }
}
