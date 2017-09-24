using System.Numerics;
using Caliburn.Micro;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.Ini;
using OpenSage.DataViewer.Framework;
using OpenSage.Graphics;
using OpenSage.Graphics.ParticleSystems;

namespace OpenSage.DataViewer.ViewModels.Ini
{
    public sealed class ParticleSystemIniEntryViewModel : FileSubObjectViewModel
    {
        private readonly ParticleSystemDefinition _definition;

        private readonly ParticleSystemManager _particleSystemManager;
        private readonly ContentManager _contentManager;

        private ParticleSystem _particleSystem;

        private DepthStencilBuffer _depthStencilBuffer;

        private readonly Camera _camera;

        public ArcballCameraController CameraController { get; }

        public override string GroupName => "Particle Systems";

        public override string Name => _definition.Name;

        public ParticleSystemIniEntryViewModel(
            ParticleSystemDefinition definition,
            ParticleSystemManager particleSystemManager,
            ContentManager contentManager)
        {
            _definition = definition;

            _particleSystemManager = particleSystemManager;

            _contentManager = contentManager;

            _camera = new Camera();
            _camera.FieldOfView = 70;

            CameraController = new ArcballCameraController(_camera);
            CameraController.Reset(Vector3.Zero, 30);
        }

        private void EnsureDepthStencilBuffer(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            if (_depthStencilBuffer != null
                && _depthStencilBuffer.Width == swapChain.BackBufferWidth
                && _depthStencilBuffer.Height == swapChain.BackBufferHeight)
            {
                return;
            }

            if (_depthStencilBuffer != null)
            {
                _depthStencilBuffer.Dispose();
                _depthStencilBuffer = null;
            }

            // TODO: This is not disposed.
            _depthStencilBuffer = new DepthStencilBuffer(
                graphicsDevice,
                swapChain.BackBufferWidth,
                swapChain.BackBufferHeight);

            _camera.Viewport = new Viewport(
                0,
                0,
                swapChain.BackBufferWidth,
                swapChain.BackBufferHeight);
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            if (_particleSystem == null)
                return;

            _particleSystemManager.Update();

            var renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.SetRenderTargetDescriptor(
                swapChain.GetNextRenderTarget(),
                LoadAction.Clear,
                new ColorRgba(0.5f, 0.5f, 0.5f, 1));

            EnsureDepthStencilBuffer(graphicsDevice, swapChain);

            renderPassDescriptor.SetDepthStencilDescriptor(_depthStencilBuffer);

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            commandEncoder.SetViewport(_camera.Viewport);

            _particleSystemManager.Draw(commandEncoder, _camera);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }

        public override void Activate()
        {
            _particleSystem = new ParticleSystem(_definition, _contentManager);
            _particleSystemManager.Add(_particleSystem);
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
