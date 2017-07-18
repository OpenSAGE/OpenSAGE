using System;
using System.IO;
using OpenZH.Graphics;

namespace OpenZH.DataViewer.Controls
{
    public sealed class DdsView : RenderedView
    {
        public Func<Stream> OpenStream { get; set; }

        public override void Initialize(GraphicsDevice graphicsDevice)
        {
            // Load texture.
        }

        public override void Draw(GraphicsDevice graphicsDevice, SwapChain swapChain)
        {
            var renderPassDescriptor = graphicsDevice.CreateRenderPassDescriptor();
            renderPassDescriptor.SetRenderTargetDescriptor(
                swapChain.GetNextRenderTarget(),
                LoadAction.Clear,
                new ColorRgba(1, 0, 0, 1));

            var commandBuffer = graphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            // TODO

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(swapChain);
        }
    }
}
