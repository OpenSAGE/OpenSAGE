using System.Collections.Generic;
using System.Linq;
using System.Numerics;
using LLGfx;
using OpenSage.Data.Ini;
using OpenSage.DataViewer.Framework;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Logic.Object;
using OpenSage.Terrain;

namespace OpenSage.DataViewer.ViewModels.Ini
{
    public sealed class ObjectDefinitionIniEntryViewModel : FileSubObjectViewModel, IRenderableViewModel
    {
        private readonly ObjectDefinition _definition;

        private readonly GameContext _gameContext;

        private Thing _thing;

        private readonly GameTimer _gameTimer;

        private readonly Camera _camera;

        public ArcballCameraController CameraController { get; }

        CameraController IRenderableViewModel.CameraController => CameraController;

        void IRenderableViewModel.OnMouseMove(int x, int y) { }

        public override string GroupName => "Object Definitions";

        public override string Name => _definition.Name;

        public IEnumerable<BitArray<ModelConditionFlag>> ModelConditionStates { get; private set; }

        public BitArray<ModelConditionFlag> SelectedModelConditionState
        {
            get { return _thing?.ModelCondition; }
            set
            {
                _thing.ModelCondition = value;
                NotifyOfPropertyChange();
            }
        }

        public ObjectDefinitionIniEntryViewModel(
            ObjectDefinition definition,
            GameContext gameContext)
        {
            _definition = definition;

            _gameContext = gameContext;

            _camera = new Camera();
            _camera.FieldOfView = 70;

            CameraController = new ArcballCameraController(_camera);
            CameraController.Reset(Vector3.Zero, 300);

            _gameTimer = AddDisposable(new GameTimer());
            _gameTimer.Start();
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain, RenderPassDescriptor renderPassDescriptor)
        {
            if (_thing == null)
                return;

            _gameTimer.Update();

            _thing.Update(_gameTimer.CurrentGameTime);

            //_gameContext.ParticleSystemManager.Update(_gameTimer.CurrentGameTime);

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            _camera.Viewport = new Viewport(
                0,
                0,
                swapChain.BackBufferWidth,
                swapChain.BackBufferHeight);

            commandEncoder.SetViewport(_camera.Viewport);

            _gameContext.MeshEffect.Begin(commandEncoder);

            var lights = new Lights();
            lights.Light0.Ambient = new Vector3(0.3f, 0.3f, 0.3f);
            lights.Light0.Direction = Vector3.Normalize(new Vector3(-0.3f, 0.2f, -0.8f));
            lights.Light0.Color = new Vector3(0.7f, 0.7f, 0.8f);

            _gameContext.MeshEffect.SetLights(ref lights);

            _thing.Draw(commandEncoder, _gameContext.MeshEffect, _camera, _gameTimer.CurrentGameTime);

            //_gameContext.ParticleSystemManager.Draw(commandEncoder, _camera);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }

        public override void Activate()
        {
            var uploadBatch = new ResourceUploadBatch(_gameContext.GraphicsDevice);
            uploadBatch.Begin();

            _thing = new Thing(
                _definition,
                new Vector3(10, 10, 0),
                0.7f,
                _gameContext,
                uploadBatch);

            uploadBatch.End();

            _gameTimer.Reset();

            ModelConditionStates = _thing.ModelConditionStates.ToList();
            NotifyOfPropertyChange(nameof(ModelConditionStates));
            SelectedModelConditionState = ModelConditionStates.FirstOrDefault();
        }

        public override void Deactivate()
        {
            _thing.Dispose();
            _thing = null;
        }

        protected override void Dispose(bool disposing)
        {
            if (_thing != null)
            {
                Deactivate();
            }

            base.Dispose(disposing);
        }
    }
}
