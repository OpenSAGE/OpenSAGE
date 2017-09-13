using System.Numerics;
using LLGfx;
using OpenSage.Data;
using OpenSage.Data.Map;
using OpenSage.DataViewer.Framework;
using OpenSage.Terrain;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class MapFileContentViewModel : FileContentViewModel
    {
        private readonly MapFile _mapFile;
        private Map _map;

        private DepthStencilBuffer _depthStencilBuffer;

        public MapCamera Camera { get; }

        public MapFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            _mapFile = MapFile.FromFileSystemEntry(file);

            Camera = new MapCamera();

            Camera.Reset(
                new Vector3(_mapFile.HeightMapData.Width * 10 / 2, 0, -_mapFile.HeightMapData.Height * 10 / 2));
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
        }

        public void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            _map = AddDisposable(new Map(
                _mapFile, 
                File.FileSystem,
                graphicsDevice));
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

            commandEncoder.SetViewport(new Viewport
            {
                X = 0,
                Y = 0,
                Width = swapChain.BackBufferWidth,
                Height = swapChain.BackBufferHeight,
                MinDepth = 0,
                MaxDepth = 1
            });

            var cameraPosition = Camera.Position;
            var view = Camera.ViewMatrix;
            var projection = Matrix4x4.CreatePerspectiveFieldOfView(
                (float) (90 * System.Math.PI / 180),
                swapChain.BackBufferWidth / (float) swapChain.BackBufferHeight,
                0.1f,
                5000.0f);

            _map.Draw(commandEncoder, ref cameraPosition, ref view, ref projection);

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }
    }
}
