using System.Collections.Generic;
using System.Diagnostics;
using System.Numerics;
using LLGfx;
using OpenSage.Data;
using OpenSage.Data.W3d;
using OpenSage.Graphics;
using OpenSage.Graphics.Util;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class W3dFileContentViewModel : FileContentViewModel
    {
        private readonly W3dFile _w3dFile;

        private ModelRenderer _modelRenderer;
        private Model _model;

        private Vector3 _cameraTarget;
        private Vector3 _cameraPosition;

        private readonly Stopwatch _stopwatch = new Stopwatch();
        private double _lastUpdate;

        public IEnumerable<object> ModelChildren => _model?.Meshes;

        private object _selectedModelChild;
        public object SelectedModelChild
        {
            get { return _selectedModelChild; }
            set
            {
                _selectedModelChild = value;

                switch (value)
                {
                    case ModelMesh mm:
                        _cameraTarget = mm.BoundingSphereCenter.ToVector3();
                        _cameraPosition = _cameraTarget + new Vector3(0, mm.BoundingSphereRadius / 2, mm.BoundingSphereRadius + 5);
                        break;
                }

                NotifyOfPropertyChange();
            }
        }

        public W3dFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            using (var fileStream = file.Open())
                _w3dFile = W3dFile.FromStream(fileStream);
        }

        public void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            _modelRenderer = new ModelRenderer(graphicsDevice, swapChain);

            _model = _modelRenderer.LoadModel(_w3dFile, File.FileSystem, graphicsDevice);

            NotifyOfPropertyChange(nameof(ModelChildren));

            _stopwatch.Start();
            _lastUpdate = 0;
        }

        private void Update(SwapChain swapChain)
        {
            var now = _stopwatch.ElapsedMilliseconds * 0.0001;
            var updateTime = now - _lastUpdate;
            _lastUpdate = now;

            var world = Matrix4x4.CreateRotationY((float) _lastUpdate, _cameraTarget);

            var view = Matrix4x4.CreateLookAt(
                _cameraPosition,
                _cameraTarget,
                Vector3.UnitY);

            var projection = Matrix4x4.CreatePerspectiveFieldOfView(
                (float) (90 * System.Math.PI / 180),
                (float) (swapChain.BackBufferWidth / swapChain.BackBufferHeight),
                0.1f,
                1000.0f);

            foreach (var mesh in _model.Meshes)
            {
                mesh.SetMatrices(ref world, ref view, ref projection);
            }
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            Update(swapChain);

            var renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.SetRenderTargetDescriptor(
                swapChain.GetNextRenderTarget(),
                LoadAction.Clear,
                new ColorRgba(0.5f, 0.5f, 0.5f, 1));

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

            foreach (var mesh in _model.Meshes)
            {
                if (mesh == _selectedModelChild)
                {
                    mesh.Draw(commandEncoder);
                }
            }

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }
    }
}
