using LLGfx;

namespace OpenSage.Graphics.Rendering
{
    internal sealed class RenderPipeline : DisposableBase
    {
        private readonly DepthStencilBufferCache _depthStencilBufferCache;

        public RenderPipeline(GraphicsDevice graphicsDevice)
        {
            _depthStencilBufferCache = AddDisposable(new DepthStencilBufferCache(graphicsDevice));
        }

        public void Execute(RenderContext context)
        {
            var renderPassDescriptor = new RenderPassDescriptor();

            var clearColor = context.Camera.BackgroundColor.ToColorRgba();

            renderPassDescriptor.SetRenderTargetDescriptor(
                context.RenderTarget,
                LoadAction.Clear,
                clearColor);

            var depthStencilBuffer = _depthStencilBufferCache.Get(
                context.SwapChain.BackBufferWidth,
                context.SwapChain.BackBufferHeight);

            renderPassDescriptor.SetDepthStencilDescriptor(depthStencilBuffer);

            var commandBuffer = context.GraphicsDevice.CommandQueue.GetCommandBuffer();

            var commandEncoder = commandBuffer.GetCommandEncoder(renderPassDescriptor);

            commandEncoder.SetViewport(context.Camera.Viewport);

            var renderables = context.Graphics.Renderables;

            foreach (var renderable in renderables)
            {
                renderable.Render(commandEncoder);
            }

            commandEncoder.Close();

            commandBuffer.CommitAndPresent(context.SwapChain);
        }
    }
}
