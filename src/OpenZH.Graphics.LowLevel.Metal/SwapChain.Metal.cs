using Metal;
using MetalKit;

namespace OpenZH.Graphics.LowLevel
{
    partial class SwapChain
    {
        private readonly GraphicsDevice _graphicsDevice;
        private readonly MTKView _metalView;

        private PixelFormat PlatformBackBufferFormat => PixelFormat.Bgra8UNorm;

        private double PlatformBackBufferWidth => _metalView.DrawableSize.Width;
        private double PlatformBackBufferHeight => _metalView.DrawableSize.Height;

        internal IMTLDrawable CurrentDrawable => _metalView.CurrentDrawable;

        public SwapChain(GraphicsDevice graphicsDevice, MTKView metalView)
        {
            _graphicsDevice = graphicsDevice;
            _metalView = metalView;
        }

        private RenderTarget PlatformGetNextRenderTarget()
        {
            return new RenderTarget(_graphicsDevice, _metalView.CurrentDrawable.Texture);
        }
    }
}