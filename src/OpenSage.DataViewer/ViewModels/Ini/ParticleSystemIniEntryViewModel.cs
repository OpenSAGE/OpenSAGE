using System.Numerics;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data.Ini;
using OpenSage.DataViewer.Framework;
using OpenSage.Graphics;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.DataViewer.ViewModels.Ini
{
    public sealed class ParticleSystemIniEntryViewModel : FileSubObjectViewModel, IRenderableViewModel
    {
        private readonly ParticleSystemDefinition _definition;

        private readonly ParticleSystemManager _particleSystemManager;
        private readonly ContentManager _contentManager;

        private ParticleSystem _particleSystem;

        private readonly GameTimer _gameTimer;

        private readonly Camera _camera;

        public ArcballCameraController CameraController { get; }

        CameraController IRenderableViewModel.CameraController => CameraController;

        void IRenderableViewModel.OnMouseMove(int x, int y) { }

        public override string GroupName => "Particle Systems";

        public override string Name => _definition.Name;

        public ParticleSystemIniEntryViewModel(
            ParticleSystemDefinition definition,
            ContentManager contentManager)
        {
            _definition = definition;

            _particleSystemManager = AddDisposable(new ParticleSystemManager(contentManager.GraphicsDevice));

            _contentManager = contentManager;

            _camera = new Camera();
            _camera.FieldOfView = 70;

            CameraController = new ArcballCameraController(_camera);
            CameraController.Reset(Vector3.Zero, 200);

            _gameTimer = new GameTimer();
            _gameTimer.Start();
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain, RenderPassDescriptor renderPassDescriptor)
        {
            if (_particleSystem == null)
                return;

            _gameTimer.Update();

            _particleSystemManager.Update(_gameTimer.CurrentGameTime);

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            _camera.Viewport = new Viewport(
                0,
                0,
                swapChain.BackBufferWidth,
                swapChain.BackBufferHeight);

            commandEncoder.SetViewport(_camera.Viewport);

            _particleSystemManager.Draw(commandEncoder, _camera);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }

        public override void Activate()
        {
            _particleSystem = new ParticleSystem(_definition, _contentManager, () => Matrix4x4.Identity);
            _particleSystemManager.Add(_particleSystem);

            _gameTimer.Reset();
        }

        public override void Deactivate()
        {
            _particleSystemManager.Remove(_particleSystem);
            _particleSystem.Dispose();
            _particleSystem = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (_particleSystem != null)
            {
                Deactivate();
            }

            base.Dispose(disposing);
        }
    }
}
