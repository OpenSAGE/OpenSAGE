using System.Collections.Generic;
using OpenSage.Graphics.Cameras;
using OpenSage.Graphics.Rendering;

namespace OpenSage.Graphics
{
    public sealed class GraphicsSystem : GameSystem
    {
        private readonly RenderContext _renderContext;

        private readonly List<MeshComponent> _meshes;

        private readonly CameraInputMessageHandler _cameraInputMessageHandler;
        private CameraInputState _cameraInputState;

        private RenderPipeline _renderPipeline;

        public GraphicsSystem(Game game)
            : base(game)
        {
            RegisterComponentList(_meshes = new List<MeshComponent>());

            _renderContext = new RenderContext();

            _cameraInputMessageHandler = new CameraInputMessageHandler();
        }

        public override void Initialize()
        {
            _renderPipeline = new RenderPipeline(Game.GraphicsDevice);

            Game.Input.MessageBuffer.Handlers.Add(_cameraInputMessageHandler);

            base.Initialize();
        }

        internal override void BuildRenderList(RenderList renderList)
        {
            foreach (var renderable in _meshes)
            {
                renderable.BuildRenderList(renderList);
            }
        }

        public override void Draw(GameTime gameTime)
        {
            // TODO: Do this in Update?
            _cameraInputMessageHandler.UpdateInputState(ref _cameraInputState);
            Game.Scene.CameraController.UpdateCamera(Game.Scene.Camera, _cameraInputState, gameTime);

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
