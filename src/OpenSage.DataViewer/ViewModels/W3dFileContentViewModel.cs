using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Numerics;
using LLGfx;
using OpenSage.Data;
using OpenSage.Data.W3d;
using OpenSage.DataViewer.Framework;
using OpenSage.Graphics;
using OpenSage.Mathematics;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class W3dFileContentViewModel : FileContentViewModel
    {
        private readonly W3dFile _w3dFile;

        private DepthStencilBuffer _depthStencilBuffer;

        private ModelRenderer _modelRenderer;
        private Model _model;

        private List<Animation> _externalAnimations;

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private double _lastUpdate;

        private Vector3 _cameraPosition;

        public ArcballCamera Camera { get; }

        private Matrix4x4 _world, _view, _projection;

        private List<W3dItemViewModelBase> _modelChildren;
        public IReadOnlyList<W3dItemViewModelBase> ModelChildren
        {
            get
            {
                if (_modelChildren == null)
                {
                    _modelChildren = new List<W3dItemViewModelBase>();

                    if (_model != null)
                    {
                        _modelChildren.Add(new ModelViewModel(_model));

                        foreach (var animation in _model.Animations)
                        {
                            _modelChildren.Add(new AnimationViewModel(_model, animation, "Animations"));
                        }

                        foreach (var animation in _externalAnimations)
                        {
                            _modelChildren.Add(new AnimationViewModel(_model, animation, "External Animations"));
                        }

                        foreach (var mesh in _model.Meshes)
                        {
                            _modelChildren.Add(new ModelMeshViewModel(mesh));
                        }
                    }
                }

                return _modelChildren;
            }
        }

        private W3dItemViewModelBase _selectedModelChild;
        public W3dItemViewModelBase SelectedModelChild
        {
            get { return _selectedModelChild; }
            set
            {
                _selectedModelChild?.Deactivate();

                _selectedModelChild = value;

                _selectedModelChild.Activate();

                Camera.Reset(
                    value.BoundingSphere.Center,
                    value.BoundingSphere.Radius * 1.4f);

                _lastUpdate = GetTimeNow();

                NotifyOfPropertyChange();
            }
        }

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

            Camera = new ArcballCamera();
        }

        public void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            // TODO: Handle output panel resize.
            _depthStencilBuffer = new DepthStencilBuffer(
                graphicsDevice, 
                (int) swapChain.BackBufferWidth, 
                (int) swapChain.BackBufferHeight);

            _modelRenderer = new ModelRenderer(graphicsDevice, swapChain);

            _model = _modelRenderer.LoadModel(_w3dFile, File.FileSystem, graphicsDevice);

            _modelChildren = null;
            NotifyOfPropertyChange(nameof(ModelChildren));
            SelectedModelChild = ModelChildren[0];

            _stopwatch.Start();
            _lastUpdate = GetTimeNow();
        }

        private double GetTimeNow()
        {
            return _stopwatch.ElapsedMilliseconds;
        }

        private float GetDeltaTime()
        {
            var now = GetTimeNow();
            var deltaTime = now - _lastUpdate;
            _lastUpdate = now;
            return (float) deltaTime;
        }

        private void Update(SwapChain swapChain)
        {
            var deltaTime = GetDeltaTime();

            _world = Matrix4x4.Identity;

            _cameraPosition = Camera.Position;
            _view = Camera.ViewMatrix;

            _projection = Matrix4x4.CreatePerspectiveFieldOfView(
                (float) (90 * System.Math.PI / 180),
                (float) (swapChain.BackBufferWidth / swapChain.BackBufferHeight),
                0.1f,
                1000.0f);

            _selectedModelChild?.Update(TimeSpan.FromMilliseconds(deltaTime));
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            Update(swapChain);

            var renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.SetRenderTargetDescriptor(
                swapChain.GetNextRenderTarget(),
                LoadAction.Clear,
                new ColorRgba(0.5f, 0.5f, 0.5f, 1));

            renderPassDescriptor.SetDepthStencilDescriptor(_depthStencilBuffer);

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            commandEncoder.SetViewport(new Viewport
            {
                X = 0,
                Y = 0,
                Width = (int) swapChain.BackBufferWidth,
                Height = (int) swapChain.BackBufferHeight,
                MinDepth = 0,
                MaxDepth = 1
            });

            _modelRenderer.PreDrawModels(
                commandEncoder, 
                ref _cameraPosition);

            _model.PreDraw(commandEncoder);

            _selectedModelChild?.Draw(commandEncoder, ref _world, ref _view, ref _projection);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }
    }

    public abstract class W3dItemViewModelBase
    {
        public abstract string GroupName { get; }
        public abstract string Name { get; }
        public abstract BoundingSphere BoundingSphere { get; }

        public virtual void Activate() { }
        public virtual void Deactivate() { }

        public virtual void Update(TimeSpan deltaTime) { }

        public abstract void Draw(
            CommandEncoder commandEncoder,
            ref Matrix4x4 world,
            ref Matrix4x4 view,
            ref Matrix4x4 projection);
    }

    public sealed class ModelViewModel : W3dItemViewModelBase
    {
        private readonly Model _model;

        public override string GroupName => string.Empty;

        public override string Name => "Hierarchy";

        public override BoundingSphere BoundingSphere => _model.BoundingSphere;

        public ModelViewModel(Model model)
        {
            _model = model;
        }

        public override void Draw(
            CommandEncoder commandEncoder, 
            ref Matrix4x4 world, 
            ref Matrix4x4 view, 
            ref Matrix4x4 projection)
        {
            _model.Draw(commandEncoder, ref world, ref view, ref projection);
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
            ref Matrix4x4 world,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
        {
            _mesh.SetMatrices(ref world, ref view, ref projection);

            _mesh.Draw(commandEncoder, false);
            _mesh.Draw(commandEncoder, true);
        }
    }

    public sealed class AnimationViewModel : W3dItemViewModelBase
    {
        private readonly Model _model;
        private readonly Animation _animation;

        private readonly AnimationPlayer _animationPlayer;

        public override string GroupName { get; }

        public override string Name => _animation.Name;

        public override BoundingSphere BoundingSphere => _model.BoundingSphere;

        public AnimationViewModel(Model model, Animation animation, string groupName)
        {
            _model = model;
            _animation = animation;

            _animationPlayer = new AnimationPlayer(_animation, _model);

            GroupName = groupName;
        }

        public override void Activate()
        {
            _animationPlayer.Start();
        }

        public override void Update(TimeSpan deltaTime)
        {
            _animationPlayer.Update(deltaTime);
        }

        public override void Draw(
            CommandEncoder commandEncoder,
            ref Matrix4x4 world,
            ref Matrix4x4 view,
            ref Matrix4x4 projection)
        {
            _model.Draw(commandEncoder, ref world, ref view, ref projection);
        }
    }
}
