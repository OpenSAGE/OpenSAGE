using System.Collections.Generic;
using System.Numerics;
using LLGfx;
using OpenSage.Data;
using OpenSage.Data.Map;
using OpenSage.DataViewer.Framework;
using OpenSage.Mathematics;
using OpenSage.Terrain;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class MapFileContentViewModel : FileContentViewModel
    {
        private readonly MapFile _mapFile;
        private Map _map;

        private DepthStencilBuffer _depthStencilBuffer;

        private Matrix4x4 _projectionMatrix;
        private Viewport _viewport;

        public MapCamera Camera { get; }

        public IEnumerable<TimeOfDay> TimesOfDay
        {
            get
            {
                yield return TimeOfDay.Morning;
                yield return TimeOfDay.Afternoon;
                yield return TimeOfDay.Evening;
                yield return TimeOfDay.Night;
            }
        }

        public TimeOfDay CurrentTimeOfDay
        {
            get { return _map?.CurrentTimeOfDay ?? TimeOfDay.Morning; }
            set
            {
                _map.CurrentTimeOfDay = value;
                NotifyOfPropertyChange();
            }
        }

        public bool RenderWireframeOverlay
        {
            get { return _map?.RenderWireframeOverlay ?? false; }
            set
            {
                _map.RenderWireframeOverlay = value;
                NotifyOfPropertyChange();
            }
        }

        private string _mousePosition;
        public string MousePosition
        {
            get => _mousePosition;
            set
            {
                _mousePosition = value;
                NotifyOfPropertyChange();
            }
        }

        public MapFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            _mapFile = MapFile.FromFileSystemEntry(file);

            Camera = new MapCamera();

            Camera.Reset(
                new Vector3(_mapFile.HeightMapData.Width * 10 / 2, _mapFile.HeightMapData.Height * 10 / 2, 0));
        }

        public void OnMouseMove(int x, int y)
        {
            if (_map == null)
            {
                return;
            }

            var view = Camera.ViewMatrix;
            var world = Matrix4x4.Identity;

            var pos1 = _viewport.Unproject(new Vector3(x, y, 0), ref _projectionMatrix, ref view, ref world);
            var pos2 = _viewport.Unproject(new Vector3(x, y, 1), ref _projectionMatrix, ref view, ref world);
            var dir = Vector3.Normalize(pos2 - pos1);

            var ray = new Ray(pos1, dir);

            var intersectionPoint = _map.Terrain.Intersect(ray);

            MousePosition = intersectionPoint?.ToString() ?? "No intersection";
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

            _depthStencilBuffer = new DepthStencilBuffer(
                graphicsDevice,
                swapChain.BackBufferWidth,
                swapChain.BackBufferHeight);

            _viewport = new Viewport(
                0, 
                0, 
                swapChain.BackBufferWidth, 
                swapChain.BackBufferHeight);

            _projectionMatrix = Matrix4x4.CreatePerspectiveFieldOfView(
                (float) (90 * System.Math.PI / 180),
                swapChain.BackBufferWidth / (float) swapChain.BackBufferHeight,
                0.1f,
                5000.0f);
        }

        public void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            _map = AddDisposable(new Map(
                _mapFile, 
                File.FileSystem,
                graphicsDevice));

            NotifyOfPropertyChange(nameof(CurrentTimeOfDay));
            NotifyOfPropertyChange(nameof(RenderWireframeOverlay));
        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            var renderPassDescriptor = new RenderPassDescriptor();
            renderPassDescriptor.SetRenderTargetDescriptor(
                swapChain.GetNextRenderTarget(),
                LoadAction.Clear,
                new ColorRgba(0.5f, 0.5f, 0.5f, 1));

            EnsureDepthStencilBuffer(graphicsDevice, swapChain);
            renderPassDescriptor.SetDepthStencilDescriptor(_depthStencilBuffer);

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            commandEncoder.SetViewport(_viewport);

            var cameraPosition = Camera.Position;
            var view = Camera.ViewMatrix;

            _map.Draw(commandEncoder, ref cameraPosition, ref view, ref _projectionMatrix);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }
    }
}
