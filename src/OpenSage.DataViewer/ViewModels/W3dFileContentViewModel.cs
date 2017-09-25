using System.Collections.Generic;
using System.IO;
using System.Numerics;
using Caliburn.Micro;
using LLGfx;
using OpenSage.Content;
using OpenSage.Data;
using OpenSage.Data.W3d;
using OpenSage.DataViewer.Framework;
using OpenSage.Graphics;
using OpenSage.Graphics.Effects;
using OpenSage.Mathematics;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class W3dFileContentViewModel : FileContentViewModel<W3dItemViewModelBase>, IRenderableViewModel
    {
        private readonly W3dFile _w3dFile;

        private readonly MeshEffect _meshEffect;
        private readonly ModelInstance _modelInstance;

        private readonly List<Animation> _externalAnimations;

        private readonly GameTimer _gameTimer;

        private readonly Camera _camera;

        public ArcballCameraController CameraController { get; }

        CameraController IRenderableViewModel.CameraController => CameraController;

        void IRenderableViewModel.OnMouseMove(int x, int y) { }

        public W3dFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            _w3dFile = W3dFile.FromFileSystemEntry(file);

            // If this is a skin file, load "external" animations.
            _externalAnimations = new List<Animation>();
            if (_w3dFile.HLod != null && _w3dFile.HLod.Header.Name.EndsWith("_SKN"))
            {
                var namePrefix = _w3dFile.HLod.Header.Name.Substring(0, _w3dFile.HLod.Header.Name.LastIndexOf('_') + 1);
                var parentFolder = Path.GetDirectoryName(_w3dFile.FilePath);
                var pathPrefix = Path.Combine(parentFolder, namePrefix);
                foreach (var animationFileEntry in file.FileSystem.GetFiles(parentFolder))
                {
                    if (!animationFileEntry.FilePath.StartsWith(pathPrefix))
                    {
                        continue;
                    }

                    var animationFile = W3dFile.FromFileSystemEntry(animationFileEntry);
                    foreach (var w3dAnimation in animationFile.Animations)
                    {
                        _externalAnimations.Add(new Animation(w3dAnimation));
                    }
                    foreach (var w3dAnimation in animationFile.CompressedAnimations)
                    {
                        _externalAnimations.Add(new Animation(w3dAnimation));
                    }
                }
            }

            _camera = new Camera();
            _camera.FieldOfView = 70;

            CameraController = new ArcballCameraController(_camera);

            _gameTimer = new GameTimer();
            _gameTimer.Start();

            var graphicsDevice = IoC.Get<GraphicsDeviceManager>().GraphicsDevice;

            var contentManager = AddDisposable(new ContentManager(File.FileSystem, graphicsDevice));

            _meshEffect = AddDisposable(new MeshEffect(graphicsDevice));

            _modelInstance = AddDisposable(new ModelInstance(contentManager.Load<Model>(File.FilePath, uploadBatch: null), graphicsDevice));
        }

        protected override IReadOnlyList<W3dItemViewModelBase> CreateSubObjects()
        {
            var result = new List<W3dItemViewModelBase>();

            if (_modelInstance.Model.HasHierarchy)
            {
                result.Add(new ModelViewModel(_modelInstance));

                foreach (var animation in _modelInstance.Model.Animations)
                {
                    result.Add(new AnimationViewModel(_modelInstance, animation, "Animations"));
                }

                foreach (var animation in _externalAnimations)
                {
                    result.Add(new AnimationViewModel(_modelInstance, animation, "External Animations"));
                }
            }

            foreach (var mesh in _modelInstance.Model.Meshes)
            {
                result.Add(new ModelMeshViewModel(mesh));
            }

            return result;
        }

        protected override void OnSelectedSubObjectChanged(W3dItemViewModelBase subObject)
        {
            if (subObject != null)
            {
                CameraController.Reset(
                    subObject.BoundingSphere.Center,
                    subObject.BoundingSphere.Radius * 1.6f);
            }

            _gameTimer.Reset();
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain, RenderPassDescriptor renderPassDescriptor)
        {
            _gameTimer.Update();

            SelectedSubObject?.Update(_gameTimer.CurrentGameTime);

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            _camera.Viewport = new Viewport(
                0,
                0,
                swapChain.BackBufferWidth,
                swapChain.BackBufferHeight);

            commandEncoder.SetViewport(_camera.Viewport);

            _meshEffect.Begin(commandEncoder);

            var lights = new Lights();
            lights.Light0.Ambient = new Vector3(0.3f, 0.3f, 0.3f);
            lights.Light0.Direction = Vector3.Normalize(new Vector3(-0.3f, 0.2f, -0.8f));
            lights.Light0.Color = new Vector3(0.7f, 0.7f, 0.8f);

            _meshEffect.SetLights(ref lights);

            var world = Matrix4x4.Identity;

            SelectedSubObject?.Draw(
                commandEncoder, 
                _meshEffect,
                _camera,
                ref world);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }
    }

    public abstract class W3dItemViewModelBase : FileSubObjectViewModel
    {
        public abstract BoundingSphere BoundingSphere { get; }

        public virtual void Update(GameTime gameTime) { }

        public abstract void Draw(
            CommandEncoder commandEncoder,
            MeshEffect meshEffect,
            Camera camera,
            ref Matrix4x4 world);
    }

    public sealed class ModelViewModel : W3dItemViewModelBase
    {
        private readonly ModelInstance _modelInstance;

        public override string GroupName => string.Empty;

        public override string Name => "Hierarchy";

        public override BoundingSphere BoundingSphere => _modelInstance.Model.BoundingSphere;

        public ModelViewModel(ModelInstance modelInstance)
        {
            _modelInstance = modelInstance;
        }

        public override void Draw(
            CommandEncoder commandEncoder,
            MeshEffect meshEffect,
            Camera camera,
            ref Matrix4x4 world)
        {
            _modelInstance.Draw(
                commandEncoder, 
                meshEffect,
                camera,
                ref world);
        }
    }

    public sealed class ModelMeshViewModel : W3dItemViewModelBase
    {
        private readonly ModelMesh _mesh;

        public override string GroupName => "Meshes";

        public override string Name => _mesh.Name;

        public override BoundingSphere BoundingSphere => _mesh.BoundingSphere;

        public ModelMeshViewModel(ModelMesh mesh)
        {
            _mesh = mesh;
        }

        public override void Draw(
            CommandEncoder commandEncoder,
            MeshEffect meshEffect,
            Camera camera,
            ref Matrix4x4 world)
        {
            _mesh.Draw(
                commandEncoder,
                meshEffect,
                camera,
                ref world,
                false);

            _mesh.Draw(
                commandEncoder,
                meshEffect,
                camera,
                ref world,
                true);
        }
    }

    public sealed class AnimationViewModel : W3dItemViewModelBase
    {
        private readonly ModelInstance _modelInstance;
        private readonly Animation _animation;

        private readonly AnimationPlayer _animationPlayer;

        public override string GroupName { get; }

        public override string Name => _animation.Name;

        public override BoundingSphere BoundingSphere => _modelInstance.Model.BoundingSphere;

        public AnimationViewModel(ModelInstance modelInstance, Animation animation, string groupName)
        {
            _modelInstance = modelInstance;
            _animation = animation;

            _animationPlayer = new AnimationPlayer(_animation, _modelInstance);

            GroupName = groupName;
        }

        public override void Activate()
        {
            _animationPlayer.Start();
        }

        public override void Update(GameTime gameTime)
        {
            _animationPlayer.Update(gameTime);
        }

        public override void Draw(
            CommandEncoder commandEncoder,
            MeshEffect meshEffect,
            Camera camera,
            ref Matrix4x4 world)
        {
            _modelInstance.Draw(
                commandEncoder, 
                meshEffect,
                camera,
                ref world);
        }
    }
}
