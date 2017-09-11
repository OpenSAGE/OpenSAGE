using LLGfx;
using OpenSage.Data;
using OpenSage.Data.Map;

namespace OpenSage.DataViewer.ViewModels
{
    public sealed class MapFileContentViewModel : FileContentViewModel
    {
        private readonly MapFile _mapFile;

        public MapFileContentViewModel(FileSystemEntry file)
            : base(file)
        {
            _mapFile = MapFile.FromFileSystemEntry(file);
        }

        public void Initialize(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {

        }

        public void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
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

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }
    }
}
